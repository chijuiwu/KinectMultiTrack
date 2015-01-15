using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectSerializer;

namespace Tiny
{
    public class WorldBodyFrame
    {
        private int depthFrameWidth;
        private int depthFrameHeight;
        private List<WorldBody> bodies;

        public WorldBodyFrame(int depthFrameWidth, int depthFrameHeight, List<WorldBody> bodies)
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

        public IEnumerable<WorldBody> Boides
        {
            get
            {
                return this.bodies;
            }
        }
    }
}
