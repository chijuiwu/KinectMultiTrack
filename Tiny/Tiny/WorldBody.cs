using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectSerializer;
using Microsoft.Kinect;

namespace Tiny
{
    class WorldBody
    {

        private Dictionary<JointType, WorldCoordinate> joints;

        public WorldBody(IEnumerable<KeyValuePair<JointType, SerializableJoint>> kinectJoints)
        {
            this.joints = new Dictionary<JointType, WorldCoordinate>();
            foreach(KeyValuePair<JointType, SerializableJoint> jointPair in kinectJoints)
            {
                JointType jointType = jointPair.Key;
                SerializableJoint joint = jointPair.Value;
                CameraSpacePoint jointPosition = joint.CameraSpacePoint;
                // TODO use the transformed points
                WorldCoordinate worldCoordinate = new WorldCoordinate(jointPosition.X, jointPosition.Y, jointPosition.Z);
                this.joints[jointType] = worldCoordinate;
            }
        }

        public Dictionary<JointType, WorldCoordinate> Joints
        {
            get
            {
                return this.joints;
            }
        }
    }
}
