using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using KinectSerializer;

namespace Tiny
{
    public class Person : IComparable<Person>
    {
        public class Position
        {
            private SBody kinect;
            private WBody worldview;

            public SBody Kinect
            {
                get
                {
                    return this.kinect;
                }
            }

            public WBody WorldView
            {
                get
                {
                    return this.worldview;
                }
            }

            public Position(SBody kinect, WBody worldview)
            {
                this.kinect = kinect;
                this.worldview = worldview;
            }

            public static Position Copy(Position c)
            {
                return new Position(SBody.Copy(c.Kinect), WBody.Copy(c.WorldView));
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

        public int CompareTo(Person other)
        {
            if (this.Id )
        }
    }
}
