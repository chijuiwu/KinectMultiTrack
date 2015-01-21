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

namespace Tiny
{
    public class KinectAgent
    {
        private bool calibrated;
        private Dictionary<ulong, Person> trackedUsers;

        private Stack<SBodyFrame> unprocessedBodyFrames;
        private Stack<Tuple<SBodyFrame, WBodyFrame>> processedBodyFrames;

        private SingleKinectUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public SBodyFrame CurrentRawFrame
        {
            get
            {
                if (this.processedBodyFrames.Count > 0)
                {
                    return this.processedBodyFrames.Peek().Item1;
                }
                else if (this.unprocessedBodyFrames.Count > 0)
                {
                    return this.unprocessedBodyFrames.Peek();
                }
                else
                {
                    return null;
                }
            }
        }

        public WBodyFrame CurrentWorldviewFrame
        {
            get
            {
                if (this.processedBodyFrames.Count > 0)
                {
                    return this.processedBodyFrames.Peek().Item2;
                }
                else
                {
                    return null;
                }
            }
        }

        public KinectAgent()
        {
            this.calibrated = false;
            this.trackedUsers = new Dictionary<ulong, Person>();

            this.unprocessedBodyFrames = new Stack<SBodyFrame>();
            this.processedBodyFrames = new Stack<Tuple<SBodyFrame, WBodyFrame>>();

            Thread kinectUIThread = new Thread(new ThreadStart(this.StartKinectUIThread));
            kinectUIThread.SetApartmentState(ApartmentState.STA);
            kinectUIThread.Start();
        }

        private void StartKinectUIThread()
        {
            this.kinectUI = new SingleKinectUI();
            this.kinectUI.Show();
            this.UpdateKinectUI += this.kinectUI.UpdateBodyFrame;
            this.DisposeKinectUI += this.kinectUI.Dispose;
            Dispatcher.Run();
        }
        public void DisposeUI()
        {
            this.DisposeKinectUI();
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
                List<WBody> worldBodiesList = new List<WBody>();
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (this.trackedUsers.ContainsKey(body.TrackingId))
                    {
                        Person person = this.trackedUsers[body.TrackingId];
                        WBody worldBody = WBody.Create(body, person.InitialAngle, person.InitialPosition);
                        person.UpdatePosition(body, worldBody);
                        worldBodiesList.Add(worldBody);
                    }
                    // ignore bodies that do not match with any tracking id
                }
                WBodyFrame worldBodyFrame = new WBodyFrame(bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight, worldBodiesList);
                this.processedBodyFrames.Push(Tuple.Create(bodyFrame, worldBodyFrame));
            }
            this.UpdateKinectUI(bodyFrame);
        }

        public bool ReadyToCalibrate
        {
            get
            {
                return this.unprocessedBodyFrames.Count >= Tracker.CALIBRATION_FRAMES;
            }
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

                Person person = new Person();

                person.InitialAngle = WBody.GetInitialAngle(frameZeroth.Bodies[personIdx]);

                // initial position = average of all previous positions
                // TODO: May break if bodies count differ from the first frame
                SBody[] previousPositions = new SBody[calibrationFrames.Length];
                for (int frameIdx = 0; frameIdx < calibrationFrames.Length; frameIdx++)
                {
                    previousPositions[frameIdx] = calibrationFrames[frameIdx].Bodies[personIdx];
                }
                person.InitialPosition = WBody.GetInitialPosition(previousPositions);

                this.trackedUsers.Add(trackingId, person);
            }
            this.calibrated = true;
        }
    }
}
