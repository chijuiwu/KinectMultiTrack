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
using System.Collections.ObjectModel;

namespace KinectSerializer
{
    [Serializable]
    public class SerializableBody : ISerializable
    {
        public const string NameIsTracked = "IsTracked";
        public const string NameTrackingId = "TrackingId";

        public const string NameAnkleLeft = "AnkleLeft";
        public const string NameAnkleRight = "AnkleRight";
        public const string NameElbowLeft = "ElbowLeft";
        public const string NameElbowRight = "ElbowRight";
        public const string NameFootLeft = "FootLeft";
        public const string NameFootRight = "FootRight";
        public const string NameHandLeft = "HandLeft";
        public const string NameHandRight = "HandRight";
        public const string NameHandTipLeft = "HandTipLeft";
        public const string NameHandTipRight = "HandTipRight";
        public const string NameHead = "Head";
        public const string NameHipLeft = "HipLeft";
        public const string NameHipRight = "HipRight";
        public const string NameKneeLeft = "KneeLeft";
        public const string NameKneeRight = "KneeRight";
        public const string NameNeck = "Neck";
        public const string NameShoulderLeft = "ShoulderLeft";
        public const string NameShoulderRight = "ShoulderRight";
        public const string NameSpineBase = "SpineBase";
        public const string NameSpineMid = "SpineMid";
        public const string NameSpineShoulder = "SpineShoulder";
        public const string NameThumbLeft = "ThumbLeft";
        public const string NameThumbRight = "ThumbRight";
        public const string NameWristLeft = "WristLeft";
        public const string NameWristRight = "WristRight";

        public const string NameJointsDictionary = "JointsDict";

        #region Fields

        public bool IsTracked
        {
            get;
            set;
        }
        public ulong TrackingId
        {
            get;
            set;
        }

        public Joint AnkleLeft
        {
            get;
            set;
        }
        public Joint AnkleRight
        {
            get;
            set;
        }
        public Joint ElbowLeft
        {
            get;
            set;
        }
        public Joint ElbowRight
        {
            get;
            set;
        }
        public Joint FootLeft
        {
            get;
            set;
        }
        public Joint FootRight
        {
            get;
            set;
        }
        public Joint HandLeft
        {
            get;
            set;
        }
        public Joint HandRight
        {
            get;
            set;
        }
        public Joint HandTipLeft
        {
            get;
            set;
        }
        public Joint HandTipRight
        {
            get;
            set;
        }
        public Joint Head
        {
            get;
            set;
        }
        public Joint HipLeft
        {
            get;
            set;
        }
        public Joint HipRight
        {
            get;
            set;
        }
        public Joint KneeLeft
        {
            get;
            set;
        }
        public Joint KneeRight
        {
            get;
            set;
        }
        public Joint Neck
        {
            get;
            set;
        }
        public Joint ShoulderLeft
        {
            get;
            set;
        }
        public Joint ShoulderRight
        {
            get;
            set;
        }
        public Joint SpineBase
        {
            get;
            set;
        }
        public Joint SpineMid
        {
            get;
            set;
        }
        public Joint SpineShoulder
        {
            get;
            set;
        }
        public Joint ThumbLeft
        {
            get;
            set;
        }
        public Joint ThumbRight
        {
            get;
            set;
        }
        public Joint WristLeft
        {
            get;
            set;
        }
        public Joint WristRight
        {
            get;
            set;
        }

        #endregion

        public Dictionary<JointType, Joint> Joints
        {
            get;
            set;
        }

