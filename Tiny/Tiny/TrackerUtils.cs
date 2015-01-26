
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Tiny
{
    public class TrackerUtils
    {
        // Calculate the coordinate differences
        public static void CalculateCoordinateDifferences(Tracker.Result result)
        {
            foreach (Tracker.Result.Person person in result.People)
            {
                double sum;
                double average;

                Dictionary<JointType, float> differences = new Dictionary<JointType, float>();
                foreach (Tracker.Result.KinectFOV fov in result.FOVs)
                {
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {

                    }
                }
            }
            foreach (SkeletonMatch reference in this.SkeletonMatches)
            {
                WBody referenceBody = reference.Skeleton.CurrentPosition.Worldview;
                Dictionary<JointType, int> jointCountsDict = new Dictionary<JointType, int>();
                foreach (SkeletonMatch other in this.SkeletonMatches)
                {
                    if (!reference.FOV.Equals(other.FOV))
                    {
                        WBody matchBody = other.Skeleton.CurrentPosition.Worldview;
                        IEnumerable<JointType> commonJoints = referenceBody.Joints.Keys.Union(matchBody.Joints.Keys);
                        // Sum coordinate differences
                        foreach (JointType jointType in commonJoints)
                        {
                            WCoordinate referenceJoint = referenceBody.Joints[jointType].Coordinate;
                            WCoordinate otherJoint = matchBody.Joints[jointType].Coordinate;
                            reference.AverageCoordinateDifferences[jointType] += WCoordinate.GetEuclideanDifference(referenceJoint, otherJoint);
                            int currentJointCount = 0;
                            jointCountsDict.TryGetValue(jointType, out currentJointCount);
                            jointCountsDict[jointType] = currentJointCount + 1;
                        }
                    }
                }
                // Average coordinate differences
                foreach (JointType jointType in reference.AverageCoordinateDifferences.Keys)
                {
                    if (jointCountsDict.ContainsKey(jointType))
                    {
                        reference.AverageCoordinateDifferences[jointType] = reference.AverageCoordinateDifferences[jointType] / jointCountsDict[jointType];
                    }
                }
            }
        }
    }
}
