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
        private IPEndPoint clientIP;

        // Unprocessed body frames, assume the frame order is perserved
        private ConcurrentQueue<SerializableBodyFrame> incomingBodyFrames;

        private LinkedList<Tuple<SerializableBodyFrame, WorldView>> processedBodyFrames;

        private Thread bodyViewerThread;
        private KinectBodyViewer kinectBodyViwer;

        public User()
        {
            this.incomingBodyFrames = new ConcurrentQueue<SerializableBodyFrame>();
            this.processedBodyFrames = new LinkedList<Tuple<SerializableBodyFrame, WorldView>>();
            this.bodyViewerThread = new Thread(new ThreadStart(this.StartKinectBodyViewerThread));
        }

        private void StartKinectBodyViewerThread()
        {
            this.kinectBodyViwer = new KinectBodyViewer();
            this.kinectBodyViwer.Show();
            Dispatcher.Run();
        }

        public Tuple<SerializableBodyFrame, WorldView> GetLastFrame()
        {
            return this.processedBodyFrames.Last.Value;
        }

        public void ProcessBodyFrame()
        {
            // TODO should have one enqueueing thread and one dequeueing thread

            SerializableBodyFrame bodyFrame;
            if (!this.incomingBodyFrames.TryDequeue(out bodyFrame))
            {
                return;
            }
            Debug.WriteLine("Client: " + this.clientIP);
            Debug.WriteLine("Time stamp: " + bodyFrame.TimeStamp);

            WorldView bodyFrameInWorldView = new WorldView(bodyFrame);
            this.processedBodyFrames.AddLast(Tuple.Create(bodyFrame, bodyFrameInWorldView));

            this.kinectBodyViwer.Dispatcher.Invoke((Action)(() =>
            {
                this.kinectBodyViwer.DisplayBodyFrame(bodyFrame);
            }));
        }

        public void CloseBodyViewer()
        {
            this.kinectBodyViwer.Dispatcher.Invoke((Action)(() =>
            {
                this.kinectBodyViwer.Close();
            }));
        }

        public IPEndPoint ClientIP
        {
            get
            {
                return this.clientIP;
            }
        }

        public ConcurrentQueue<SerializableBodyFrame> IncomingBodyFrames
        {
            get
            {
                return this.incomingBodyFrames;
            }
        }

        public LinkedList<Tuple<SerializableBodyFrame, WorldCoordinate>> ProcessedBodyFrames
        {
            get
            {
                return this.processedBodyFrames;
            }
        }
    }
}
