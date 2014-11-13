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
        public class User
        {
            private KinectCamera clientCamera;
            private SerializableBodyFrame worldBodyFrame;

            public User(KinectCamera clientCamera)
            {
                this.clientCamera = clientCamera;
                this.worldBodyFrame = clientCamera.CurrentBodyFrame;
            }

            public KinectCamera ClientCamera
            {
                get
                {
                    return this.clientCamera;
                }
            }

            public SerializableBodyFrame WorldBodyFrame
            {
                get
                {
                    return this.worldBodyFrame;
                }
                set
                {
                    this.worldBodyFrame = value;
                }
            }
        }

        private ConcurrentDictionary<IPEndPoint, User> users;

        public IEnumerable<SerializableBodyFrame> ClientBodyFrames
        {
            get
            {
                foreach (User user in this.users.Values)
                {
                    yield return user.ClientCamera.CurrentBodyFrame;
                }
            }
        }

        public IEnumerable<SerializableBodyFrame> ProcessedBodyFrames
        {
            get
            {
                foreach (User user in this.users.Values)
                {
                    yield return user.WorldBodyFrame;
                }
            }
        }

        public WorldCamera()
        {
            this.users = new ConcurrentDictionary<IPEndPoint, User>();
        }

        public void AddOrUpdateClientCamera(IPEndPoint clientIP, SerializableBodyFrame bodyFrame)
        {
            bool containsClientIP = this.users.ContainsKey(clientIP);
            if (containsClientIP)
            {
                this.users[clientIP].ClientCamera.UpdateBodyFrame(bodyFrame);
            }
            else
            {
                User user = new User(new KinectCamera(clientIP, bodyFrame));
                this.users[clientIP] = user;
            }
        }

        public void RemoveClientCamera(IPEndPoint clientIP)
        {
            User user;
            bool foundUser = this.users.TryRemove(clientIP, out user);
            if (foundUser)
            {
                user.ClientCamera.Close();
            }
        }

        public void SynchronizeFrames()
        {
            foreach (User user in this.users.Values)
            {
                user.WorldBodyFrame = SerializableBodyFrame.Copy(user.ClientCamera.CurrentBodyFrame);
            }
            SerializableBodyFrame referenceBodyFrame = this.users.First().Value.WorldBodyFrame;
            // Assumes only one body
            SerializableBody referenceBody = referenceBodyFrame.Bodies.First();
            Dictionary<JointType, SerializableJoint> referenceJoints = referenceBody.Joints;
            SerializableJoint referenceHead = referenceJoints[JointType.Head];
            CameraSpacePoint referenceHeadCS = referenceHead.CameraSpacePoint;
            SerializableJoint referenceShoulderLeft = referenceJoints[JointType.ShoulderLeft];
            CameraSpacePoint referenceShoulderLeftCS = referenceShoulderLeft.CameraSpacePoint;
            SerializableJoint referenceShoulderRight = referenceJoints[JointType.ShoulderRight];
            CameraSpacePoint referenceShoulderRightCS = referenceShoulderRight.CameraSpacePoint;
            foreach (User user in this.users.Values)
            {
                // Translate the joint positions
                if (!user.WorldBodyFrame.Equals(referenceBodyFrame))
                {
                    // Assumes only one body
                    SerializableBody userBody = user.WorldBodyFrame.Bodies.First();
                    Dictionary<JointType, SerializableJoint> userJoints = userBody.Joints;
                    SerializableJoint userHead = userJoints[JointType.Head];
                    CameraSpacePoint userHeadCS = userHead.CameraSpacePoint;
                    SerializableJoint userShoulderLeft = userJoints[JointType.ShoulderLeft];
                    CameraSpacePoint userShoulderLeftCS = userShoulderLeft.CameraSpacePoint;
                    SerializableJoint userShoulderRight = userJoints[JointType.ShoulderRight];
                    CameraSpacePoint userShoulderRightCS = userShoulderRight.CameraSpacePoint;
                    float headPosDiffX = referenceHeadCS.X - userHeadCS.X;
                    float headPosDiffY = referenceHeadCS.Y - userHeadCS.Y;
                    float headPosDiffZ = referenceHeadCS.Z - userHeadCS.Z;
                }
            }
        }

    }
}
