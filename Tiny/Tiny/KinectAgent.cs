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
        private int depthFrameWidth;
        private int depthFrameHeight;

        private bool calibrated;
        private Dictionary<ulong, Person> trackedUsers;

        private Stack<SBodyFrame> unprocessedBodyFrames;
        private Stack<Tuple<SBodyFrame, WorldBodyFrame>> processedBodyFrames;

        private SingleKinectUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public KinectAgent()
        {
            this.calibrated = false;
            this.trackedUsers = new Dictionary<ulong, Person>();

            this.unprocessedBodyFrames = new Stack<SBodyFrame>();
            this.processedBodyFrames = new Stack<Tuple<SBodyFrame, WorldBodyFrame>>();

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
                List<WorldBody> worldviewBodies = new List<WorldBody>();
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (this.trackedUsers.ContainsKey(body.TrackingId))
                    {
                        Person person = this.trackedUsers[body.TrackingId];
                        worldviewBodies.Add(WorldBody.Create(body, person.InitialAngle, person.InitialPosition));
                    }
                    // ignore bodies that do not match with any tracking id
                }
                this.processedBodyFrames.Push(Tuple.Create(bodyFrame, new WorldBodyFrame(bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight, worldviewBodies)));
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

            for (int personIdx = 0; personIdx < calibrationFrames[0].Bodies.Count; personIdx++)
            {
                Person person = new Person();

                person.TrackingId = calibrationFrames[0].Bodies[personIdx].TrackingId;

                person.InitialAngle = WorldBody.GetInitialAngle(calibrationFrames[0].Bodies[personIdx]);

                // initial position = average of all previous positions
                SBody[] previousPositions = new SBody[calibrationFrames.Length];
                for (int frameIdx = 0; frameIdx < previousPositions.Length; frameIdx++)
                {
                    previousPositions[frameIdx] = calibrationFrames[frameIdx].Bodies[personIdx];
                }
                person.InitialPosition = WorldBody.GetInitialPosition(previousPositions);

                this.trackedUsers.Add(person.TrackingId, person);
            }
            this.depthFrameWidth = calibrationFrames[0].DepthFrameWidth;
            this.depthFrameHeight = calibrationFrames[0].DepthFrameHeight;
            this.calibrated = true;
        }

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

        public WorldBodyFrame CurrentWorldviewFrame
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
    }
}
