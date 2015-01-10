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

namespace Tiny
{
    public class KinectProfile
    {
        private int depthFrameWidth;
        private int depthFrameHeight;

        // Unprocessed body frames, assume the frame order is perserved
        private ConcurrentQueue<SerializableBodyFrame> incomingBodyFrames;
        private ConcurrentStack<SerializableBodyFrame> calibrationBodyFrames;
        private ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>> processedBodyFrames;
        public event KinectBodyFrameHandler UpdateKinectBodyFrame;
        public delegate void KinectBodyFrameHandler(SerializableBodyFrame bodyFrame);

        private SingleKinectUI kinectUI;
        public event KinectUIHandler DisposeKinectUI;
        public delegate void KinectUIHandler();

        public KinectProfile()
        {
            this.incomingBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.calibrationBodyFrames = new ConcurrentStack<SerializableBodyFrame>();
            this.processedBodyFrames = new ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>>();
            
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

        public void AddFrame(SerializableBodyFrame bodyFrame)
        {
            this.incomingBodyFrames.Enqueue(bodyFrame);
        }

        public void DisposeUI()
        {
            this.DisposeKinectUI();
        }

        public void CalibrateKinect()
        {
            // TODO: scale to multiple bodies in one frame
            if (this.ReadyToCalibrate)
            {
                SerializableBodyFrame[] calibrationFrames = new SerializableBodyFrame[Tracker.CALIBRATION_FRAMES];
                int frameCount = 0;
                while (frameCount < Tracker.CALIBRATION_FRAMES)
                {
                    SerializableBodyFrame calibrationFrame;
                    this.calibrationBodyFrames.TryPop(out calibrationFrame);
                    calibrationFrames[frameCount++] = calibrationFrame;
                }
                // Get rid of rest of calibration frames
                //this.calibrationBodyFrames.Clear();

                SerializableBodyFrame firstCalibrationFrame = calibrationFrames[0];
                SerializableBody[] calibrationBodies = new SerializableBody[calibrationFrames.Length];
                for (int i = 0; i < calibrationBodies.Length; i++)
                {
                    calibrationBodies[i] = calibrationFrames[i].Bodies[0];
                }
                this.initAngle = WorldView.GetInitialAngle(firstCalibrationFrame.Bodies[0]);
                Debug.WriteLine("angle: " + this.initAngle);
                this.initCentrePosition = WorldView.GetInitialCentrePosition(calibrationBodies);
                this.depthFrameWidth = firstCalibrationFrame.DepthFrameWidth;
                this.depthFrameHeight = firstCalibrationFrame.DepthFrameHeight;
                this.calibrationCompleted = true;
            }
        }

        // TODO should have one enqueueing thread and one dequeueing thread
        public void ProcessBodyFrame()
        {
            SerializableBodyFrame nextKinectFrame;
            if (!this.incomingBodyFrames.TryDequeue(out nextKinectFrame))
            {
                return;
            }

            Debug.WriteLine("Processing bodyframe @ timestamp: " + nextKinectFrame.TimeStamp);

            if (this.CalibrationCompleted)
            {
                this.processedBodyFrames.Push(Tuple.Create(nextKinectFrame, new WorldView(WorldView.GetBodyWorldCoordinates(nextKinectFrame.Bodies[0], this.initAngle, this.initCentrePosition), this.initAngle, this.initCentrePosition, this.depthFrameWidth, this.depthFrameHeight)));
            }
            else
            {
                if (nextKinectFrame.Bodies.Count > 0)
                {
                    this.calibrationBodyFrames.Push(nextKinectFrame);
                }
            }

            if (this.UpdateKinectBodyFrame != null)
            {
                this.UpdateKinectBodyFrame(nextKinectFrame);
            }
        }

        public bool ReadyToCalibrate
        {
            get
            {
                return !this.calibrationCompleted && this.calibrationBodyFrames.Count >= Tracker.CALIBRATION_FRAMES;
            }
        }

        public SerializableBodyFrame LatestFrame
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

        public WorldView LatestWorldView
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
