using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using KinectSerializer;

namespace Tiny
{
    class Person
    {
        private double initAngle;
        private WCoordinate initPosition;
        private Stack<Tuple<SBody, WBody>> previousPositions;

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
                return this.initPosition;
            }
            set
            {
                this.InitialPosition = value;
            }
        }

        public Person()
        {
            this.previousPositions = new Stack<Tuple<SBody, WBody>>();
        }

        public void UpdatePosition(SBody body, WBody worldviewBody)
        {
            this.previousPositions.Push(Tuple.Create(body, worldviewBody));
        }
    }
}
