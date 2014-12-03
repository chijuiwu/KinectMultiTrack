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
    class WorldView
    {
        private int depthFrameWidth;
        private int depthFrameHeight;

        private Dictionary<ulong, WorldBody> bodyCoordinates;

        // Assume one user in each frame
        // TODO: scale to multiple users in one frame
        public WorldView(SerializableBodyFrame bodyFrame, WorldBody worldBody)
        {
            this.depthFrameWidth = bodyFrame.DepthFrameWidth;
            this.depthFrameHeight = bodyFrame.DepthFrameHeight;
            this.bodyCoordinates = new Dictionary<ulong, WorldBody>();
            this.bodyCoordinates[0] = worldBody;
        }

        public int DepthFrameWidth
        {
            get
            {
                return this.depthFrameWidth;
            }
        }

        public int DepthFrameHeight
        {
            get
            {
                return this.depthFrameHeight;
            }
        }

        public IEnumerable<WorldBody> WorldBoides
        {
            get
            {
                foreach(WorldBody body in bodyCoordinates.Values)
                {
                    yield return body;
                }
            }
        }

        // Get the initial angle between user and Kinect
        public static double GetInitialAngle(SerializableBody body)
        {
            SerializableJoint shoulderLeft = body.Joints[JointType.ShoulderLeft];
            SerializableJoint shoulderRight = body.Joints[JointType.ShoulderRight];

            if (shoulderLeft.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Getting initial angle]: ShoulderLeft joint not tracked");
            }
            else if (shoulderRight.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Getting initial angle]: ShoulderRight joint not tracked");
            }

            CameraSpacePoint shoulderLeftPos = shoulderLeft.CameraSpacePoint;
            CameraSpacePoint shoulderRightPos = shoulderRight.CameraSpacePoint;

            float lengthAdjacent = shoulderRightPos.X - shoulderLeftPos.X;
            float lengthOpposite = shoulderRightPos.Z - shoulderLeftPos.Z;

            return Math.Atan(lengthOpposite/lengthAdjacent);
        }

        // Get the initial centre position of user's body
        // Each item in the array is a body at a particular frame in the initial data collection sequence
        public static WorldCoordinate GetInitialCentrePosition(SerializableBody[] userInitialBodies)
        {
            float totalAverageX = 0;
            float totalAverageY = 0;
            float totalAverageZ = 0;

            foreach (SerializableBody body in userInitialBodies)
            {
                float sumOfXs = 0;
                float sumOfYs = 0;
                float sumOfZs = 0;

                foreach(JointType jointType in BodyStructure.Joints)
                {
                    if (!body.Joints.ContainsKey(jointType))
                    {
                        throw new UntrackedJointException("[Getting initial centre position]: " + jointType + " not unavialble");
                    }
                    SerializableJoint joint = body.Joints[jointType];
                    if (joint.TrackingState.Equals(TrackingState.NotTracked))
                    {
                        throw new UntrackedJointException("[Getting initial centre position]: " + jointType + " not tracked");
                    }
                    sumOfXs += joint.CameraSpacePoint.X;
                    sumOfYs += joint.CameraSpacePoint.Y;
                    sumOfZs += joint.CameraSpacePoint.Z;
                }

                totalAverageX += sumOfXs / BodyStructure.Joints.Count;
                totalAverageY += sumOfYs / BodyStructure.Joints.Count;
                totalAverageZ += sumOfZs / BodyStructure.Joints.Count;
            }

            float centreX = totalAverageX / userInitialBodies.Length;
            float centreY = totalAverageY / userInitialBodies.Length;
            float centreZ = totalAverageZ / userInitialBodies.Length;
            return new WorldCoordinate(centreX, centreY, centreZ);
        }

        // Joints transformed to the origin of the world coordinate system
        public static WorldBody GetTransformedBody(SerializableBody body, double initialAngle, WorldCoordinate centrePoint)
        {
            WorldBody transformedWorldBody = new WorldBody();

            foreach(JointType jointType in body.Joints.Keys)
            {
                SerializableJoint joint = body.Joints[jointType];
                CameraSpacePoint jointPosition = joint.CameraSpacePoint;

                // Translation
                float translatedX = jointPosition.X - centrePoint.X;
                float translatedY = jointPosition.Y - centrePoint.Y;
                float translatedZ = jointPosition.Z - centrePoint.Z;

                // Rotation
                float transformedX = (float) (translatedX * Math.Cos(initialAngle) + translatedZ * Math.Sin(initialAngle));
                float transformedY = translatedY;
                float transformedZ = (float) (translatedZ * Math.Cos(initialAngle) - translatedX * Math.Sin(initialAngle));

                transformedWorldBody.Joints[jointType] = new WorldCoordinate(transformedX, transformedY, transformedZ);
            }

            return transformedWorldBody;
        }
    }
}
