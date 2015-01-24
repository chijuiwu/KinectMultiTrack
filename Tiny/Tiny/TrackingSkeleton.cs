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
    public class TrackingSkeleton
    {
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
        public double InitialAngle { get; set; }
        public WCoordinate InitialPosition { get; set; }
        public Stack<TrackingSkeleton.Position> Positions { get; private set; }

        public TrackingSkeleton.Position CurrentPosition
        {
            get
            {
                return this.Positions.Peek();
            }
        }

        public TrackingSkeleton(ulong id)
        {
            this.Id = id;
            this.Positions = new Stack<TrackingSkeleton.Position>();
        }

        public TrackingSkeleton(ulong id, double initAngle, WCoordinate initPos, Stack<TrackingSkeleton.Position> positions)
        {
            this.Id = id;
            this.InitialAngle = initAngle;
            this.InitialPosition = initPos;
            this.Positions = positions;
        }

        public void UpdatePosition(SBody body, WBody worldviewBody)
        {
            this.Positions.Push(new TrackingSkeleton.Position(body, worldviewBody));
        }

        // Copy only the current positions
        public static TrackingSkeleton Copy(TrackingSkeleton person)
        {
            Stack<TrackingSkeleton.Position> currentPos = new Stack<TrackingSkeleton.Position>();
            currentPos.Push(TrackingSkeleton.Position.Copy(person.Positions.Peek()));
            TrackingSkeleton copy = new TrackingSkeleton(person.Id, person.InitialAngle, person.InitialPosition, currentPos);
            return copy;
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
            
            TrackingSkeleton p = obj as TrackingSkeleton;
            if ((Object)p == null)
            {
                return false;
            }

            return (this.Id == p.Id) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public bool Equals(TrackingSkeleton p)
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
