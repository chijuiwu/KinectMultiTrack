using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using KinectSerializer;
using KinectMultiTrack.WorldView;

namespace KinectMultiTrack
{
    public class MovingSkeleton
    {
        // 50 seconds
        private static readonly int MAX_POSITIONS_STORED = 3000;

        public class Position
        {
            public SBody Kinect { get; private set; }
            public WBody Worldview { get; private set; }

            public Position(SBody kinect, WBody worldview)
            {
                this.Kinect = kinect;
                this.Worldview = worldview;
            }

            public static Position Copy(Position position)
            {
                return new Position(SBody.Copy(position.Kinect), WBody.Copy(position.Worldview));
            }
        }

        public ulong TrackingId { get; private set; }
        public long Timestamp { get; private set; }
        public double InitialAngle { get; private set; }
        public WCoordinate InitialCenterPosition { get; private set; }
        public float InitialDistance { get; private set; } // distance(z-value) from the reference Kinect
        public Stack<MovingSkeleton.Position> Positions { get; private set; }

        public MovingSkeleton.Position CurrentPosition
        {
            get
            {
                return this.Positions.FirstOrDefault();
            }
        }

        public MovingSkeleton(SBody body, long timestamp, double initAngle, WCoordinate initCenterPosition)
        {
            this.TrackingId = body.TrackingId;
            this.Timestamp = timestamp;
            this.InitialAngle = initAngle;
            this.InitialCenterPosition = initCenterPosition;
            this.InitialDistance = this.InitialCenterPosition.Z;
            this.Positions = new Stack<MovingSkeleton.Position>();
            // Enable tracking!!
            this.UpdatePosition(timestamp, body);
        }

        private MovingSkeleton(Position currentPosition, ulong trackingId, long timestamp, double initAngle, WCoordinate initCenterPosition)
        {
            this.TrackingId = trackingId;
            this.Timestamp = timestamp;
            this.InitialAngle = initAngle;
            this.InitialCenterPosition = initCenterPosition;
            this.InitialDistance = this.InitialCenterPosition.Z;
            this.Positions = new Stack<MovingSkeleton.Position>();
            this.Positions.Push(currentPosition);
        }

        public static MovingSkeleton Copy(MovingSkeleton skeleton)
        {
            return new MovingSkeleton(Position.Copy(skeleton.CurrentPosition), skeleton.TrackingId, skeleton.Timestamp, skeleton.InitialAngle, skeleton.InitialCenterPosition);
        }

        public void UpdatePosition(long timestamp, SBody body)
        {
            this.Timestamp = timestamp;
            if (this.Positions.Count() > MovingSkeleton.MAX_POSITIONS_STORED)
            {
                this.Positions.Clear();
            }
            this.Positions.Push(new MovingSkeleton.Position(body, WBody.Create(body, this.InitialAngle, this.InitialCenterPosition)));
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

            return (this.TrackingId == p.TrackingId) && (this.InitialAngle == p.InitialAngle) && (this.InitialCenterPosition == p.InitialCenterPosition);
        }

        public bool Equals(MovingSkeleton p)
        {
            return (this.TrackingId == p.TrackingId) && (this.InitialAngle == p.InitialAngle) && (this.InitialCenterPosition == p.InitialCenterPosition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.TrackingId.GetHashCode();
                hash = hash * 23 + this.InitialAngle.GetHashCode();
                hash = hash * 23 + this.InitialCenterPosition.GetHashCode();
                return hash;
            }
        }
    }
}
