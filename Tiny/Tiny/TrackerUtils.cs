
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
        public static IEnumerable<Tuple<TrackerResult.PotentialSkeleton, KinectBody>> GetDifferences(KinectBody average, IEnumerable<Tuple<TrackerResult.PotentialSkeleton, KinectBody>> kinectBodies)
        {
            List<Tuple<TrackerResult.PotentialSkeleton, KinectBody>> differences = new List<Tuple<TrackerResult.PotentialSkeleton, KinectBody>>();
            foreach (Tuple<TrackerResult.PotentialSkeleton, KinectBody> skeletonCoordinatesTuple in kinectBodies)
            {   
                TrackerResult.PotentialSkeleton skeletonMatch = skeletonCoordinatesTuple.Item1;
                KinectBody body = skeletonCoordinatesTuple.Item2;

                KinectBody differencePerSkeleton = new KinectBody();
                foreach (JointType jt in body.Joints.Keys)
                {
                    CameraSpacePoint rawPt = body.Joints[jt].Position;
                    CameraSpacePoint averagePt = average.Joints[jt].Position;
                    CameraSpacePoint differencePt = new CameraSpacePoint();
                    differencePt.X = (float)Math.Pow(rawPt.X - averagePt.X, 2);
                    differencePt.Y = (float)Math.Pow(rawPt.Y - averagePt.Y, 2);
                    differencePt.Z = (float)Math.Pow(rawPt.Z - averagePt.Z, 2);
                    differencePerSkeleton.Joints[jt] = new KinectJoint(body.Joints[jt].TrackingState, differencePt);
                }
                differences.Add(Tuple.Create(skeletonMatch, differencePerSkeleton));
            }
            return differences;
        }
    }
}
