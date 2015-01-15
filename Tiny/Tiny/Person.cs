using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Tiny
{
    class Person
    {
        private double initAngle;
        private WorldCoordinate initPosition;
        private ulong trackingId;

        public Person()
        {
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

        public WorldCoordinate InitialPosition
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

        public ulong TrackingId
        {
            get
            {
                return this.trackingId;
            }
            set
            {
                this.trackingId = value;
            }
        }
    }
}
