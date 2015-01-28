using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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
        public class Dimension
        {
            public int DepthFrameWidth { get; private set; }
            public int DepthFrameHeight { get; private set; }

            public Dimension(int depthFrameWidth, int depthFrameHeight)
            {
                this.DepthFrameWidth = depthFrameWidth;
                this.DepthFrameHeight = depthFrameHeight;
            }
        }

        public KinectCamera.Dimension FrameDimension { get; private set; }
        private bool calibrated;
        private Dictionary<ulong, TrackingSkeleton> skeletons;

        private Stack<SBodyFrame> unprocessedBodyFrames;

        private KinectStreamUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public IEnumerable<TrackingSkeleton> Skeletons
        {
            get
            {
                foreach (TrackingSkeleton skeleton in this.skeletons.Values)
                {
                    if (skeleton.Positions.Count > 0)
                    {
                        yield return TrackingSkeleton.Copy(skeleton);
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

        public KinectCamera(string ip)
        {
            this.calibrated = false;
            this.FrameDimension = null;
            this.skeletons = new Dictionary<ulong, TrackingSkeleton>();

            this.unprocessedBodyFrames = new Stack<SBodyFrame>();

            Thread kinectUIThread = new Thread(new ParameterizedThreadStart(this.StartKinectUIThread));
            kinectUIThread.SetApartmentState(ApartmentState.STA);
            kinectUIThread.Start(ip);
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
            this.DisposeKinectUI();
        }

        public void Calibrate()
        {
            SBodyFrame[] calibrationFrames = new SBodyFrame[Tracker.CALIBRATION_FRAMES];
            int calibrationFramesCount = 0;
            while (calibrationFramesCount < Tracker.CALIBRATION_FRAMES)
            {
                calibrationFrames[calibrationFramesCount++] = this.unprocessedBodyFrames.Pop();
            }
            this.unprocessedBodyFrames.Clear();

            SBodyFrame frameZeroth = calibrationFrames[0];
            for (int personIdx = 0; personIdx < frameZeroth.Bodies.Count; personIdx++)
            {
                ulong trackingId = frameZeroth.Bodies[personIdx].TrackingId;
                TrackingSkeleton skeleton = new TrackingSkeleton(trackingId);
                skeleton.InitialAngle = WBody.GetInitialAngle(frameZeroth.Bodies[personIdx]);
                // initial position = average of all previous positions
                // TODO: May break if bodies count differ from the first frame
                SBody[] previousPositions = new SBody[calibrationFrames.Length];
                for (int frameIdx = 0; frameIdx < calibrationFrames.Length; frameIdx++)
                {
                    previousPositions[frameIdx] = calibrationFrames[frameIdx].Bodies[personIdx];
                }
                skeleton.InitialPosition = WBody.GetInitialPosition(previousPositions);
                this.skeletons[trackingId] = skeleton;
            }
            this.calibrated = true;
            this.FrameDimension = new KinectCamera.Dimension(frameZeroth.DepthFrameWidth, frameZeroth.DepthFrameHeight);
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
                        TrackingSkeleton skeleton = this.skeletons[body.TrackingId];
                        WBody worldBody = WBody.Create(body, skeleton.InitialAngle, skeleton.InitialPosition);
                        skeleton.UpdatePosition(body, worldBody);
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