        public SerializableBody(Body body)
        {
            this.IsTracked = body.IsTracked;
            this.TrackingId = body.TrackingId;
            this.AnkleLeft = body.Joints[JointType.AnkleLeft];
            this.AnkleRight = body.Joints[JointType.AnkleRight];
            this.ElbowLeft = body.Joints[JointType.ElbowLeft];
            this.ElbowRight = body.Joints[JointType.ElbowRight];
            this.FootLeft = body.Joints[JointType.FootLeft];
            this.FootRight = body.Joints[JointType.FootRight];
            this.HandLeft = body.Joints[JointType.HandLeft];
            this.HandRight = body.Joints[JointType.HandRight];
            this.HandTipLeft = body.Joints[JointType.HandTipLeft];
            this.HandTipRight = body.Joints[JointType.HandTipRight];
            this.Head = body.Joints[JointType.Head];
            this.HipLeft = body.Joints[JointType.HipLeft];
            this.HipRight = body.Joints[JointType.HipRight];
            this.KneeLeft = body.Joints[JointType.KneeLeft];
            this.KneeRight = body.Joints[JointType.KneeRight];
            this.Neck = body.Joints[JointType.Neck];
            this.ShoulderLeft = body.Joints[JointType.ShoulderLeft];
            this.ShoulderRight = body.Joints[JointType.ShoulderRight];
            this.SpineBase = body.Joints[JointType.SpineBase];
            this.SpineMid = body.Joints[JointType.SpineMid];
            this.SpineShoulder = body.Joints[JointType.SpineShoulder];
            this.ThumbLeft = body.Joints[JointType.ThumbLeft];
            this.ThumbRight = body.Joints[JointType.ThumbRight];
            this.WristLeft = body.Joints[JointType.WristLeft];
            this.WristRight = body.Joints[JointType.WristRight];

            this.Joints = new Dictionary<JointType, Joint>();
            this.Joints.Add(JointType.AnkleLeft, body.Joints[JointType.AnkleLeft]);
            this.Joints.Add(JointType.AnkleRight, body.Joints[JointType.AnkleRight]);
            this.Joints.Add(JointType.ElbowLeft, body.Joints[JointType.ElbowLeft]);
            this.Joints.Add(JointType.ElbowRight, body.Joints[JointType.ElbowRight]);
            this.Joints.Add(JointType.FootLeft, body.Joints[JointType.FootLeft]);
            this.Joints.Add(JointType.FootRight, body.Joints[JointType.FootRight]);
            this.Joints.Add(JointType.HandLeft, body.Joints[JointType.HandLeft]);
            this.Joints.Add(JointType.HandRight, body.Joints[JointType.HandRight]);
            this.Joints.Add(JointType.HandTipLeft, body.Joints[JointType.HandTipLeft]);
            this.Joints.Add(JointType.HandTipRight, body.Joints[JointType.HandTipRight]);
            this.Joints.Add(JointType.Head, body.Joints[JointType.Head]);
            this.Joints.Add(JointType.HipLeft, body.Joints[JointType.HipLeft]);
            this.Joints.Add(JointType.HipRight, body.Joints[JointType.HipRight]);
            this.Joints.Add(JointType.KneeLeft, body.Joints[JointType.KneeLeft]);
            this.Joints.Add(JointType.KneeRight, body.Joints[JointType.KneeRight]);
            this.Joints.Add(JointType.Neck, body.Joints[JointType.Neck]);
            this.Joints.Add(JointType.ShoulderLeft, body.Joints[JointType.ShoulderLeft]);
            this.Joints.Add(JointType.ShoulderRight, body.Joints[JointType.ShoulderRight]);
            this.Joints.Add(JointType.SpineBase, body.Joints[JointType.SpineBase]);
            this.Joints.Add(JointType.SpineMid, body.Joints[JointType.SpineMid]);
            this.Joints.Add(JointType.SpineShoulder, body.Joints[JointType.SpineShoulder]);
            this.Joints.Add(JointType.ThumbLeft, body.Joints[JointType.ThumbLeft]);
            this.Joints.Add(JointType.ThumbRight, body.Joints[JointType.ThumbRight]);
            this.Joints.Add(JointType.WristLeft, body.Joints[JointType.WristLeft]);
            this.Joints.Add(JointType.WristRight, body.Joints[JointType.WristRight]);
        }

