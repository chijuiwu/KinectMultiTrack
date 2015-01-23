using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using KinectSerializer;

namespace Tiny
{
    public class Person
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
        public Stack<Person.Position> Positions { get; private set; }

        public Person.Position CurrentPosition
        {
            get
            {
                return this.Positions.Peek();
            }
        }

        public Person(ulong id)
        {
            this.Id = id;
            this.Positions = new Stack<Person.Position>();
        }

        public Person(ulong id, double initAngle, WCoordinate initPos, Stack<Person.Position> positions)
        {
            this.Id = id;
            this.InitialAngle = initAngle;
            this.InitialPosition = initPos;
            this.Positions = positions;
        }

        public void UpdatePosition(SBody body, WBody worldviewBody)
        {
            this.Positions.Push(new Person.Position(body, worldviewBody));
        }

        // Copy only the current positions
        public static Person Copy(Person person)
        {
            Stack<Person.Position> currentPos = new Stack<Person.Position>();
            currentPos.Push(Person.Position.Copy(person.Positions.Peek()));
            Person copy = new Person(person.Id, person.InitialAngle, person.InitialPosition, currentPos);
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
            
            Person p = obj as Person;
            if ((Object)p == null)
            {
                return false;
            }

            return (this.Id == p.Id) && (this.InitialAngle == p.InitialAngle) && (this.InitialPosition == p.InitialPosition);
        }

        public bool Equals(Person p)
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
