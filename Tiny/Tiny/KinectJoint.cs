
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny
{
    public class KinectJoint
    {
        public TrackingState TrackingState { get; private set; }
        public CameraSpacePoint Position { get; private set; }

        public KinectJoint(TrackingState state, CameraSpacePoint position)
        {
            this.TrackingState = state;
            this.Position = position;
        }
    }
}
