using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using System.IO;

namespace KinectSerializer
{
    [Serializable]
    public class SerializableBody : ISerializable
    {
        public const string NameIsTracked = "IsTracked";
        public const string NameTrackingId = "TrackingId";

        public const string NameAnkleLeft = "AnkleLeft";
        public const string NameAnkleRight = "AnkleRight";

        private bool isTracked;
        private ulong trackingId;

        private Joint ankleLeft;
        private Joint ankleRight;

        public SerializableBody(Body body) {
            this.isTracked = body.IsTracked;
            this.trackingId = body.TrackingId;
            this.ankleLeft = body.Joints[JointType.AnkleLeft];
            this.ankleRight = body.Joints[JointType.AnkleRight];
        }

        protected SerializableBody(SerializationInfo info, StreamingContext ctx)
        {
            this.isTracked = (bool)info.GetValue(SerializableBody.NameIsTracked, typeof(bool));
            this.trackingId = (ulong)info.GetValue(SerializableBody.NameTrackingId, typeof(ulong));
            this.ankleLeft = (Joint)info.GetValue(SerializableBody.NameAnkleLeft, typeof(Joint));
            this.ankleRight = (Joint)info.GetValue(SerializableBody.NameAnkleRight, typeof(Joint));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBody.NameIsTracked, this.isTracked, typeof(bool));
            info.AddValue(SerializableBody.NameTrackingId, this.trackingId, typeof(ulong));
            info.AddValue(SerializableBody.NameAnkleLeft, this.ankleLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameAnkleRight, this.ankleRight, typeof(Joint));
        }

        public bool IsTracked 
        {
            get
            {
                return this.isTracked;
            }
        }
        public ulong TrackingId 
        {
            get
            {
                return this.trackingId;
            }
        }

        public Joint AnkleLeft
        {
            get
            {
                return this.ankleLeft;
            }
        }

        public Joint AnkleRight
        {
            get
            {
                return this.ankleRight;
            }
        }
    }
}
