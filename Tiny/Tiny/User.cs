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

        private Thread bodyViewerThread;
        private KinectBodyViewer kinectBodyViwer;

        public User()
        {
            this.incomingBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.calibrationBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.processedBodyFrames = new ConcurrentStack<Tuple<SerializableBodyFrame, WorldView>>();
            this.bodyViewerThread = new Thread(new ThreadStart(this.StartKinectBodyViewerThread));
            this.bodyViewerThread.SetApartmentState(ApartmentState.STA);
            this.bodyViewerThread.Start();
        }

        private void StartKinectBodyViewerThread()
        {
            this.kinectBodyViwer = new KinectBodyViewer();
            this.kinectBodyViwer.Show();
            Dispatcher.Run();
        }

        // TODO should have one enqueueing thread and one dequeueing thread
        public void ProcessBodyFrame()
        {
            SerializableBodyFrame bodyFrame;
            if (!this.incomingBodyFrames.TryDequeue(out bodyFrame))
            {
                return;
            }

            Debug.WriteLine("Processing bodyframe @ timestamp: " + bodyFrame.TimeStamp);

            if (this.ReadyToCalibrate())
            {
                // Kinect Calibration
                SerializableBodyFrame firstBodyFrame;
                this.calibrationBodyFrames.TryPeek(out firstBodyFrame);
                this.initAngle = WorldView.GetInitialAngle(firstBodyFrame.);
            }
            else if (!this.calibrationCompleted)
            {
                this.calibrationBodyFrames.Enqueue(bodyFrame);
            }
            else 
            {

            }

            WorldView bodyFrameInWorldView = new WorldView(bodyFrame);
            this.processedBodyFrames.Push(Tuple.Create(bodyFrame, bodyFrameInWorldView));

            // Display the next recent frame
            this.kinectBodyViwer.Dispatcher.Invoke((Action)(() =>
            {
                this.kinectBodyViwer.DisplayBodyFrame(bodyFrame);
            }));
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

        public Tuple<SerializableBodyFrame, WorldView> LastFrame
        {
            get
            {
                return this.processedBodyFrames.First();
            }
        }

        public bool CalibrationCompleted
        {
            get
            {
                return this.processedBodyFrames.Count == UserTracker.CALIBRATION_FRAMES;
            }
        }

    }
}
