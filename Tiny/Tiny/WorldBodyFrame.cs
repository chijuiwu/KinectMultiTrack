using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectSerializer;

namespace Tiny
{
    public class WBodyFrame
    {
        private List<WBody> bodies;

        public WBodyFrame(int depthFrameWidth, int depthFrameHeight, List<WBody> bodies)
        {
            this.depthFrameWidth = depthFrameWidth;
            this.depthFrameHeight = depthFrameHeight;
            this.bodies = bodies;
        }


        public IEnumerable<WBody> Boides
        {
            get
            {
                return this.bodies;
            }
        }
    }
}
