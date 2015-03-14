using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Threading;
using KinectSerializer;
using KinectMultiTrack.Properties;
using KinectMultiTrack.UI;
using KinectMultiTrack.WorldView;

namespace KinectMultiTrack
{
    public class KinectClient
    {
        public class Specification
        {
            public int DepthFrameWidth { get; set; }
            public int DepthFrameHeight { get; set; }
            public double Height { get; set; }
            public double TiltAngle { get; set; }
        }
        public uint Id { get; private set; }
        public IPEndPoint IP { get; private set; }
        public bool Calibrated { get; private set; }
        public KinectClient.Specification CameraSpecification { get; private set; }
        private readonly List<TrackingSkeleton> skeletonsList;
        private readonly Stack<SBodyFrame> calibrationFrames;

        public IEnumerable<TrackingSkeleton> Skeletons
        {
            get
            {
                return this.skeletonsList;
            }
        }

        public int UncalibratedFramesCount
        {
            get
            {
                return this.calibrationFrames.Count;
            }
        }

        public KinectClient(uint id, IPEndPoint ip, double height, double tiltAngle)
        {
            this.Id = id;
            this.IP = ip;
            this.Calibrated = false;
            this.CameraSpecification = new Specification();
            this.CameraSpecification.Height = height;
            this.CameraSpecification.TiltAngle = tiltAngle;
            this.skeletonsList = new List<TrackingSkeleton>();
            this.calibrationFrames = new Stack<SBodyFrame>();
        }

        public void AddCalibrationFrame(SBodyFrame frame)
        {
            this.calibrationFrames.Push(frame);
        }

        public void UpdateFrame(SBodyFrame frame)
        {
            List<ulong> unclaimedTrackedIds = new List<ulong>();
            List<TrackingSkeleton> unclaimedTrackedSkeletons = new List<TrackingSkeleton>();
            foreach (SBody body in frame.Bodies)
            {
                ulong trackingId = body.TrackingId;
                TrackingSkeleton skeleton = this.skeletonsList.Find(x => x.TrackingId == trackingId);
                if (skeleton != null)
                {
                    skeleton.UpdatePosition(frame.TimeStamp, body);
                }
                else
                {
                    unclaimedTrackedIds.Add(trackingId);
                    unclaimedTrackedSkeletons.Add(skeleton);
                }
            }

            // TODO: make it better
            foreach (TrackingSkeleton skeleton in unclaimedTrackedSkeletons)
            {
                if (unclaimedTrackedIds.Count > 0)
                {
                    ulong unclaimedId = unclaimedTrackedIds.First();
                    SBody unclaimedBody = frame.Bodies.Find(x => x.TrackingId == unclaimedId);
                    unclaimedTrackedIds.Remove(unclaimedId);
                    skeleton.UpdatePosition(frame.TimeStamp, unclaimedBody);
                }
            }
        }

        public void Calibrate()
        {
            SBodyFrame[] selectedFrames = new SBodyFrame[Tracker.MIN_CALIBRATION_FRAMES];
            // Add from back to front
            uint calibrationFramesToAdd = Tracker.MIN_CALIBRATION_FRAMES;
            while (calibrationFramesToAdd > 0)
            {
                selectedFrames[--calibrationFramesToAdd] = this.calibrationFrames.Pop();
            }

            // Use the last frame body as reference
            int frameNthIdx = selectedFrames.Length-1;
            SBodyFrame frameNth = selectedFrames[frameNthIdx];

            this.CameraSpecification.DepthFrameWidth = frameNth.DepthFrameWidth;
            this.CameraSpecification.DepthFrameHeight = frameNth.DepthFrameHeight;

            // Coordinate calibration
            for (int personIdx = 0; personIdx < frameNth.Bodies.Count; personIdx++)
            {
                SBody currentBody = selectedFrames[frameNthIdx].Bodies[personIdx];
                ulong trackingId = currentBody.TrackingId;
                long timestamp = frameNth.TimeStamp;
                
                // Initial angle
                double initAngle = WBody.GetInitialAngle(frameNth.Bodies[personIdx]);
                
                // initial center position = average of previous positions in frames where that person exists
                List<SBody> positionsList = new List<SBody>();
                for (int frameIdx = 0; frameIdx < selectedFrames.Length; frameIdx++)
                {
                    if (selectedFrames[frameIdx].Bodies.Count > personIdx)
                    {
                        positionsList.Add(selectedFrames[frameIdx].Bodies[personIdx]);
                    }
                }
                WCoordinate initCenterPos = WBody.GetInitialCenterPosition(positionsList);

                this.skeletonsList.Add(new TrackingSkeleton(currentBody, timestamp, initAngle, initCenterPos));
            }
            this.Calibrated = true;
        }

        public void PrepareRecalibration()
        {
            this.calibrationFrames.Clear();
            this.skeletonsList.Clear();
            this.Calibrated = false;
        }

        public static TrackerResult.KinectFOV ExtractFOVInfo(KinectClient kinect)
        {
            return new TrackerResult.KinectFOV(kinect.Id, kinect.IP, kinect.CameraSpecification);
        }
    }
}
