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
    public class MovingSkeleton
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

        public ulong TrackingId { get; private set; }
        public long Timestamp { get; private set; }
        public double InitialAngle { get; private set; }
        public WCoordinate InitialPosition { get; private set; }
        public float InitialDistance { get; private set; } // distance(z-value) from the reference Kinect
        public Stack<MovingSkeleton.Position> Positions { get; private set; }

        public MovingSkeleton.Position CurrentPosition
        {
            get
            {
                return this.Positions.FirstOrDefault();
            }
        }

        public MovingSkeleton(ulong trackingId, long timestamp, double initAngle, WCoordinate initPos)
        {
            this.TrackingId = trackingId;
            this.Timestamp = timestamp;
            this.InitialAngle = initAngle;
            this.InitialPosition = initPos;
            this.InitialDistance = this.InitialPosition.Z;
            this.Positions = new Stack<MovingSkeleton.Position>();
        }

        public void UpdatePosition(long timestamp, SBody body, WBody worldviewBody)
        {
            this.Timestamp = timestamp;
            if (this.Positions.Count() > MAX_POSITIONS_STORED)
            {
                this.Positions.Clear();
            }
            this.Positions.Push(new MovingSkeleton.Position(body, worldviewBody));
        }

        // Copy only the current position
        public static MovingSkeleton CurrentCopy(MovingSkeleton skeleton)
        {
            MovingSkeleton copy = new MovingSkeleton(skeleton.TrackingId, skeleton.Timestamp, skeleton.InitialAngle, skeleton.InitialPosition);
            Position currentPos = skeleton.CurrentPosition;
            if (currentPos != null)
            {
                copy.Positions.Push(Position.Copy(currentPos));
            }
            return copy;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append("Id: ").Append(this.TrackingId).Append(", ");
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
            
            MovingSkeleton p = obj as MovingSkeleton;
            if ((Object)p == null)
            {
                return false;
            }

            return (this.TrackingId == p.TrackingId) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public bool Equals(MovingSkeleton p)
        {
            return (this.TrackingId == p.TrackingId) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.TrackingId.GetHashCode();
                hash = hash * 23 + this.InitialAngle.GetHashCode();
                hash = hash * 23 + this.InitialPosition.GetHashCode();
                return hash;
            }
        }
    }
}
