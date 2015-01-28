
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;


namespace Tiny.UI
{
    class JointShape
    {
        public CameraSpacePoint Coordinate { get; private set; }
        public TrackingState TrackingState { get; private set; }

        public JointShape(CameraSpacePoint coordinate, TrackingState trackingState)
        {
            this.Coordinate = coordinate;
            this.TrackingState = trackingState;
        }
    }
}
