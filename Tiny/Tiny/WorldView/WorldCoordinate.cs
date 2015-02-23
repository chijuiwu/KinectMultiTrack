using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny.WorldView
{
    public class WCoordinate
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public WCoordinate(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static WCoordinate Copy(WCoordinate c)
        {
            return new WCoordinate(c.X, c.Y, c.Z);
        }

        public override string ToString()
        {
            return String.Format("({0},{1},{2})", this.X, this.Y, this.Z);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            WCoordinate c = obj as WCoordinate;
            if ((Object)c == null)
            {
                return false;
            }

            return (this.X == c.X) && (this.Y == c.Y) && (this.Z == c.Z);
        }

        public bool Equals(WCoordinate c)
        {
            return (this.X == c.X) && (this.Y == c.Y) && (this.Z == c.Z);
        }

        // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.X.GetHashCode();
                hash = hash * 23 + this.Y.GetHashCode();
                hash = hash * 23 + this.Z.GetHashCode();
                return hash;
            }
        }

        public static float CalculateDifference(WCoordinate c0, WCoordinate c1)
        {
            return (float)Math.Sqrt(Math.Pow(c0.X-c1.X, 2) + Math.Pow(c0.Y-c1.Y, 2) + Math.Pow(c0.Z-c1.Z, 2));
        }
    }
}
