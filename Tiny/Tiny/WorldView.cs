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
    public class WorldView
    {
        private double initAngle;
        WorldCoordinate initCentrePosition;
        private int kinectDepthFrameWidth;
        private int kinectDepthFrameHeight;
        private Dictionary<ulong, WorldBody> bodyCoordinates;

        // Assume one user in each frame
        // TODO: scale to multiple users in one frame
        public WorldView(WorldBody worldBody, double initAngle, WorldCoordinate initCentrePosition, int kinectDepthWidth, int kinectDepthHeight)
        {
            this.initAngle = initAngle;
            this.initCentrePosition = initCentrePosition;
            this.kinectDepthFrameWidth = kinectDepthWidth;
            this.kinectDepthFrameHeight = kinectDepthHeight;
            this.bodyCoordinates = new Dictionary<ulong, WorldBody>();
            this.bodyCoordinates[0] = worldBody;
        }

        public double InitialAngle
        {
            get
            {
                return this.initAngle;
            }
        }

        public WorldCoordinate InitialCentrePosition
        {
            get
            {
                return this.initCentrePosition;
            }
        }

        public int KinectDepthFrameWidth
        {
            get
            {
                return this.kinectDepthFrameWidth;
            }
        }

        public int KinectDepthFrameHeight
        {
            get
            {
                return this.kinectDepthFrameHeight;
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
        public static WorldBody GetBodyWorldCoordinates(SerializableBody body, double initialAngle, WorldCoordinate centrePoint)
        {
            WorldBody bodyWorld = new WorldBody();

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

                bodyWorld.Joints[jointType] = new WorldCoordinate(transformedX, transformedY, transformedZ);
            }

            return bodyWorld;
        }

        // Joints transformed back to the Kinect camera space point
        public static KinectBody GetBodyKinectCoordinates(WorldBody body, double initialAngle, WorldCoordinate centrePoint)
        {
            KinectBody bodyKinect = new KinectBody();

            foreach(JointType jointType in body.Joints.Keys)
            {
                WorldCoordinate jointWorld = body.Joints[jointType];

                double sinAngle = Math.Sin(initialAngle);
                double cosAngle = Math.Cos(initialAngle);

                double[,] matrix = new double[2, 2];
                matrix[0, 0] = cosAngle;
                matrix[0, 1] = sinAngle;
                matrix[1, 0] = -sinAngle;
                matrix[1, 1] = cosAngle;

                double determinant = 1 / (matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0]);

                double[,] swappedMatrix = new double[2, 2];
                swappedMatrix[0, 0] = matrix[1, 1];
                swappedMatrix[0, 1] = -matrix[0, 1];
                swappedMatrix[1, 0] = -matrix[1, 0];
                swappedMatrix[1, 1] = matrix[0, 0];

                double[,] inverseMatrix = new double[2, 2];
                inverseMatrix[0, 0] = determinant * swappedMatrix[0, 0];
                inverseMatrix[0, 1] = determinant * swappedMatrix[0, 1];
                inverseMatrix[1, 0] = determinant * swappedMatrix[1, 0];
                inverseMatrix[1, 1] = determinant * swappedMatrix[1, 1];

                float translatedX = (float)(inverseMatrix[0, 0] * jointWorld.X + inverseMatrix[0, 1] * jointWorld.Z);
                float translatedY = jointWorld.Y;
                float translatedZ = (float)(inverseMatrix[1, 0] * jointWorld.X + inverseMatrix[1, 1] * jointWorld.Z);

                CameraSpacePoint jointKinect = new CameraSpacePoint();
                jointKinect.X = translatedX + centrePoint.X;
                jointKinect.Y = translatedY + centrePoint.Y;
                jointKinect.Z = translatedZ + centrePoint.Z;

                bodyKinect.Joints[jointType] = jointKinect;
            }

            return bodyKinect;
        }
    }
}
