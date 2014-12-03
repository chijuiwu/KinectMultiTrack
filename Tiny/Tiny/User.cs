using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net;
using System.Collections.Concurrent;
using System.Diagnostics;
using KinectSerializer;

namespace Tiny
{
    class User
    {
        private double initAngle;
        private WorldCoordinate initCentrePosition;
        private bool calibrationCompleted;

        // Unprocessed body frames, assume the frame order is perserved
        private ConcurrentQueue<SerializableBodyFrame> incomingBodyFrames;
        private ConcurrentQueue<SerializableBodyFrame> calibrationBodyFrames;
        private ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>> processedBodyFrames;

        public event DisplayKinectBodyFrameHandler DisplayKinectBodyFrame;
        public delegate void DisplayKinectBodyFrameHandler(SerializableBodyFrame bodyFrame);

        private KinectBodyViewer kinectBodyViwer;

        public User()
        {
            this.incomingBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.calibrationBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.processedBodyFrames = new ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>>();
            
            Thread bodyViewerThread = new Thread(new ThreadStart(this.StartKinectBodyViewerThread));
            bodyViewerThread.SetApartmentState(ApartmentState.STA);
            bodyViewerThread.Start();
        }

        private void StartKinectBodyViewerThread()
        {
            this.kinectBodyViwer = new KinectBodyViewer();
            this.kinectBodyViwer.Show();
            this.DisplayKinectBodyFrame += this.kinectBodyViwer.UpdateBodyFrame;
            Dispatcher.Run();
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

            // Kinect calibration
            // TODO: scale to multiple users in one frame
            if (this.ReadyToCalibrate())
            {
                SerializableBodyFrame firstCalibrationFrame;
                this.calibrationBodyFrames.TryPeek(out firstCalibrationFrame);
                this.initAngle = WorldView.GetInitialAngle(firstCalibrationFrame.Bodies[0]);
                SerializableBodyFrame[] calibrationFrames = this.calibrationBodyFrames.ToArray();
                SerializableBody[] calibrationBodies = new SerializableBody[calibrationFrames.Length];
                for (int i = 0; i < calibrationBodies.Length; i++)
                {
                    calibrationBodies[i] = calibrationFrames[i].Bodies[0];
                }
                this.initCentrePosition = WorldView.GetInitialCentrePosition(calibrationBodies);
                this.calibrationCompleted = true;
            }
            else if (!this.calibrationCompleted)
            {
                this.calibrationBodyFrames.Enqueue(nextKinectFrame);
            }
            else 
            {
                this.processedBodyFrames.Push(Tuple.Create(nextKinectFrame, new WorldView(nextKinectFrame, WorldView.GetTransformedBody(nextKinectFrame.Bodies[0], this.initAngle, this.initCentrePosition))));
            }

            if (this.DisplayKinectBodyFrame != null)
            {
                this.DisplayKinectBodyFrame(nextKinectFrame);
            }
            else
            {
                Debug.WriteLine("null");
            }
        }

        private bool ReadyToCalibrate()
        {
            return !this.calibrationCompleted && this.processedBodyFrames.Count == UserTracker.CALIBRATION_FRAMES;
        }

        public void CloseBodyViewer()
        {
            this.kinectBodyViwer.Dispatcher.Invoke((Action)(() =>
            {
                this.kinectBodyViwer.Close();
            }));
        }

        public ConcurrentQueue<SerializableBodyFrame> IncomingBodyFrames
        {
            get
            {
                return this.incomingBodyFrames;
            }
        }

        public ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>> ProcessedBodyFrames
        {
            get
            {
                return this.processedBodyFrames;
            }
        }

        public bool CalibrationCompleted
        {
            get
            {
                return this.calibrationCompleted;
            }
        }

        public SerializableBodyFrame LastKinectFrame
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

        public WorldView LastWorldView
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
