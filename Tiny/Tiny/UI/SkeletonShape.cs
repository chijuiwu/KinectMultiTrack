
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny.UI
{
    class SkeletonShape
    {
        public Dictionary<JointType, JointShape> Joints { get; private set; }

        public SkeletonShape(Dictionary<JointType, JointShape> joints)
        {
            this.Joints = joints;
        }
    }
}
