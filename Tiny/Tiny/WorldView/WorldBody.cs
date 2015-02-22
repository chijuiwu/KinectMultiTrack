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

        // Return the initial angle between user and Kinect
        public static double GetInitialAngle(SBody body)
        {
            SJoint shoulderLeft = body.Joints[JointType.ShoulderLeft];
            SJoint shoulderRight = body.Joints[JointType.ShoulderRight];
            if (shoulderLeft.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Calculating initial angle]: ShoulderLeft joint not tracked");
            }
            else if (shoulderRight.TrackingState.Equals(TrackingState.NotTracked))
            {
                throw new UntrackedJointException("[Calculating initial angle]: ShoulderRight joint not tracked");
            }
            CameraSpacePoint shoulderLeftPos = shoulderLeft.CameraSpacePoint;
            CameraSpacePoint shoulderRightPos = shoulderRight.CameraSpacePoint;
            float lengthOpposite = shoulderRightPos.Z - shoulderLeftPos.Z;
            float lengthAdjacent = shoulderRightPos.X - shoulderLeftPos.X;
            return Math.Atan2(lengthOpposite, lengthAdjacent);
        }

        // Return the initial centre position of the user's body
        // Each item in the array is the body at a given time frame during the initial data collection
        public static WCoordinate GetInitialPosition(List<SBody> initialBodies)
        {
            float totalAverageX = 0, totalAverageY = 0, totalAverageZ = 0;
            int countBodies = 0;
            foreach (SBody body in initialBodies)
            {
                countBodies++;
                float sumXs = 0, sumYs = 0, sumZs = 0;
                int countXs = 0, countYs = 0, countZs = 0;
                foreach (JointType jt in SkeletonStructure.Joints)
                {
                    if (!body.Joints.ContainsKey(jt))
                    {
                        continue;
                    }
                    if (body.Joints[jt].TrackingState.Equals(TrackingState.NotTracked))
                    {
                        continue;
                    }
                    sumXs += body.Joints[jt].CameraSpacePoint.X;
                    sumYs += body.Joints[jt].CameraSpacePoint.Y;
                    sumZs += body.Joints[jt].CameraSpacePoint.Z;
                    countXs++;
                    countYs++;
                    countZs++;
                }
                totalAverageX += sumXs / (float)countXs;
                totalAverageY += sumYs / (float)countYs;
                totalAverageZ += sumZs / (float)countZs;
            }
            float centreX = totalAverageX / (float)countBodies;
            float centreY = totalAverageY / (float)countBodies;
            float centreZ = totalAverageZ / (float)countBodies;
            return new WCoordinate(centreX, centreY, centreZ);
        }

        // Joints transformed to the origin of the world coordinate system
        public static WBody Create(SBody body, double initialAngle, WCoordinate initialCenterPosition)
        {
            WBody worldviewBody = new WBody();
            foreach (JointType jointType in body.Joints.Keys)
            {
                SJoint joint = body.Joints[jointType];
                CameraSpacePoint jointPosition = joint.CameraSpacePoint;

                // Translation
                float translatedX = jointPosition.X - initialCenterPosition.X;
                float translatedY = jointPosition.Y - initialCenterPosition.Y;
                float translatedZ = jointPosition.Z - initialCenterPosition.Z;

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

            foreach (JointType jt in body.Joints.Keys)
            {
                WCoordinate worldviewJoint = body.Joints[jt].Coordinate;

                double sinAngle = Math.Sin(initialAngle);
                double cosAngle = Math.Cos(initialAngle);

                double[,] matrix = new double[2, 2];
                matrix[0, 0] = cosAngle;
                matrix[0, 1] = sinAngle;
                matrix[1, 0] = -sinAngle;
                matrix[1, 1] = cosAngle;

                double determinant = 1 / (matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0]);

                double[,] inverseMatrix = new double[2, 2];
                inverseMatrix[0, 0] = matrix[1, 1];
                inverseMatrix[0, 1] = -matrix[0, 1];
                inverseMatrix[1, 0] = -matrix[1, 0];
                inverseMatrix[1, 1] = matrix[0, 0];
                inverseMatrix[0, 0] = determinant * inverseMatrix[0, 0];
                inverseMatrix[0, 1] = determinant * inverseMatrix[0, 1];
                inverseMatrix[1, 0] = determinant * inverseMatrix[1, 0];
                inverseMatrix[1, 1] = determinant * inverseMatrix[1, 1];

                float translatedX = (float)(inverseMatrix[0, 0] * worldviewJoint.X + inverseMatrix[0, 1] * worldviewJoint.Z);
                float translatedY = worldviewJoint.Y;
                float translatedZ = (float)(inverseMatrix[1, 0] * worldviewJoint.X + inverseMatrix[1, 1] * worldviewJoint.Z);

                CameraSpacePoint coordinate = new CameraSpacePoint();
                coordinate.X = translatedX + centrePoint.X;
                coordinate.Y = translatedY + centrePoint.Y;
                coordinate.Z = translatedZ + centrePoint.Z;

                kinectSkeleton.Joints[jt] = new KinectJoint(body.Joints[jt].TrackingState, coordinate);
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
