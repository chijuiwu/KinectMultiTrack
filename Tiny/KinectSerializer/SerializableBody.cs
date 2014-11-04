using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;
using Microsoft.Kinect;

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

        public SerializableBody(SerializationInfo info, StreamingContext ctx)
        {
            this.isTracked = (bool)info.GetValue(SerializableBody.NameIsTracked, typeof(bool));
            this.trackingId = (ulong)info.GetValue(SerializableBody.NameTrackingId, typeof(ulong));
            this.ankleLeft = (Joint)info.GetValue(SerializableBody.NameAnkleLeft, typeof(Joint));
            this.ankleRight = (Joint)info.GetValue(SerializableBody.NameAnkleRight, typeof(Joint));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBody.NameIsTracked, this.isTracked);
            info.AddValue(SerializableBody.NameTrackingId, this.trackingId);
            info.AddValue(SerializableBody.NameAnkleLeft, this.ankleLeft);
            info.AddValue(SerializableBody.NameAnkleRight, this.ankleRight);
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

        public Joint AngleLeft
        {
            get
            {
                return this.ankleLeft;
            }
        }
    }
}