        protected SerializableBody(SerializationInfo info, StreamingContext ctx)
        {
            this.IsTracked = (bool)info.GetValue(SerializableBody.NameIsTracked, typeof(bool));
            this.TrackingId = (ulong)info.GetValue(SerializableBody.NameTrackingId, typeof(ulong));
            this.AnkleLeft = (Joint)info.GetValue(SerializableBody.NameAnkleLeft, typeof(Joint));
            this.AnkleRight = (Joint)info.GetValue(SerializableBody.NameAnkleRight, typeof(Joint));
            this.ElbowLeft = (Joint)info.GetValue(SerializableBody.NameElbowLeft, typeof(Joint));
            this.ElbowRight = (Joint)info.GetValue(SerializableBody.NameElbowRight, typeof(Joint));
            this.FootLeft = (Joint)info.GetValue(SerializableBody.NameFootLeft, typeof(Joint));
            this.FootRight = (Joint)info.GetValue(SerializableBody.NameFootRight, typeof(Joint));
            this.AnkleLeft = (Joint)info.GetValue(SerializableBody.NameAnkleLeft, typeof(Joint));
            this.HandLeft = (Joint)info.GetValue(SerializableBody.NameHandLeft, typeof(Joint));
            this.HandRight = (Joint)info.GetValue(SerializableBody.NameHandRight, typeof(Joint));
            this.HandTipLeft = (Joint)info.GetValue(SerializableBody.NameHandTipLeft, typeof(Joint));
            this.HandTipRight = (Joint)info.GetValue(SerializableBody.NameHandTipRight, typeof(Joint));
            this.Head = (Joint)info.GetValue(SerializableBody.NameHead, typeof(Joint));
            this.HipLeft = (Joint)info.GetValue(SerializableBody.NameHipLeft, typeof(Joint));
            this.HipRight = (Joint)info.GetValue(SerializableBody.NameHipRight, typeof(Joint));
            this.KneeLeft = (Joint)info.GetValue(SerializableBody.NameKneeLeft, typeof(Joint));
            this.KneeRight = (Joint)info.GetValue(SerializableBody.NameKneeRight, typeof(Joint));
            this.Neck = (Joint)info.GetValue(SerializableBody.NameNeck, typeof(Joint));
            this.ShoulderLeft = (Joint)info.GetValue(SerializableBody.NameShoulderLeft, typeof(Joint));
            this.ShoulderRight = (Joint)info.GetValue(SerializableBody.NameShoulderRight, typeof(Joint));
            this.SpineMid = (Joint)info.GetValue(SerializableBody.NameSpineMid, typeof(Joint));
            this.SpineShoulder = (Joint)info.GetValue(SerializableBody.NameSpineShoulder, typeof(Joint));
            this.ThumbLeft = (Joint)info.GetValue(SerializableBody.NameThumbLeft, typeof(Joint));
            this.ThumbRight = (Joint)info.GetValue(SerializableBody.NameThumbRight, typeof(Joint));
            this.WristLeft = (Joint)info.GetValue(SerializableBody.NameWristLeft, typeof(Joint));
            this.WristRight = (Joint)info.GetValue(SerializableBody.NameWristRight, typeof(Joint));
            this.WristRight = (Joint)info.GetValue(SerializableBody.NameWristRight, typeof(Joint));
            this.Joints = (Dictionary<JointType, Joint>)info.GetValue(SerializableBody.NameJointsDictionary, typeof(Dictionary<JointType, Joint>));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBody.NameIsTracked, this.IsTracked, typeof(bool));
            info.AddValue(SerializableBody.NameTrackingId, this.TrackingId, typeof(ulong));
            info.AddValue(SerializableBody.NameAnkleLeft, this.AnkleLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameAnkleRight, this.AnkleRight, typeof(Joint));
            info.AddValue(SerializableBody.NameElbowLeft, this.ElbowLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameElbowRight, this.ElbowRight, typeof(Joint));
            info.AddValue(SerializableBody.NameFootLeft, this.FootLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameFootRight, this.FootRight, typeof(Joint));
            info.AddValue(SerializableBody.NameHandLeft, this.HandLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameHandRight, this.HandRight, typeof(Joint));
            info.AddValue(SerializableBody.NameHandTipLeft, this.HandTipLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameHandTipRight, this.HandTipRight, typeof(Joint));
            info.AddValue(SerializableBody.NameHead, this.Head, typeof(Joint));
            info.AddValue(SerializableBody.NameHipLeft, this.HipLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameHipRight, this.HipRight, typeof(Joint));
            info.AddValue(SerializableBody.NameKneeLeft, this.KneeLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameKneeRight, this.KneeRight, typeof(Joint));
            info.AddValue(SerializableBody.NameNeck, this.Neck, typeof(Joint));
            info.AddValue(SerializableBody.NameShoulderLeft, this.ShoulderLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameShoulderRight, this.ShoulderRight, typeof(Joint));
            info.AddValue(SerializableBody.NameSpineBase, this.SpineBase, typeof(Joint));
            info.AddValue(SerializableBody.NameSpineMid, this.SpineMid, typeof(Joint));
            info.AddValue(SerializableBody.NameSpineShoulder, this.SpineShoulder, typeof(Joint));
            info.AddValue(SerializableBody.NameThumbLeft, this.ThumbLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameThumbRight, this.ThumbRight, typeof(Joint));
            info.AddValue(SerializableBody.NameWristLeft, this.WristLeft, typeof(Joint));
            info.AddValue(SerializableBody.NameWristRight, this.WristRight, typeof(Joint));
            info.AddValue(SerializableBody.NameJointsDictionary, this.Joints, typeof(Dictionary<JointType, Joint>));
        }
    }
}
