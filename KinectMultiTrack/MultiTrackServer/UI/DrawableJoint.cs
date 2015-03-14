using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace KinectMultiTrack.UI
{
    internal class DrawableJoint
    {
        public Point Point { get; private set;}
        public TrackingState TrackingState { get; private set; }

        public DrawableJoint(Point point, TrackingState trackingState)
        {
            this.Point = point;
            this.TrackingState = trackingState;
        }
    }
}
