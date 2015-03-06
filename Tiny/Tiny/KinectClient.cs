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
using Tiny.Properties;
using Tiny.UI;
using Tiny.WorldView;

namespace Tiny
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
        private readonly Dictionary<ulong, MovingSkeleton> movingSkeletonsDict;
        private readonly Stack<SBodyFrame> unprocessedBodyFrames;
        
        private KinectStreamUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public IEnumerable<MovingSkeleton> MovingSkeletons
        {
            get
            {
                foreach (MovingSkeleton skeleton in this.movingSkeletonsDict.Values)
                {
                    yield return skeleton;
                }
            }
        }

        public int UncalibratedFramesCount
        {
            get
            {
                return this.unprocessedBodyFrames.Count;
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
            this.movingSkeletonsDict = new Dictionary<ulong, MovingSkeleton>();
            this.unprocessedBodyFrames = new Stack<SBodyFrame>();

            //Commented because multi-threading issues
            //Thread kinectUIThread = new Thread(new ParameterizedThreadStart(this.StartKinectUIThread));
            //kinectUIThread.SetApartmentState(ApartmentState.STA);
            //kinectUIThread.Start(ip);
        }

        private void StartKinectUIThread(object obj)
        {
            this.kinectUI = new KinectStreamUI();
            this.kinectUI.Show();
            this.kinectUI.ClientIPLabel.Content = (obj as string);
            this.UpdateKinectUI += this.kinectUI.UpdateBodyFrame;
            this.DisposeKinectUI += this.kinectUI.Dispose;
            Dispatcher.Run();
        }

        public void DisposeUI()
        {
            if (this.kinectUI != null)
            {
                this.DisposeKinectUI();
            }
        }

        public void StoreFrame(SBodyFrame bodyFrame)
        {
            if (!this.Calibrated)
            {
                this.unprocessedBodyFrames.Push(bodyFrame);
            }
            else
            {
                foreach (SBody body in bodyFrame.Bodies)
                {
                    ulong trackingId = body.TrackingId;
                    if (this.movingSkeletonsDict.ContainsKey(trackingId))
                    {
                        this.movingSkeletonsDict[trackingId].UpdatePosition(bodyFrame.TimeStamp, body);
                    }
                    else
                    {
                        // TODO: Fire unknown skeleton detected
                    }
                }
            }
            if (this.UpdateKinectUI != null)
            {
                this.UpdateKinectUI(bodyFrame);
            }
        }

        public void Calibrate()
        {
            SBodyFrame[] calibrationFrames = new SBodyFrame[Tracker.MIN_CALIBRATION_FRAMES];
            // Add from back to front
            uint calibrationFramesToAdd = Tracker.MIN_CALIBRATION_FRAMES;
            while (calibrationFramesToAdd > 0)
            {
                calibrationFrames[--calibrationFramesToAdd] = this.unprocessedBodyFrames.Pop();
            }

            // Use the last frame body as reference
            int frameNthIdx = calibrationFrames.Length-1;
            SBodyFrame frameNth = calibrationFrames[frameNthIdx];

            this.CameraSpecification.DepthFrameWidth = frameNth.DepthFrameWidth;
            this.CameraSpecification.DepthFrameHeight = frameNth.DepthFrameHeight;

            // Coordinate calibration
            for (int personIdx = 0; personIdx < frameNth.Bodies.Count; personIdx++)
            {
                SBody currentBody = calibrationFrames[frameNthIdx].Bodies[personIdx];
                ulong trackingId = currentBody.TrackingId;
                long timestamp = frameNth.TimeStamp;
                
                // Initial angle
                double initAngle = WBody.GetInitialAngle(frameNth.Bodies[personIdx]);
                
                // initial center position = average of previous positions in frames where that person exists
                List<SBody> positionsList = new List<SBody>();
                for (int frameIdx = 0; frameIdx < calibrationFrames.Length; frameIdx++)
                {
                    if (calibrationFrames[frameIdx].Bodies.Count > personIdx)
                    {
                        positionsList.Add(calibrationFrames[frameIdx].Bodies[personIdx]);
                    }
                }
                WCoordinate initCenterPos = WBody.GetInitialCenterPosition(positionsList);

                MovingSkeleton skeleton = new MovingSkeleton(currentBody, timestamp, initAngle, initCenterPos);
                this.movingSkeletonsDict[trackingId] = skeleton;
            }

            this.Calibrated = true;
        }

        public static TrackerResult.KinectFOV ExtractFOVInfo(KinectClient kinect)
        {
            return new TrackerResult.KinectFOV(kinect.Id, kinect.IP, kinect.CameraSpecification);
        }
    }
}
