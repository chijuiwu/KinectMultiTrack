using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectSerializer;
using System.Windows;
using System.Windows.Media;
using SkeletonVis = KinectMultiTrack.UI.SkeletonVisualizer;
using System.Windows.Threading;

namespace KinectMultiTrack.UI
{
    public class SkeletonVisualizer
    {
        private static readonly Brush backgroundBrush = Brushes.Black;
        
        // Joints
        private static readonly double jointThickness = 3;
        private static readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private static readonly Brush inferredJointBrush = Brushes.Yellow;
        
        // Bones
        private static readonly Pen defaultTrackedBonePen = new Pen(Brushes.Blue, 6);
        private static readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public static void DrawBackground(int frameWidth, int frameHeight, DrawingContext dc)
        {
            dc.DrawRectangle(SkeletonVis.backgroundBrush, null, new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        public static void DrawClipRegion(int frameWidth, int frameHeight, DrawingGroup dg)
        {
            dg.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        public static void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc)
        {
            SkeletonVis.DrawBody(joints, dc, SkeletonVis.defaultTrackedBonePen);
        }

        public static void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc, Pen bonePen)
        {
            // Draw bones
            foreach (var bone in SkeletonStructure.Bones)
            {
                JointType jt0 = bone.Item1;
                JointType jt1 = bone.Item2;
                Point jointPt0 = joints[jt0].Item1;
                Point jointPt1 = joints[jt1].Item1;
                TrackingState joint0TS = joints[jt0].Item2;
                TrackingState joint1TS = joints[jt1].Item2;
                if (joint0TS == TrackingState.NotTracked || joint1TS == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (joint0TS == TrackingState.Tracked && joint1TS == TrackingState.Tracked)
                {
                    SkeletonVis.DrawBone(jointPt0, jointPt1, dc, bonePen);
                }
                else
                {
                    SkeletonVis.DrawBone(jointPt0, jointPt1, dc, SkeletonVis.inferredBonePen);
                }
            }
            // Draw joints
            foreach (Tuple<Point, TrackingState> joint in joints.Values)
            {
                Point coordinate = joint.Item1;
                TrackingState trackingState = joint.Item2;
                if (trackingState == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (trackingState == TrackingState.Tracked)
                {
                    SkeletonVis.DrawJoint(coordinate, dc, SkeletonVis.trackedJointBrush, SkeletonVis.jointThickness);
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    SkeletonVis.DrawJoint(coordinate, dc, SkeletonVis.inferredJointBrush, SkeletonVis.jointThickness);
                }
            }
        }

        private static void DrawJoint(Point joint, DrawingContext dc, Brush brush, double thickness)
        {
            dc.Dispatcher.Invoke((Action)(() =>
            {
                dc.DrawEllipse(brush, null, joint, thickness, thickness);
            }));
            //dc.DrawEllipse(brush, null, joint, thickness, thickness);
        }

        private static void DrawBone(Point from, Point to, DrawingContext dc, Pen pen)
        {
            dc.Dispatcher.Invoke((Action)(() =>
            {
                dc.DrawLine(pen, from, to);
            }));
            //dc.DrawLine(pen, from, to);
        }
    }
}
