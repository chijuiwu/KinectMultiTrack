
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Tiny.WorldView;
using KinectSerializer;
using System.Diagnostics;

namespace Tiny
{
    public class TrackerUtils
    {
        public static float CalculateCameraSpacePointDifference(CameraSpacePoint pt0, CameraSpacePoint pt1)
        {
            return (float) Math.Sqrt(pt0.X * pt1.X + pt0.Y * pt1.Y + pt0.Z * pt1.Z);
        }

        // Calculate the coordinate differences
        public static void CalculateCoordinateDifferences(Tracker.Result result)
        {
            foreach (Tracker.Result.Person person in result.People)
            {
                float average;
                Dictionary<JointType, int> jointCount = new Dictionary<JointType,int>();
                Dictionary<JointType, float> differences = new Dictionary<JointType, float>();
                foreach (JointType jt in BodyStructure.Joints)
                {
                    jointCount[jt] = 0;
                    differences[jt] = 0;
                }

                foreach (Tracker.Result.SkeletonMatch current in person.SkeletonMatches)
                {
                    Tracker.Result.KinectFOV currentFOV = current.FOV;
                    TrackingSkeleton skeleton = current.Skeleton;
                    WBody wvPos = skeleton.CurrentPosition.Worldview;
                    double initAngle = skeleton.InitialAngle;
                    WCoordinate initPos = skeleton.InitialPosition;
                    KinectSkeleton body = WBody.TransformBodyToKinectSkeleton(wvPos, initAngle, initPos);
            
                    foreach (Tracker.Result.SkeletonMatch other in person.SkeletonMatches)
                    {
                        if (!current.Equals(other))
                        {
                            TrackingSkeleton otherSkeleton = other.Skeleton;
                            WBody otherWVPosition = otherSkeleton.CurrentPosition.Worldview;
                            KinectSkeleton otherBody = WBody.TransformBodyToKinectSkeleton(otherWVPosition, initAngle, initPos);

                            IEnumerable<JointType> commonJoints = body.Joints.Keys.Intersect(otherBody.Joints.Keys);
                            foreach (JointType jt in commonJoints)
                            {
                                jointCount[jt]++;
                                differences[jt] += TrackerUtils.CalculateCameraSpacePointDifference(body.Joints[jt], otherBody.Joints[jt]);
                            }
                        }
                    }
                }

                Debug.WriteLine("Coordinate Differences...");
                foreach (JointType jt in differences.Keys)
                {
                    differences[jt] = differences[jt] / jointCount[jt];
                    Debug.WriteLine("JointType: " + jt + " Difference: " + differences[jt]);
                }

                average = differences.Values.Average();
                Debug.WriteLine("Average: " + average);
            }
        }
    }
}
