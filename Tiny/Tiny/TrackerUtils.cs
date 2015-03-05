
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
