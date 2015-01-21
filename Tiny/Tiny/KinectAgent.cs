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
        public class Dimension
        {
            private int depthFrameWidth;
            private int depthFrameHeight;

            public int DepthFrameWidth
            {
                get
                {
                    return this.depthFrameWidth;
                }
            }

            public int DepthFrameHeight
            {
                get
                {
                    return this.depthFrameHeight;
                }
            }

            public Dimension(int depthFrameWidth, int depthFrameHeight)
            {
                this.depthFrameWidth = depthFrameWidth;
                this.depthFrameHeight = depthFrameHeight;
            }
        }

        private bool calibrated;
        private KinectAgent.Dimension dimension;
        private Dictionary<ulong, Person> people;

        private Stack<SBodyFrame> unprocessedBodyFrames;

        private SingleKinectUI kinectUI;
        public event KinectBodyFrameHandler UpdateKinectUI;
        public delegate void KinectBodyFrameHandler(SBodyFrame bodyFrame);
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public KinectAgent.Dimension FrameDimension
        {
            get
            {
                return this.dimension;
            }
        }

        public IEnumerable<Person> People
        {
            get
            {
                foreach (Person person in this.people.Values)
                {
                    yield return Person.Copy(person);
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

        public KinectAgent()
        {
            this.calibrated = false;
            this.dimension = null;
            this.people = new Dictionary<ulong, Person>();

            this.unprocessedBodyFrames = new Stack<SBodyFrame>();

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

                Person person = new Person(trackingId);

                person.InitialAngle = WBody.GetInitialAngle(frameZeroth.Bodies[personIdx]);

                // initial position = average of all previous positions
                // TODO: May break if bodies count differ from the first frame
                SBody[] previousPositions = new SBody[calibrationFrames.Length];
                for (int frameIdx = 0; frameIdx < calibrationFrames.Length; frameIdx++)
                {
                    previousPositions[frameIdx] = calibrationFrames[frameIdx].Bodies[personIdx];
                }
                person.InitialPosition = WBody.GetInitialPosition(previousPositions);

                this.people.Add(trackingId, person);
            }
            this.calibrated = true;
            this.dimension = new KinectAgent.Dimension(frameZeroth.DepthFrameWidth, frameZeroth.DepthFrameHeight);
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
                    if (this.people.ContainsKey(body.TrackingId))
                    {
                        Person person = this.people[body.TrackingId];
                        WBody worldBody = WBody.Create(body, person.InitialAngle, person.InitialPosition);
                        person.UpdatePosition(body, worldBody);
                    }
                    // ignore bodies that do not match with any tracking id
                }
            }
            this.UpdateKinectUI(bodyFrame);
        }
    }
}
