using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny
{
    public class KinectBody
    {
        public Dictionary<JointType, KinectJoint> Joints { get; private set; }

        public KinectBody()
        {
            this.Joints = new Dictionary<JointType, KinectJoint>();
        }
    }
}
