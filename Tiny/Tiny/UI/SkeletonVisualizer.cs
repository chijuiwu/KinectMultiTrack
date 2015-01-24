using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectSerializer;
using System.Windows;
using System.Windows.Media;
using SkeletonVis = Tiny.UI.SkeletonVisualizer;

namespace Tiny.UI
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

        public static void DrawBody(Dictionary<JointType, Tuple<TrackingState, Point>> jointPts, DrawingContext dc)
        {
            SkeletonVis.DrawBody(jointPts, dc, SkeletonVis.defaultTrackedBonePen);
        }

        public static void DrawBody(Dictionary<JointType, Tuple<TrackingState, Point>> jointPts, DrawingContext dc, Pen bonePen)
        {
            // Draw joints
            foreach (JointType jointType in jointPts.Keys)
            {
                TrackingState jointTS = jointPts[jointType].Item1;
                Point jointPt = jointPts[jointType].Item2;
                if (jointTS == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (jointTS == TrackingState.Tracked)
                {
                    SkeletonVis.DrawJoint(jointPt, dc, SkeletonVis.trackedJointBrush, SkeletonVis.jointThickness);
                }
                else if (jointTS == TrackingState.Inferred)
                {
                    SkeletonVis.DrawJoint(jointPt, dc, SkeletonVis.inferredJointBrush, SkeletonVis.jointThickness);
                }
            }

            // Draw bones
            foreach (var bone in BodyStructure.Bones)
            {
                JointType jointType0 = bone.Item1;
                JointType jointType1 = bone.Item2;
                TrackingState joint0TS = jointPts[jointType0].Item1;
                TrackingState joint1TS = jointPts[jointType1].Item1;
                Point jointPt0 = jointPts[jointType0].Item2;
                Point jointPt1 = jointPts[jointType1].Item2;
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
        }

        private static void DrawJoint(Point joint, DrawingContext dc, Brush brush, double thickness)
        {
            dc.DrawEllipse(brush, null, joint, thickness, thickness);
        }

        private static void DrawBone(Point from, Point to, DrawingContext dc, Pen pen)
        {
            dc.DrawLine(pen, from, to);
        }
    }
}
