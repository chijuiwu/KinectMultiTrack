using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny
{
    public class WCoordinate
    {
        private float x;
        private float y;
        private float z;

        public float X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public float Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public float Z
        {
            get
            {
                return this.z;
            }
            set
            {
                this.z = value;
            }
        }

        public WCoordinate(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static WCoordinate Copy(WCoordinate c)
        {
            return new WCoordinate(c.X, c.Y, c.Z);
        }

        public override string ToString()
        {
            return String.Format("({0},{1},{2})", this.X, this.Y, this.Z);
        }
    }
}
