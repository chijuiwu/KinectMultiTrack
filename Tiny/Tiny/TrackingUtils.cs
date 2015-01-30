
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Tiny.WorldView;

namespace Tiny
{
    public class TrackingUtils
    {
        public static Tracker.Result.SkeletonMatch GetLocalSkeletonReference(Tracker.Result.Person person)
        {
            foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
            {
                if (match.FOV.ClientIP.Address.ToString().Equals("127.0.0.1"))
                {
                    return match;
                }
            }
            return person.SkeletonMatches.First();
        }

        public static Dictionary<JointType, KinectJoint> GetKinectJoints(Tracker.Result.SkeletonMatch match, TrackingSkeleton reference)
        {
            WBody position = match.Skeleton.CurrentPosition.Worldview;
            KinectBody body = WBody.TransformToKinectBody(position, reference.InitialAngle, reference.InitialPosition);
            return body.Joints;
        }

        public static Dictionary<JointType, KinectJoint> GetAverages(IEnumerable<Dictionary<JointType, KinectJoint>> jointsList)
        {
            Dictionary<JointType, CameraSpacePoint> sum = new Dictionary<JointType, CameraSpacePoint>();
            Dictionary<JointType, int> count = new Dictionary<JointType, int>();
            foreach (Dictionary<JointType, KinectJoint> joints in jointsList)
            {
                foreach (JointType jt in joints.Keys)
                {
                    if (sum.ContainsKey(jt))
                    {
                        CameraSpacePoint newSum = new CameraSpacePoint();
                        newSum.X = sum[jt].X + joints[jt].Coordinate.X;
                        newSum.Y = sum[jt].Y + joints[jt].Coordinate.Y;
                        newSum.Z = sum[jt].Z + joints[jt].Coordinate.Z;
                        sum[jt] = newSum;
                        count[jt] += 1;
                    }
                    else
                    {
                        sum[jt] = joints[jt].Coordinate;
                        count[jt] = 1;
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

        public static IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> GetDifferences(Dictionary<JointType, KinectJoint> averages, IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> raw)
        {
            List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> differences = new List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>>();
            foreach (Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>> skeletonCoordinatesTuple in raw)
            {   
                Tracker.Result.SkeletonMatch skeletonMatch = skeletonCoordinatesTuple.Item1;
                Dictionary<JointType, KinectJoint> coordinates = skeletonCoordinatesTuple.Item2;

                Dictionary<JointType, KinectJoint> differencePerSkeleton = new Dictionary<JointType, KinectJoint>();
                foreach (JointType jt in coordinates.Keys)
                {
                    CameraSpacePoint rawPt = coordinates[jt].Coordinate;
                    CameraSpacePoint averagePt = averages[jt].Coordinate;
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
