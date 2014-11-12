using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using KinectSerializer;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Kinect;

namespace Tiny
{
    class KinectCamera
    {
        private IPEndPoint clientIP;
        private KinectBodyViewer kinectBodyViwer;
        // not sure
        private ConcurrentQueue<SerializableBodyFrame> bodyFramesQueue;

        public KinectCamera(IPEndPoint clientIP)
        {
            this.clientIP = clientIP;
            this.kinectBodyViwer = new KinectBodyViewer();
            this.kinectBodyViwer.Show();
            this.bodyFramesQueue = new ConcurrentQueue<SerializableBodyFrame>();
        }

        public void updateBodyFrame(SerializableBodyFrame bodyFrame)
        {
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
