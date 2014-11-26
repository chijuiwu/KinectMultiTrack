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

        private Dictionary<JointType, WorldCoordinate> jointCoordinates;

        public WorldBody(IEnumerable<KeyValuePair<JointType, SerializableJoint>> joints)
        {
            this.jointCoordinates = new Dictionary<JointType, WorldCoordinate>();
            foreach(KeyValuePair<JointType, SerializableJoint> jointPair in joints)
            {
                JointType jointType = jointPair.Key;
                SerializableJoint joint = jointPair.Value;
                CameraSpacePoint jointPosition = joint.CameraSpacePoint;
                // TODO use the transformed points
                WorldCoordinate worldCoordinate = new WorldCoordinate(jointPosition.X, jointPosition.Y, jointPosition.Z);
                this.jointCoordinates[jointType] = worldCoordinate;
            }
        }

        public Dictionary<JointType, WorldCoordinate> JointWorldCoordinates
        {
            get
            {
                return this.jointCoordinates;
            }
        }
    }
}
