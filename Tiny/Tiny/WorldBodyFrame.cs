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
        private int depthFrameWidth;
        private int depthFrameHeight;
        private List<WBody> bodies;

        public WBodyFrame(int depthFrameWidth, int depthFrameHeight, List<WBody> bodies)
        {
            this.depthFrameWidth = depthFrameWidth;
            this.depthFrameHeight = depthFrameHeight;
            this.bodies = bodies;
        }

        public int DepthFrameWidth
        {
            get
            {
                return this.depthFrameWidth;
            }
        }

        public int DepthFrameHeight
        {
            get
            {
                return this.depthFrameHeight;
            }
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
