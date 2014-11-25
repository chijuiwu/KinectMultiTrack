using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectSerializer;
using Tiny.Exceptions;

namespace Tiny
{
    class WorldCoordinateSystem
    {
        
        // Get the initial angle between user and Kinect
        public static double GetInitialAngle(SerializableBody body)
        {
            SerializableJoint shoulderLeft = body.Joints[JointType.ShoulderLeft];
            SerializableJoint shoulderRight = body.Joints[JointType.ShoulderRight];

            if (shoulderLeft.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Getting Initial Angle]: ShoulderLeft joint not tracked");
            }
            else if (shoulderRight.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Getting Initial Angle]: ShoulderRight joint not tracked");
            }

            CameraSpacePoint shoulderLeftPos = shoulderLeft.CameraSpacePoint;
            CameraSpacePoint shoulderRightPos = shoulderRight.CameraSpacePoint;

            float lengthAdjacent = shoulderRightPos.X - shoulderLeftPos.X;
            float lengthOpposite = shoulderRightPos.Z - shoulderLeftPos.Z;

            return Math.Atan(lengthOpposite/lengthAdjacent);
        }

        // Get the initial centre position of user's body
        public CameraSpacePoint GetInitialCentrePosition(SerializableBody body)
        {
            float sumOfXs = 0;
            float sumOfYs = 0;
            float sumOfZs = 0;
        }

    }
}
