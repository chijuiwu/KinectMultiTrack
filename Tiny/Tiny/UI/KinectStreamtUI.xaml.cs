using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KinectSerializer;
using Microsoft.Kinect;
using System.Diagnostics;
using System.Windows.Threading;
using SkeletonVis = Tiny.UI.SkeletonVisualizer;

namespace Tiny.UI
{
    public partial class KinectStreamUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        public DrawingImage BodyStreamImageSource { get; private set; }

        public KinectStreamUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.BodyStreamImageSource = new DrawingImage(this.bodyDrawingGroup);
        }

        public void Dispose()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.Close();
            }));
        }

        public void UpdateBodyFrame(SBodyFrame bodyFrame)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                //this.DisplayBodyFrames(bodyFrame);
            }));
        }

        private void DisplayBodyFrames(SBodyFrame bodyFrame)
        {
            int frameWidth = bodyFrame.DepthFrameWidth;
            int frameHeight = bodyFrame.DepthFrameWidth;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                this.DrawBackground(frameWidth, frameHeight, dc);
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (body.IsTracked)
                    {
                        Dictionary<JointType, SJoint> joints = body.Joints;
                        Dictionary<JointType, Tuple<Point, TrackingState>> jointPts = new Dictionary<JointType, Tuple<Point, TrackingState>>();
                        foreach (JointType jt in joints.Keys)
                        {
                            Point point = new Point(joints[jt].DepthSpacePoint.X, joints[jt].DepthSpacePoint.Y);
                            jointPts[jt] = Tuple.Create(point, joints[jt].TrackingState);
                        }
                        this.DrawBody(jointPts, dc);
                    }
                }
            }
            this.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
        }

        private static readonly Brush backgroundBrush = Brushes.Black;

        // Joints
        private static readonly double jointThickness = 3;
        private static readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private static readonly Brush inferredJointBrush = Brushes.Yellow;

        // Bones
        private static readonly Pen defaultTrackedBonePen = new Pen(Brushes.Blue, 6);
        private static readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private void DrawBackground(int frameWidth, int frameHeight, DrawingContext dc)
        {
            dc.DrawRectangle(KinectStreamUI.backgroundBrush, null, new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawClipRegion(int frameWidth, int frameHeight, DrawingGroup dg)
        {
            dg.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc)
        {
            this.DrawBody(joints, dc, KinectStreamUI.defaultTrackedBonePen);
        }

        private void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc, Pen bonePen)
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
                    this.DrawBone(jointPt0, jointPt1, dc, bonePen);
                }
                else
                {
                    this.DrawBone(jointPt0, jointPt1, dc, KinectStreamUI.inferredBonePen);
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
                    this.DrawJoint(coordinate, dc, KinectStreamUI.trackedJointBrush, KinectStreamUI.jointThickness);
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    this.DrawJoint(coordinate, dc, KinectStreamUI.inferredJointBrush, KinectStreamUI.jointThickness);
                }
            }
        }

        private void DrawJoint(Point joint, DrawingContext dc, Brush brush, double thickness)
        {
            dc.DrawEllipse(brush, null, joint, thickness, thickness);
        }

        private void DrawBone(Point from, Point to, DrawingContext dc, Pen pen)
        {
            dc.DrawLine(pen, from, to);
        }
    }
}
