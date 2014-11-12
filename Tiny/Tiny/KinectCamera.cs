using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KinectSerializer;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Windows.Threading;

namespace Tiny
{
    class KinectCamera
    {
        private IPEndPoint clientIP;
        public IPEndPoint ClientIP
        {
            get
            {
                return this.clientIP;
            }
        }
        private KinectBodyViewer kinectBodyViwer;
        private SerializableBodyFrame currentBodyFrame;
        public SerializableBodyFrame CurrentBodyFrame
        {
            get
            {
                return this.currentBodyFrame;
            }
        }
        // not sure
        private ConcurrentQueue<SerializableBodyFrame> bodyFramesQueue;

        public KinectCamera(IPEndPoint clientIP, SerializableBodyFrame currentBodyFrame)
        {
            this.clientIP = clientIP;
            Thread bodyViewerThread = new Thread(new ThreadStart(this.StartKinectBodyViewerThread));
            bodyViewerThread.SetApartmentState(ApartmentState.STA);
            bodyViewerThread.Start();
            this.currentBodyFrame = currentBodyFrame;
            this.bodyFramesQueue = new ConcurrentQueue<SerializableBodyFrame>();
        }

        private void StartKinectBodyViewerThread()
        {
            this.kinectBodyViwer = new KinectBodyViewer();
            this.kinectBodyViwer.Show();
            Dispatcher.Run();
        }

        public void UpdateBodyFrame(SerializableBodyFrame bodyFrame)
        {
            this.currentBodyFrame = bodyFrame;
            //this.bodyFramesQueue.Enqueue(bodyFrame);
            Debug.WriteLine("Client: " + this.clientIP);
            Debug.WriteLine("Time stamp: " + bodyFrame.TimeStamp);
            List<SerializableBody> bodies = bodyFrame.Bodies;
            foreach (SerializableBody body in bodies)
            {
                Debug.WriteLine("Trackeding ID: " + body.TrackingId);
                SerializableJoint head = body.Joints[JointType.Head];
                CameraSpacePoint headCameraPoint = head.CameraSpacePoint;
                DepthSpacePoint headDepthPoint = head.DepthSpacePoint;
                Debug.WriteLine("Head CameraPt: " + headCameraPoint.X + ", " + headCameraPoint.Y + ", " + headCameraPoint.Z);
                Debug.WriteLine("Head DepthPt: " + headDepthPoint.X + ", " + headDepthPoint.Y);
            }
            this.kinectBodyViwer.Dispatcher.Invoke((Action)(() =>
            {
                this.kinectBodyViwer.DisplayBodyFrame(bodyFrame);
            }));
        }
    }
}
