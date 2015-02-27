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
    public class KinectCamera
    {
        public class Specification
        {
            public int DepthFrameWidth { get; private set; }
            public int DepthFrameHeight { get; private set; }
            public double Angle { get; private set; }
            public double Height { get; private set; }

            public Specification(int depthFrameWidth, int depthFrameHeight)
            {
                this.DepthFrameWidth = depthFrameWidth;
                this.DepthFrameHeight = depthFrameHeight;
                // TODO: Update these values
                this.Angle = 0.0;
                this.Height = 0.0;
            }
        }

        public uint Id { get; private set; }
        public KinectCamera.Specification CameraSpecification { get; private set; }
        private bool calibrated;
        private Dictionary<ulong, TSkeleton> skeletons;

        private Stack<SBodyFrame> unprocessedBodyFrames;

        private KinectStreamUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public IEnumerable<TSkeleton> Skeletons
        {
            get
            {
                foreach (TSkeleton skeleton in this.skeletons.Values)
                {
                    if (skeleton.Positions.Count > 0)
                    {
                        yield return TSkeleton.Copy(skeleton);
                    }
                }
            }
        }

        public int UnprocessedFramesCount
        {
            get
            {
                return this.unprocessedBodyFrames.Count;
            }
        }

        public KinectCamera(string ip, uint id)
        {
            this.Id = id;
            this.calibrated = false;
            this.CameraSpecification = null;
            this.skeletons = new Dictionary<ulong, TSkeleton>();

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
            SBodyFrame[] calibrationFrames = new SBodyFrame[Tracker.CALIBRATION_FRAMES];
            // add from back back the 
            int calibrationFramesToAdd = Tracker.CALIBRATION_FRAMES;
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
                TSkeleton skeleton = new TSkeleton(trackingId, frameNth.TimeStamp, initAngle, initPos);
                this.skeletons[trackingId] = skeleton;
            }
            this.calibrated = true;
            this.CameraSpecification = new KinectCamera.Specification(frame0th.DepthFrameWidth, frame0th.DepthFrameHeight);
        }

        public void ProcessFrames(SBodyFrame bodyFrame)
        {
            Debug.WriteLine(Resources.PROCESS_BODYFRAME + bodyFrame.TimeStamp);
            if (!this.calibrated)
            {
                this.unprocessedBodyFrames.Push(bodyFrame);
            }
            else
            {
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (this.skeletons.ContainsKey(body.TrackingId))
                    {
                        TSkeleton skeleton = this.skeletons[body.TrackingId];
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
    }
}
