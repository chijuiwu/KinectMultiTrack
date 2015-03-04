
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Tiny.WorldView;

namespace Tiny
{
    public class TUtils
    {
        public static TrackerResult.PotentialSkeleton GetLocalSkeletonReference(TrackerResult.Person person)
        {
            foreach (TrackerResult.PotentialSkeleton match in person.SkeletonsList)
            {
                if (match.FOV.ClientIP.Address.ToString().Equals("127.0.0.1"))
                {
                    return match;
                }
            }
            return person.SkeletonsList.First();
        }

        public static Dictionary<JointType, KinectJoint> GetKinectJoints(TrackerResult.PotentialSkeleton match, MovingSkeleton reference)
        {
            WBody position = match.Skeleton.CurrentPosition.Worldview;
            KinectBody body = WBody.TransformToKinectBody(position, reference.InitialAngle, reference.InitialPosition);
            return body.Joints;
        }

        // 
        public static Dictionary<JointType, KinectJoint> GetAverages(IEnumerable<Dictionary<JointType, KinectJoint>> kinectCollection)
        {
            Dictionary<JointType, CameraSpacePoint> sum = new Dictionary<JointType, CameraSpacePoint>();
            Dictionary<JointType, uint> count = new Dictionary<JointType, uint>();
            foreach (Dictionary<JointType, KinectJoint> kinectJoints in kinectCollection)
            {
                foreach (JointType jt in kinectJoints.Keys)
                {
                    if (kinectJoints[jt].TrackingState == TrackingState.Inferred)
                    {
                        // Don't use inferred points to calculate the average
                        continue;
                    }
                    if (!sum.ContainsKey(jt))
                    {
                        sum[jt] = kinectJoints[jt].Position;
                        count[jt] = 1;
                    }
                    else
                    {
                        CameraSpacePoint newSum = new CameraSpacePoint();
                        newSum.X = sum[jt].X + kinectJoints[jt].Position.X;
                        newSum.Y = sum[jt].Y + kinectJoints[jt].Position.Y;
                        newSum.Z = sum[jt].Z + kinectJoints[jt].Position.Z;
                        sum[jt] = newSum;
                        count[jt]++;
                    }
                }
            }
            Dictionary<JointType, KinectJoint> averages = new Dictionary<JointType, KinectJoint>();
            foreach (JointType jt in sum.Keys)
            {
                CameraSpacePoint averagePt = new CameraSpacePoint();
                averagePt.X = sum[jt].X / (float)count[jt];
                averagePt.Y = sum[jt].Y / (float)count[jt];
                averagePt.Z = sum[jt].Z / (float)count[jt];
                averages[jt] = new KinectJoint(TrackingState.Tracked, averagePt);
            }
            return averages;
        }

        public static IEnumerable<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> GetDifferences(Dictionary<JointType, KinectJoint> averages, IEnumerable<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> raw)
        {
            List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> differences = new List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>>();
            foreach (Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>> skeletonCoordinatesTuple in raw)
            {   
                TrackerResult.PotentialSkeleton skeletonMatch = skeletonCoordinatesTuple.Item1;
                Dictionary<JointType, KinectJoint> coordinates = skeletonCoordinatesTuple.Item2;

                Dictionary<JointType, KinectJoint> differencePerSkeleton = new Dictionary<JointType, KinectJoint>();
                foreach (JointType jt in coordinates.Keys)
                {
                    CameraSpacePoint rawPt = coordinates[jt].Position;
                    CameraSpacePoint averagePt = averages[jt].Position;
                    CameraSpacePoint differencePt = new CameraSpacePoint();
                    differencePt.X = (float)Math.Pow(rawPt.X - averagePt.X, 2);
                    differencePt.Y = (float)Math.Pow(rawPt.Y - averagePt.Y, 2);
                    differencePt.Z = (float)Math.Pow(rawPt.Z - averagePt.Z, 2);
                    differencePerSkeleton[jt] = new KinectJoint(coordinates[jt].TrackingState, differencePt);
                }
                differences.Add(Tuple.Create(skeletonMatch, differencePerSkeleton));
            }
            return differences;
        }
    }
}
