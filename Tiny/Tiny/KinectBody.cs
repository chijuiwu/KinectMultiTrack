using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny
{
    class KinectBody
    {
        private Dictionary<JointType, CameraSpacePoint> joints;

        public KinectBody()
        {
            this.joints = new Dictionary<JointType, CameraSpacePoint>();
        }

        public Dictionary<JointType, CameraSpacePoint> Joints
        {
            get
            {
                return this.joints;
            }
        }
    }
}
