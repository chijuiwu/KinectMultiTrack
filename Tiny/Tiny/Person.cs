using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using KinectSerializer;

namespace Tiny
{
    public class Person
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

        private ulong id;
        private double initAngle;
        private WCoordinate initPos;
        private Stack<Person.Position> positions;

        public ulong Id
        {
            get
            {
                return this.id;
            }
        }

        public double InitialAngle
        {
            get
            {
                return this.initAngle;
            }
            set
            {
                this.initAngle = value;
            }
        }

        public WCoordinate InitialPosition
        {
            get
            {
                return this.initPos;
            }
            set
            {
                this.InitialPosition = value;
            }
        }

        public Person.Position CurrentPosition
        {
            get
            {
                return this.positions.Peek();
            }
        }

        public Person(ulong id)
        {
            this.id = id;
            this.positions = new Stack<Person.Position>();
        }

        public Person(ulong id, double initAngle, WCoordinate initPos, Stack<Person.Position> positions)
        {
            this.id = id;
            this.initAngle = initAngle;
            this.initPos = initPos;
            this.positions = positions;
        }

        public void UpdatePosition(SBody body, WBody worldviewBody)
        {
            this.positions.Push(new Person.Position(body, worldviewBody));
        }

        // Copy only the current positions
        public static Person Copy(Person person)
        {
            Stack<Person.Position> currentPos = new Stack<Person.Position>();
            currentPos.Push(Person.Position.Copy(person.positions.Peek()));
            Person copy = new Person(person.Id, person.InitialAngle, person.InitialPosition, currentPos);
            return copy;
        }
    }
}
