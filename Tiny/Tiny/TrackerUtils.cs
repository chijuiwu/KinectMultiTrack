
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
        public static TResult.SkeletonReplica GetLocalSkeletonReference(TResult.Person person)
        {
            foreach (TResult.SkeletonReplica match in person.Replicas)
            {
                if (match.FOV.ClientIP.Address.ToString().Equals("127.0.0.1"))
                {
                    return match;
                }
            }
            return person.Replicas.First();
        }

        public static Dictionary<JointType, KinectJoint> GetKinectJoints(TResult.SkeletonReplica match, TSkeleton reference)
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
                        sum[jt] = kinectJoints[jt].Coordinate;
                        count[jt] = 1;
                    }
                    else
                    {
                        CameraSpacePoint newSum = new CameraSpacePoint();
                        newSum.X = sum[jt].X + kinectJoints[jt].Coordinate.X;
                        newSum.Y = sum[jt].Y + kinectJoints[jt].Coordinate.Y;
                        newSum.Z = sum[jt].Z + kinectJoints[jt].Coordinate.Z;
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

        public static IEnumerable<Tuple<TResult.SkeletonReplica, Dictionary<JointType, KinectJoint>>> GetDifferences(Dictionary<JointType, KinectJoint> averages, IEnumerable<Tuple<TResult.SkeletonReplica, Dictionary<JointType, KinectJoint>>> raw)
        {
            List<Tuple<TResult.SkeletonReplica, Dictionary<JointType, KinectJoint>>> differences = new List<Tuple<TResult.SkeletonReplica, Dictionary<JointType, KinectJoint>>>();
            foreach (Tuple<TResult.SkeletonReplica, Dictionary<JointType, KinectJoint>> skeletonCoordinatesTuple in raw)
            {   
                TResult.SkeletonReplica skeletonMatch = skeletonCoordinatesTuple.Item1;
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
