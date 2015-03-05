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
        public readonly uint Id { get; private set; }
        public readonly IPEndPoint IP { get; private set; }
        public bool Calibrated { get; private set; }
        public bool IsTracking { get; private set; }
        public readonly KinectClient.Specification CameraSpecification { get; private set; }
        private readonly Dictionary<ulong, MovingSkeleton> skeletonsDict;
        private readonly Stack<SBodyFrame> unprocessedBodyFrames;
        
        private KinectStreamUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public IEnumerable<MovingSkeleton> CurrentlyMovingSkeletons
        {
            get
            {
                foreach (MovingSkeleton skeleton in this.skeletonsDict.Values)
                {
                    yield return MovingSkeleton.CurrentCopy(skeleton);
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
            this.IsTracking = false;
            this.CameraSpecification = new Specification();
            this.CameraSpecification.Height = height;
            this.CameraSpecification.TiltAngle = tiltAngle;
            this.skeletonsDict = new Dictionary<ulong, MovingSkeleton>();
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

        public void Calibrate()
        {
            SBodyFrame[] calibrationFrames = new SBodyFrame[Tracker.MIN_CALIBRATION_FRAMES];
            // add from back back the 
            uint calibrationFramesToAdd = Tracker.MIN_CALIBRATION_FRAMES;
            while (calibrationFramesToAdd > 0)
            {
                calibrationFrames[--calibrationFramesToAdd] = this.unprocessedBodyFrames.Pop();
            }
            SBodyFrame frame0th = calibrationFrames[0];
            SBodyFrame frameNth = calibrationFrames[calibrationFrames.Length-1];
            for (int personIdx = 0; personIdx < frame0th.Bodies.Count; personIdx++)
            {
                ulong trackingId = frame0th.Bodies[personIdx].TrackingId;
                // Use 0th frame to get the initial angle
                double initAngle = WBody.GetInitialAngle(frame0th.Bodies[personIdx]);
                // initial position = average of previous positions in frames where that person exists
                List<SBody> previousPosList = new List<SBody>();
                for (int frameIdx = 0; frameIdx < calibrationFrames.Length; frameIdx++)
                {
                    if (calibrationFrames[frameIdx].Bodies.Count > personIdx)
                    {
                        previousPosList.Add(calibrationFrames[frameIdx].Bodies[personIdx]);
                    }
                }
                WCoordinate initPos = WBody.GetInitialPosition(previousPosList);
                MovingSkeleton skeleton = new MovingSkeleton(trackingId, frameNth.TimeStamp, initAngle, initPos);
                this.skeletonsDict[trackingId] = skeleton;
            }
            this.Calibrated = true;
            this.CameraSpecification.DepthFrameWidth = frame0th.DepthFrameWidth;
            this.CameraSpecification.DepthFrameHeight = frame0th.DepthFrameHeight;
        }

        public void ProcessFrames(SBodyFrame bodyFrame)
        {
            Debug.WriteLine(Resources.PROCESS_BODYFRAME + bodyFrame.TimeStamp);
            if (!this.Calibrated)
            {
                this.unprocessedBodyFrames.Push(bodyFrame);
            }
            else
            {
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (this.skeletonsDict.ContainsKey(body.TrackingId))
                    {
                        MovingSkeleton skeleton = this.skeletonsDict[body.TrackingId];
                        WBody worldBody = WBody.Create(body, skeleton.InitialAngle, skeleton.InitialPosition);
                        skeleton.UpdatePosition(bodyFrame.TimeStamp, body, worldBody);
                    }
                    // ignore bodies that do not match with any tracking id
                }
            }
            if (this.UpdateKinectUI != null)
            {
                this.UpdateKinectUI(bodyFrame);
            }
        }

        public static TrackerResult.KinectFOV ExtractFOVInfo(KinectClient kinect)
        {
            return new TrackerResult.KinectFOV(kinect.Id, kinect.IP, kinect.CameraSpecification);
        }
    }
}
