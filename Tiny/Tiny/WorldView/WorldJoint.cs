using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny.WorldView
{
    public class WJoint
    {
        public WCoordinate Coordinate { get; private set; }
        public TrackingState TrackingState { get; private set; }

        public WJoint(WCoordinate coordinate, TrackingState trackingState)
        {
            this.Coordinate = coordinate;
            this.TrackingState = trackingState;
        }
    }
}
