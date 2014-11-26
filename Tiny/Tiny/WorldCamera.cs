using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectSerializer;
using System.Collections.Concurrent;
using System.Net;
using System.Diagnostics;
using Microsoft.Kinect;

namespace Tiny
{
    class WorldCamera
    {
        private ConcurrentDictionary<IPEndPoint, User> users;

        public IEnumerable<KeyValuePair<IPEndPoint, User>> Users
        {
            get
            {
                return this.users.AsEnumerable<KeyValuePair<IPEndPoint, User>>();
            }
        }

        public WorldCamera()
        {
            this.users = new ConcurrentDictionary<IPEndPoint, User>();
        }

        public void AddOrUpdateClient(IPEndPoint clientIP, SerializableBodyFrame bodyFrame)
        {
            if (!this.users.ContainsKey(clientIP))
            {
                this.users[clientIP] = new User();
            }
            this.users[clientIP].IncomingBodyFrames.Enqueue(bodyFrame);
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            User user;
            if (this.users.TryRemove(clientIP, out user))
            {
                user.CloseBodyViewer();
            }
        }

        public void SynchronizeFrames()
        {
            foreach (User user in this.users.Values)
            {
                user.ProcessBodyFrame();
            }
            //if (this.users.Count() <= 1) return;

            //KeyValuePair<IPEndPoint, User> firstUserEntry = this.users.First(entry => entry.Key.Address.ToString().StartsWith("127.0.0.1"));
            //IPEndPoint referenceUserIP = firstUserEntry.Key;
            //Debug.WriteLine("Reference User: " + referenceUserIP);
            //User referenceUser = firstUserEntry.Value;
            //SerializableBodyFrame referenceBodyFrame = referenceUser.WorldBodyFrame;
            //if (referenceBodyFrame.Bodies.Count() == 0) return;
            
            //// Assumes only one body
            //SerializableBody referenceBody = referenceBodyFrame.Bodies.First();
            //Dictionary<JointType, SerializableJoint> referenceJoints = referenceBody.Joints;
            //SerializableJoint referenceHead = referenceJoints[JointType.Head];
            //CameraSpacePoint referenceHeadCS = referenceHead.CameraSpacePoint;
            //SerializableJoint referenceShoulderLeft = referenceJoints[JointType.ShoulderLeft];
            //CameraSpacePoint referenceShoulderLeftCS = referenceShoulderLeft.CameraSpacePoint;
            //SerializableJoint referenceShoulderRight = referenceJoints[JointType.ShoulderRight];
            //CameraSpacePoint referenceShoulderRightCS = referenceShoulderRight.CameraSpacePoint;
            //foreach (IPEndPoint userIP in this.users.Keys)
            //{
            //    User user = this.users[userIP];
            //    // Translate the joint positions
            //    if (!user.WorldBodyFrame.Equals(referenceBodyFrame))
            //    {
            //        Debug.WriteLine("Transform user positiosn: " + userIP);

            //        if (user.WorldBodyFrame.Bodies.Count() == 0) return;

            //        // Assumes only one body
            //        SerializableBody userBody = user.WorldBodyFrame.Bodies.First();
            //        Dictionary<JointType, SerializableJoint> userJoints = userBody.Joints;
            //        SerializableJoint userHead = userJoints[JointType.Head];
            //        CameraSpacePoint userHeadCS = userHead.CameraSpacePoint;
            //        SerializableJoint userShoulderLeft = userJoints[JointType.ShoulderLeft];
            //        CameraSpacePoint userShoulderLeftCS = userShoulderLeft.CameraSpacePoint;
            //        SerializableJoint userShoulderRight = userJoints[JointType.ShoulderRight];
            //        CameraSpacePoint userShoulderRightCS = userShoulderRight.CameraSpacePoint;
            //        double headPosDiffX = Math.Pow(referenceHeadCS.X - userHeadCS.X, 2);
            //        double headPosDiffY = Math.Pow(referenceHeadCS.Y - userHeadCS.Y, 2);
            //        double headPosDiffZ = Math.Pow(referenceHeadCS.Z - userHeadCS.Z, 2);
            //        Debug.WriteLine(String.Format("Head x: {0}, y: {1}, z: {2}", headPosDiffX, headPosDiffY, headPosDiffZ));
            //    }
            //}
        }

    }
}
