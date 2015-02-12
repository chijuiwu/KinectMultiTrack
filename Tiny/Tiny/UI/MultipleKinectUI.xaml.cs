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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KinectSerializer;
using Microsoft.Kinect;
using System.Net;
using System.Diagnostics;
using System.Threading;
using SkeletonVis = Tiny.UI.SkeletonVisualizer;

namespace Tiny.UI
{
    public partial class MultipleKinectUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> personColors;

        public MultipleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // HACK: Max 6 people
            this.personColors = new List<Pen>();
            this.personColors.Add(new Pen(Brushes.Red, 6));
            this.personColors.Add(new Pen(Brushes.Orange, 6));
            this.personColors.Add(new Pen(Brushes.Green, 6));
            this.personColors.Add(new Pen(Brushes.Blue, 6));
            this.personColors.Add(new Pen(Brushes.Indigo, 6));
            this.personColors.Add(new Pen(Brushes.Violet, 6));
        }
        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public void UpdateDisplay(Tracker.Result result)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
        }

        private void DisplayBodyFrames(Tracker.Result result)
        {
            if (!result.People.Any())
            {
                return;
            }
            KinectCamera.Dimension firstFOVDim = result.FOVs.First().Dimension;
            int frameWidth = firstFOVDim.DepthFrameWidth;
            int frameHeight = firstFOVDim.DepthFrameHeight;

            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                this.DrawBackground(frameWidth, frameHeight, dc);
                int personIdx = 0;
                foreach (Tracker.Result.Person person in result.People)
                {
                    Pen pen = this.personColors[personIdx++];
                    foreach (Tracker.Result.Replica match in person.Replicas)
                    {
                        SBody body = match.Skeleton.CurrentPosition.Kinect;
                        Dictionary<JointType, SJoint> joints = body.Joints;
                        Dictionary<JointType, Tuple<Point, TrackingState>> jointPts = new Dictionary<JointType, Tuple<Point, TrackingState>>();
                        foreach (JointType jt in joints.Keys)
                        {
                            Point point = new Point(joints[jt].DepthSpacePoint.X, joints[jt].DepthSpacePoint.Y);
                            jointPts[jt] = Tuple.Create(point, joints[jt].TrackingState);
                        }
                        this.DrawBody(jointPts, dc, pen);
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
            dc.DrawRectangle(MultipleKinectUI.backgroundBrush, null, new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawClipRegion(int frameWidth, int frameHeight, DrawingGroup dg)
        {
            dg.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc)
        {
            this.DrawBody(joints, dc, MultipleKinectUI.defaultTrackedBonePen);
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
                    this.DrawBone(jointPt0, jointPt1, dc, MultipleKinectUI.inferredBonePen);
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
                    this.DrawJoint(coordinate, dc, MultipleKinectUI.trackedJointBrush, MultipleKinectUI.jointThickness);
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    this.DrawJoint(coordinate, dc, MultipleKinectUI.inferredJointBrush, MultipleKinectUI.jointThickness);
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
