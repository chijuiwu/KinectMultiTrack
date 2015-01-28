using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectSerializer;
using Microsoft.Kinect;
using Tiny.Exceptions;

namespace Tiny.WorldView
{
    public class WBody
    {
        public Dictionary<JointType, WJoint> Joints { get; private set; }

        public WBody()
        {
            this.Joints = new Dictionary<JointType, WJoint>();
        }

        // Get the initial angle between user and Kinect
        public static double GetInitialAngle(SBody body)
        {
            SJoint shoulderLeft = body.Joints[JointType.ShoulderLeft];
            SJoint shoulderRight = body.Joints[JointType.ShoulderRight];

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

            return Math.Atan(lengthOpposite / lengthAdjacent);
        }

        // Get the initial centre position of user's body
        // Each item in the array is a body at a particular frame in the initial data collection sequence
        public static WCoordinate GetInitialPosition(List<SBody> userInitialBodies)
        {
            float totalAverageX = 0;
            float totalAverageY = 0;
            float totalAverageZ = 0;

            foreach (SBody body in userInitialBodies)
            {
                float sumOfXs = 0;
                float sumOfYs = 0;
                float sumOfZs = 0;

                foreach (JointType jointType in BodyStructure.Joints)
                {
                    if (!body.Joints.ContainsKey(jointType))
                    {
                        throw new UntrackedJointException("[Getting initial centre position]: " + jointType + " not unavialble");
                    }
                    SJoint joint = body.Joints[jointType];
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

            float centreX = totalAverageX / userInitialBodies.Count;
            float centreY = totalAverageY / userInitialBodies.Count;
            float centreZ = totalAverageZ / userInitialBodies.Count;
            return new WCoordinate(centreX, centreY, centreZ);
        }

        // Joints transformed to the origin of the world coordinate system
        public static WBody Create(SBody body, double initialAngle, WCoordinate centrePoint)
        {
            WBody worldviewBody = new WBody();

            foreach (JointType jointType in body.Joints.Keys)
            {
                SJoint joint = body.Joints[jointType];
                CameraSpacePoint jointPosition = joint.CameraSpacePoint;

                // Translation
                float translatedX = jointPosition.X - centrePoint.X;
                float translatedY = jointPosition.Y - centrePoint.Y;
                float translatedZ = jointPosition.Z - centrePoint.Z;

                // Rotation
                float transformedX = (float)(translatedX * Math.Cos(initialAngle) + translatedZ * Math.Sin(initialAngle));
                float transformedY = translatedY;
                float transformedZ = (float)(translatedZ * Math.Cos(initialAngle) - translatedX * Math.Sin(initialAngle));

                WCoordinate coordinate = new WCoordinate(transformedX, transformedY, transformedZ);
                worldviewBody.Joints[jointType] = new WJoint(coordinate, joint.TrackingState);
            }

            return worldviewBody;
        }

        // Joints transformed back to the Kinect camera space point
        public static KinectBody TransformToKinectBody(WBody body, double initialAngle, WCoordinate centrePoint)
        {
            KinectBody kinectSkeleton = new KinectBody();

            foreach (JointType jointType in body.Joints.Keys)
            {
                WJoint worldviewJoint = body.Joints[jointType];
                WCoordinate worldviewJointPos = worldviewJoint.Coordinate;

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

                float translatedX = (float)(inverseMatrix[0, 0] * worldviewJointPos.X + inverseMatrix[0, 1] * worldviewJointPos.Z);
                float translatedY = worldviewJointPos.Y;
                float translatedZ = (float)(inverseMatrix[1, 0] * worldviewJointPos.X + inverseMatrix[1, 1] * worldviewJointPos.Z);

                CameraSpacePoint jointKinect = new CameraSpacePoint();
                jointKinect.X = translatedX + centrePoint.X;
                jointKinect.Y = translatedY + centrePoint.Y;
                jointKinect.Z = translatedZ + centrePoint.Z;

                kinectSkeleton.Joints[jointType] = jointKinect;
            }

            return kinectSkeleton;
        }

        public static WBody Copy(WBody body)
        {
            WBody copy = new WBody();
            foreach (JointType jointType in body.Joints.Keys)
            {
                WJoint joint = body.Joints[jointType];
                copy.Joints[jointType] = new WJoint(WCoordinate.Copy(joint.Coordinate), joint.TrackingState);
            }
            return copy;
        }

        public static double CalculateDifferences(WBody body0, WBody body1)
        {
            double diff = 0;
            IEnumerable<JointType> commonJoints = body0.Joints.Keys.Intersect(body1.Joints.Keys);
            foreach (JointType jointType in commonJoints)
            {
                WJoint joint0 = body0.Joints[jointType];
                WJoint joint1 = body1.Joints[jointType];
                diff += WCoordinate.CalculateDifference(joint0.Coordinate, joint1.Coordinate);
            }
            return diff;
        }
    }
}
