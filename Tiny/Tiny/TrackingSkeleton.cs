using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using KinectSerializer;
using Tiny.WorldView;

namespace Tiny
{
    public class TSkeleton
    {
        // 100 seconds
        private readonly int MAX_POSITIONS_STORED = 3000;

        public class Position
        {
            public SBody Kinect { get; private set; }
            public WBody Worldview { get; private set; }

            public Position(SBody kinect, WBody worldview)
            {
                this.Kinect = kinect;
                this.Worldview = worldview;
            }

            public static Position Copy(Position c)
            {
                return new Position(SBody.Copy(c.Kinect), WBody.Copy(c.Worldview));
            }
        }

        public ulong Id { get; private set; }
        public long Timestamp { get; private set; }
        public double InitialAngle { get; private set; }
        public WCoordinate InitialPosition { get; private set; }
        public float InitialDistance { get; private set; } // distance(z-value) from the reference Kinect
        public Stack<TSkeleton.Position> Positions { get; private set; }

        public TSkeleton.Position CurrentPosition
        {
            get
            {
                if (this.Positions.Count > 0)
                {
                    return this.Positions.Peek();
                }
                else
                {
                    return null;
                }
            }
        }

        public TSkeleton(ulong id, long timestamp, double initAngle, WCoordinate initPos)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.InitialAngle = initAngle;
            this.InitialPosition = initPos;
            this.InitialDistance = this.InitialPosition.Z;
            this.Positions = new Stack<TSkeleton.Position>();
        }

        public void UpdatePosition(long timestamp, SBody body, WBody worldviewBody)
        {
            this.Timestamp = timestamp;
            if (this.Positions.Count() > MAX_POSITIONS_STORED)
            {
                this.Positions.Clear();
            }
            this.Positions.Push(new TSkeleton.Position(body, worldviewBody));
        }

        // Copy only the current positions
        public static TSkeleton Copy(TSkeleton skeleton)
        {
            if (skeleton.Positions.Count > 0)
            {
                TSkeleton copy = new TSkeleton(skeleton.Id, skeleton.Timestamp, skeleton.InitialAngle, skeleton.InitialPosition);
                copy.Positions.Push(Position.Copy(skeleton.CurrentPosition));
                return copy;
            }
            else
            {
                return new TSkeleton(skeleton.Id, skeleton.Timestamp, skeleton.InitialAngle, skeleton.InitialPosition);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append("Id: ").Append(this.Id).Append(", ");
            sb.Append("Position: ").Append(this.CurrentPosition);
            sb.Append("]");
            return sb.ToString();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            TSkeleton p = obj as TSkeleton;
            if ((Object)p == null)
            {
                return false;
            }

            return (this.Id == p.Id) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public bool Equals(TSkeleton p)
        {
            return (this.Id == p.Id) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.Id.GetHashCode();
                hash = hash * 23 + this.InitialAngle.GetHashCode();
                hash = hash * 23 + this.InitialPosition.GetHashCode();
                return hash;
            }
        }
    }
}
