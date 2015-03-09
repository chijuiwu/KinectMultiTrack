using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectMultiTrack
{
    public class KinectBody
    {
        public Dictionary<JointType, KinectJoint> Joints { get; private set; }

        public KinectBody()
        {
            this.Joints = new Dictionary<JointType, KinectJoint>();
        }

        public static KinectBody GetAverageBody(IEnumerable<KinectBody> bodies)
        {
            Dictionary<JointType, CameraSpacePoint> sum = new Dictionary<JointType, CameraSpacePoint>();
            Dictionary<JointType, uint> count = new Dictionary<JointType, uint>();
            foreach (KinectBody body in bodies)
            {
                foreach (JointType jt in body.Joints.Keys)
                {
                    if (body.Joints[jt].TrackingState == TrackingState.Inferred)
                    {
                        // Don't use inferred points to calculate the average
                        continue;
                    }
                    if (!sum.ContainsKey(jt))
                    {
                        sum[jt] = body.Joints[jt].Position;
                        count[jt] = 1;
                    }
                    else
                    {
                        CameraSpacePoint newSum = new CameraSpacePoint();
                        newSum.X = sum[jt].X + body.Joints[jt].Position.X;
                        newSum.Y = sum[jt].Y + body.Joints[jt].Position.Y;
                        newSum.Z = sum[jt].Z + body.Joints[jt].Position.Z;
                        sum[jt] = newSum;
                        count[jt]++;
                    }
                }
            }
            KinectBody average = new KinectBody();
            foreach (JointType jt in sum.Keys)
            {
                CameraSpacePoint averagePt = new CameraSpacePoint();
                averagePt.X = sum[jt].X / (float)count[jt];
                averagePt.Y = sum[jt].Y / (float)count[jt];
                averagePt.Z = sum[jt].Z / (float)count[jt];
                average.Joints[jt] = new KinectJoint(TrackingState.Tracked, averagePt);
            }
            return average;
        }
    }
}
