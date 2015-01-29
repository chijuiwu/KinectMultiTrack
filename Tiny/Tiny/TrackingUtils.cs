
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
        public static Tracker.Result.SkeletonMatch GetReferenceMatch(Tracker.Result.Person person)
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

        public static IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> GetRawCoordinates(Tracker.Result.Person person)
        {
            Tracker.Result.SkeletonMatch reference = TrackingUtils.GetReferenceMatch(person);
            Tracker.Result.KinectFOV referenceFOV = reference.FOV;
            TrackingSkeleton referenceSkeleton = reference.Skeleton;

            List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> raw = new List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>>();
            foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
            {
                WBody position = match.Skeleton.CurrentPosition.Worldview;
                KinectBody body = WBody.TransformToKinectBody(position, referenceSkeleton.InitialAngle, referenceSkeleton.InitialPosition);
                raw.Add(Tuple.Create(match, body.Joints));
            }
            return raw;
        }

        public static Dictionary<JointType, CameraSpacePoint> GetAverages(IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> raw)
        {
            Dictionary<JointType, CameraSpacePoint> sum = new Dictionary<JointType, CameraSpacePoint>();
            Dictionary<JointType, int> count = new Dictionary<JointType, int>();

            foreach (Dictionary<JointType, CameraSpacePoint> jointPositions in raw.Select(t=>t.Item2))
            {
                foreach (JointType jt in jointPositions.Keys)
                {
                    if (sum.ContainsKey(jt))
                    {
                        CameraSpacePoint newSum = new CameraSpacePoint();
                        newSum.X = sum[jt].X + jointPositions[jt].X;
                        newSum.Y = sum[jt].Y + jointPositions[jt].Y;
                        newSum.Z = sum[jt].Z + jointPositions[jt].Z;
                        sum[jt] = newSum;
                        count[jt] += 1;
                    }
                    else
                    {
                        sum[jt] = jointPositions[jt];
                        count[jt] = 1;
                    }
                }
            }
         
            Dictionary<JointType, CameraSpacePoint> averages = new Dictionary<JointType, CameraSpacePoint>();
            foreach (JointType jt in sum.Keys)
            {
                CameraSpacePoint averagePt = new CameraSpacePoint();
                averagePt.X = sum[jt].X / (float)count[jt];
                averagePt.Y = sum[jt].Y / (float)count[jt];
                averagePt.Z = sum[jt].Z / (float)count[jt];
                averages[jt] = averagePt;
            }
            return averages;
        }

        public static IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> GetDifferences(IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> raw, Dictionary<JointType, CameraSpacePoint> averages)
        {
            List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>> differences = new List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>>>();
            foreach (Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, CameraSpacePoint>> skeletonCoordinatesTuple in raw)
            {   
                Tracker.Result.SkeletonMatch skeletonMatch = skeletonCoordinatesTuple.Item1;
                Dictionary<JointType, CameraSpacePoint> coordinates = skeletonCoordinatesTuple.Item2;

                Dictionary<JointType, CameraSpacePoint> differencePerSkeleton = new Dictionary<JointType, CameraSpacePoint>();
                foreach (JointType jt in coordinates.Keys)
                {
                    CameraSpacePoint rawPt = coordinates[jt];
                    CameraSpacePoint averagePt = averages[jt];
                    CameraSpacePoint differencePt = new CameraSpacePoint();
                    differencePt.X = (float)Math.Pow(rawPt.X - averagePt.X, 2);
                    differencePt.Y = (float)Math.Pow(rawPt.Y - averagePt.Y, 2);
                    differencePt.Z = (float)Math.Pow(rawPt.Z - averagePt.Z, 2);
                    differencePerSkeleton[jt] = differencePt;
                }
                differences.Add(Tuple.Create(skeletonMatch, differencePerSkeleton));
            }
            return differences;
        }
    }
}
