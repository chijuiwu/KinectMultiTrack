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
        public const string NameJointType = "Type";
        public const string NameCamperaSpacePoint = "CameraSpacePoint";
        public const string NameDepthSpacePoint = "DepthSpacePoint";
        public const string NameOrientation = "Orientation";

        private TrackingState trackingState;

        public TrackingState TrackingState
        {
            get
            {
                return this.trackingState;
            }
        }

        private JointType type;

        public JointType JointType
        {
            get
            {
                return this.type;
            }
        }

        private JointOrientation orientation;
        public JointOrientation JointOrientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                this.orientation = value;
            }
        }

        private CameraSpacePoint cameraSpacePoint;

        public CameraSpacePoint CameraSpacePoint
        {
            get
            {
                return this.cameraSpacePoint;
            }
            set
            {
                this.cameraSpacePoint = value;
            }
        }

        private DepthSpacePoint depthSpacePoint;
        
        public DepthSpacePoint DepthSpacePoint
        {
            get
            {
                return this.depthSpacePoint;
            }
            set
            {
                this.depthSpacePoint = value;
            }
        }

        public SJoint(TrackingState trackingState, JointType type, JointOrientation orientation, CameraSpacePoint cameraSpacePoint, DepthSpacePoint depthSpacePoint)
        {
            this.trackingState = trackingState;
            this.type = type;
            this.orientation = orientation;
            this.cameraSpacePoint = cameraSpacePoint;
            this.depthSpacePoint = depthSpacePoint;
        }

        protected SJoint(SerializationInfo info, StreamingContext ctx)
        {
            this.trackingState = (TrackingState)info.GetValue(SJoint.NameTrackingState, typeof(TrackingState));
            this.type = (JointType)info.GetValue(SJoint.NameJointType, typeof(JointType));
            this.cameraSpacePoint = (CameraSpacePoint)info.GetValue(SJoint.NameCamperaSpacePoint, typeof(CameraSpacePoint));
            this.depthSpacePoint = (DepthSpacePoint)info.GetValue(SJoint.NameDepthSpacePoint, typeof(DepthSpacePoint));
            this.orientation = (JointOrientation)info.GetValue(SJoint.NameOrientation, typeof(JointOrientation));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SJoint.NameTrackingState, this.trackingState, typeof(TrackingState));
            info.AddValue(SJoint.NameJointType, this.type, typeof(JointType));
            info.AddValue(SJoint.NameCamperaSpacePoint, this.cameraSpacePoint, typeof(CameraSpacePoint));
            info.AddValue(SJoint.NameDepthSpacePoint, this.depthSpacePoint, typeof(DepthSpacePoint));
            info.AddValue(SJoint.NameOrientation, this.orientation, typeof(JointOrientation));
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
            SJoint copy = new SJoint(joint.TrackingState, joint.JointType, joint.JointOrientation, copyCSPoint, copyDSPoint);
            return copy;
        }
    }
}
