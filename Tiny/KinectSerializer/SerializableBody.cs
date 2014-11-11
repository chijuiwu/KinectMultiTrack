using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using System.IO;
using System.Collections.ObjectModel;

namespace KinectSerializer
{
    [Serializable]
    public class SerializableBody : ISerializable
    {
        public const string NameIsTracked = "IsTracked";
        public const string NameTrackingId = "TrackingId";
        public const string NameJointsDictionary = "JointsDict";
        public const string NameJointsDepthPositionDictionary = "JointsCameraPositionDict";

        private bool isTracked;

        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }
        }

        private ulong trackingId;

        public ulong TrackingId
        {
            get
            {
                return this.trackingId;
            }
        }

        private Dictionary<JointType, SerializableJoint> jointsDict;

        public Dictionary<JointType, SerializableJoint> Joints
        {
            get
            {
                return this.jointsDict;
            }
        }

        public SerializableBody(bool isTracked, ulong trackingId)
        {
            this.isTracked = isTracked;
            this.trackingId = trackingId;
            this.jointsDict = new Dictionary<JointType, SerializableJoint>();
        }

        public void updateJoint(JointType jointType, SerializableJoint joint)
        {
            this.jointsDict[jointType] = joint;
        }

        protected SerializableBody(SerializationInfo info, StreamingContext ctx)
        {
            this.isTracked = (bool)info.GetValue(SerializableBody.NameIsTracked, typeof(bool));
            this.trackingId = (ulong)info.GetValue(SerializableBody.NameTrackingId, typeof(ulong));
            this.jointsDict = (Dictionary<JointType, SerializableJoint>)info.GetValue(SerializableBody.NameJointsDictionary, typeof(Dictionary<JointType, SerializableJoint>));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBody.NameIsTracked, this.IsTracked, typeof(bool));
            info.AddValue(SerializableBody.NameTrackingId, this.TrackingId, typeof(ulong));
            info.AddValue(SerializableBody.NameJointsDictionary, this.jointsDict, typeof(Dictionary<JointType, SerializableJoint>));
        }
    }
}
