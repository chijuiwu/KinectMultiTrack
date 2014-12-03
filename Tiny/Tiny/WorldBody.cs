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

        public WorldBody()
        {
            this.joints = new Dictionary<JointType, WorldCoordinate>();
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
