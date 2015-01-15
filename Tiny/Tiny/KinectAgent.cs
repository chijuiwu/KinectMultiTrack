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
        private List<Person> trackedUsers;

        // Unprocessed body frames, assume the frame order is perserved
        private Queue<SerializableBodyFrame> incomingBodyFrames;
        private Stack<SerializableBodyFrame> calibrationBodyFrames;
        private Stack<Tuple<SerializableBodyFrame, WorldView>> processedBodyFrames;
        public event KinectBodyFrameHandler UpdateKinectBodyFrame;
        public delegate void KinectBodyFrameHandler(SerializableBodyFrame bodyFrame);

        private SingleKinectUI kinectUI;
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public KinectAgent()
        {
            this.calibrated = false;
            this.trackedUsers = new List<Person>();

            this.incomingBodyFrames = new Queue<SerializableBodyFrame>();
            this.calibrationBodyFrames = new Stack<SerializableBodyFrame>();
            this.processedBodyFrames = new Stack<Tuple<SerializableBodyFrame, WorldView>>();
            
            Thread kinectUIThread = new Thread(new ThreadStart(this.StartKinectUIThread));
            kinectUIThread.SetApartmentState(ApartmentState.STA);
            kinectUIThread.Start();
        }

        private void StartKinectUIThread()
        {
            this.kinectUI = new SingleKinectUI();
            this.kinectUI.Show();
            this.UpdateKinectBodyFrame += this.kinectUI.UpdateBodyFrame;
            this.DisposeKinectUI += this.kinectUI.Dispose;
            Dispatcher.Run();
        }
        public void DisposeUI()
        {
            this.DisposeKinectUI();
        }

        public void AddFrame(SerializableBodyFrame bodyFrame)
        {
            this.incomingBodyFrames.Enqueue(bodyFrame);
        }

        public void ProcessFrames()
        {
            try
            {
                SerializableBodyFrame nextBodyFrame = this.incomingBodyFrames.Dequeue();
                Debug.WriteLine(Resources.PROCESS_BODYFRAME + nextBodyFrame.TimeStamp);

                if (this.calibrated)
                {
                    this.processedBodyFrames.Push(Tuple.Create(nextBodyFrame, new WorldView(WorldView.GetBodyWorldCoordinates(nextBodyFrame.Bodies[0], this.initAngle, this.initCentrePosition), this.initAngle, this.initCentrePosition, this.depthFrameWidth, this.depthFrameHeight)));
                }
                else if (nextBodyFrame.Bodies.Count > 0)
                {
                    this.calibrationBodyFrames.Push(nextBodyFrame);
                }

                this.UpdateKinectBodyFrame(nextBodyFrame);
            } catch (InvalidOperationException ignored)
            {
                return;
            }
        }

        public bool ReadyForCalibration
        {
            get
            {
                return this.calibrationBodyFrames.Count >= Tracker.CALIBRATION_FRAMES;
            }
        }

        public void Calibrate()
        {
            SerializableBodyFrame[] calibrationFrames = new SerializableBodyFrame[Tracker.CALIBRATION_FRAMES];
            int calibrationFramesCount = 0;
            while (calibrationFramesCount < Tracker.CALIBRATION_FRAMES)
            {
                calibrationFrames[calibrationFramesCount++] = this.calibrationBodyFrames.Pop();
            }

            for (int personIdx = 0; personIdx < calibrationFrames[0].Bodies.Count; personIdx++)
            {
                Person person = new Person();
                this.trackedUsers.Add(person);
                
                person.InitialAngle = WorldView.GetInitialAngle(calibrationFrames[0].Bodies[personIdx]);

                // initial position = average of all previous positions
                SerializableBody[] previousPositions = new SerializableBody[calibrationFrames.Length];
                for(int frameIdx = 0; frameIdx < previousPositions.Length; frameIdx++)
                {
                    previousPositions[frameIdx] = calibrationFrames[frameIdx].Bodies[personIdx];
                }
                person.InitialPosition = WorldView.GetInitialPosition(previousPositions);
            }
            this.depthFrameWidth = calibrationFrames[0].DepthFrameWidth;
            this.depthFrameHeight = calibrationFrames[0].DepthFrameHeight;
            this.calibrated = true;
        }

        public SerializableBodyFrame LatestRawFrame
        {
            get
            {
                if (this.processedBodyFrames.Count > 0)
                {
                    Tuple<SerializableBodyFrame, WorldView> lastKinectFrameTuple;
                    this.processedBodyFrames.TryPeek(out lastKinectFrameTuple);
                    return lastKinectFrameTuple.Item1;
                }
                else if (this.calibrationBodyFrames.Count > 0)
                {
                    SerializableBodyFrame lastKinectFrame;
                    this.calibrationBodyFrames.TryPeek(out lastKinectFrame);
                    return lastKinectFrame;
                }
                else if (this.incomingBodyFrames.Count > 0)
                {
                    SerializableBodyFrame lastKinectFrame;
                    this.incomingBodyFrames.TryPeek(out lastKinectFrame);
                    return lastKinectFrame;
                }
                else
                {
                    return null;
                }
            }
        }

        public WorldView LatestWorldviewFrame
        {
            get
            {
                if (this.processedBodyFrames.Count > 0)
                {

                    Tuple<SerializableBodyFrame, WorldView> lastKinectFrameTuple;
                    this.processedBodyFrames.TryPeek(out lastKinectFrameTuple);
                    return lastKinectFrameTuple.Item2;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
