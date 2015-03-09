using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Microsoft.Kinect;

namespace KinectSerializer
{
    [Serializable]
    public class SJoint : ISerializable
    {
        public const string NameTrackingState = "TrackingState";
        public const string NameType = "JointType";
        public const string NameOrientation = "Orientation";
        public const string NameCamperaSpacePoint = "CameraSpacePoint";
        public const string NameDepthSpacePoint = "DepthSpacePoint";

        public TrackingState TrackingState { get; set; }

        public JointType Type { get; set; }

        public JointOrientation Orientation { get; set; }

        public CameraSpacePoint CameraSpacePoint { get; set; }

        public DepthSpacePoint DepthSpacePoint { get; set; }

        public SJoint(TrackingState trackingState, JointType type, JointOrientation orientation, CameraSpacePoint cameraSpacePoint, DepthSpacePoint depthSpacePoint)
        {
            this.TrackingState = trackingState;
            this.Type = type;
            this.Orientation = orientation;
            this.CameraSpacePoint = cameraSpacePoint;
            this.DepthSpacePoint = depthSpacePoint;
        }

        protected SJoint(SerializationInfo info, StreamingContext ctx)
        {
            this.TrackingState = (TrackingState)info.GetValue(SJoint.NameTrackingState, typeof(TrackingState));
            this.Type = (JointType)info.GetValue(SJoint.NameType, typeof(JointType));
            this.CameraSpacePoint = (CameraSpacePoint)info.GetValue(SJoint.NameCamperaSpacePoint, typeof(CameraSpacePoint));
            this.DepthSpacePoint = (DepthSpacePoint)info.GetValue(SJoint.NameDepthSpacePoint, typeof(DepthSpacePoint));
            this.Orientation = (JointOrientation)info.GetValue(SJoint.NameOrientation, typeof(JointOrientation));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SJoint.NameTrackingState, this.TrackingState, typeof(TrackingState));
            info.AddValue(SJoint.NameType, this.Type, typeof(JointType));
            info.AddValue(SJoint.NameCamperaSpacePoint, this.CameraSpacePoint, typeof(CameraSpacePoint));
            info.AddValue(SJoint.NameDepthSpacePoint, this.DepthSpacePoint, typeof(DepthSpacePoint));
            info.AddValue(SJoint.NameOrientation, this.Orientation, typeof(JointOrientation));
        }

        internal static SJoint Copy(SJoint joint)
        {
            CameraSpacePoint jointCSPoint = joint.CameraSpacePoint;
            CameraSpacePoint copyCSPoint = new CameraSpacePoint();
            copyCSPoint.X = jointCSPoint.X;
            copyCSPoint.Y = jointCSPoint.Y;
            copyCSPoint.Z = jointCSPoint.Z;
            DepthSpacePoint jointDSPoint = joint.DepthSpacePoint;
            DepthSpacePoint copyDSPoint = new DepthSpacePoint();
            copyDSPoint.X = jointDSPoint.X;
            copyDSPoint.Y = jointDSPoint.Y;
            SJoint copy = new SJoint(joint.TrackingState, joint.Type, joint.Orientation, copyCSPoint, copyDSPoint);
            return copy;
        }
    }
}
