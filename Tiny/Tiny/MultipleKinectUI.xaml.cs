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

namespace Tiny
{
    public partial class MultipleKinectUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> kinectColors;
        private readonly double jointThickness = 3;
        private readonly double clipBoundsThickness = 10;
        private readonly Brush backgroundBrush = Brushes.Black;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public MultipleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // Frames from a kinect have the same color (Hack: max 6 Kinects)
            this.kinectColors = new List<Pen>();
            this.kinectColors.Add(new Pen(Brushes.Red, 6));
            this.kinectColors.Add(new Pen(Brushes.Orange, 6));
            this.kinectColors.Add(new Pen(Brushes.Green, 6));
            this.kinectColors.Add(new Pen(Brushes.Blue, 6));
            this.kinectColors.Add(new Pen(Brushes.Indigo, 6));
            this.kinectColors.Add(new Pen(Brushes.Violet, 6));
        }
        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public void UpdateDisplay(IEnumerable<Tuple<IPEndPoint, SBodyFrame>> bodyFrames)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(bodyFrames);
            }));
        }

        private void DisplayBodyFrames(IEnumerable<Tuple<IPEndPoint, SBodyFrame>> bodyFrames)
        {
            if (!bodyFrames.Any()) return;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SBodyFrame frameZeroth = bodyFrames.First().Item2;
                int frameWidth = frameZeroth.DepthFrameWidth;
                int frameHeight = frameZeroth.DepthFrameHeight;
                // background
                dc.DrawRectangle(this.backgroundBrush, null, new Rect(0.0, 0.0, frameWidth, frameHeight));

                int kinectIdx = 0;
                foreach (Tuple<IPEndPoint, SBodyFrame> kinectBodyFrame in bodyFrames)
                {
                    Pen kinectPen = this.kinectColors[kinectIdx++];
                    SBodyFrame bodyFrame = kinectBodyFrame.Item2;
                    foreach (SBody body in bodyFrame.Bodies)
                    {
                        if (body.IsTracked)
                        {
                            Dictionary<JointType, SJoint> joints = body.Joints;
                            Dictionary<JointType, Tuple<SJoint, Point>> jointPts = new Dictionary<JointType, Tuple<SJoint, Point>>();
                            foreach (JointType jointType in joints.Keys)
                            {
                                SJoint joint = joints[jointType];
                                Point jointPt = new Point(joint.DepthSpacePoint.X, joint.DepthSpacePoint.Y);
                                jointPts[jointType] = Tuple.Create(joint, jointPt);
                            }
                            this.DrawBody(jointPts, dc, kinectPen);
                        }
                    }
                }
                this.bodyDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
            }
        }

        private void DrawBody(Dictionary<JointType, Tuple<SJoint, Point>> jointPts, DrawingContext dc, Pen trackedBonePen)
        {
            // Draw joints
            foreach (JointType jointType in jointPts.Keys)
            {
                TrackingState jointTS = jointPts[jointType].Item1.TrackingState;
                Point jointPt = jointPts[jointType].Item2;
                if (jointTS == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (jointTS == TrackingState.Tracked)
                {
                    this.DrawJoint(jointPt, dc, this.trackedJointBrush);
                }
                else
                {
                    this.DrawJoint(jointPt, dc, this.inferredJointBrush);
                }
            }

            // Draw bones
            foreach (var bone in BodyStructure.Bones)
            {
                JointType jointType0 = bone.Item1;
                JointType jointType1 = bone.Item2;
                TrackingState joint0TS = jointPts[jointType0].Item1.TrackingState;
                TrackingState joint1TS = jointPts[jointType1].Item1.TrackingState;
                Point jointPt0 = jointPts[jointType0].Item2;
                Point jointPt1 = jointPts[jointType1].Item2;
                if (joint0TS == TrackingState.NotTracked || joint1TS == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (joint0TS == TrackingState.Tracked && joint1TS == TrackingState.Tracked)
                {
                    this.DrawBone(jointPt0, jointPt1, dc, trackedBonePen);
                }
                else
                {
                    this.DrawBone(jointPt0, jointPt1, dc, this.inferredBonePen);
                }
            }
        }

        private void DrawJoint(Point joint, DrawingContext dc, Brush brush)
        {
            dc.DrawEllipse(brush, null, joint, this.jointThickness, this.jointThickness);
        }

        private void DrawBone(Point jointPoint0, Point jointPoint1, DrawingContext dc, Pen pen)
        {
            dc.DrawLine(pen, jointPoint0, jointPoint1);
        }
    }
}
