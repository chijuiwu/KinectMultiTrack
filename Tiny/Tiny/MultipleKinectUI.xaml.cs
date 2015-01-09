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
using System.Diagnostics;

namespace Tiny
{
    public partial class MultipleKinectUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> bodyColors;
        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public MultipleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            this.bodyColors = new List<Pen>();
            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));
        }
        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        internal void UpdateFrames(IEnumerable<SerializableBodyFrame> bodyFrames)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(bodyFrames);
            }));
        }

        private void DisplayBodyFrames(IEnumerable<SerializableBodyFrame> bodyFrames)
        {
            if (!bodyFrames.Any()) return;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SerializableBodyFrame firstFrame = bodyFrames.First();
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, firstFrame.DepthFrameWidth, firstFrame.DepthFrameHeight));
                int penIndex = 0;
                foreach (SerializableBodyFrame bodyFrame in bodyFrames)
                {
                    foreach (SerializableBody body in bodyFrame.Bodies)
                    {
                        if (body.IsTracked)
                        {
                            Pen drawPen = this.bodyColors[penIndex++];
                            Dictionary<JointType, SerializableJoint> joints = body.Joints;

                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                            foreach (JointType jointType in joints.Keys)
                            {
                                DepthSpacePoint depthSpacePoint = joints[jointType].DepthSpacePoint;
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }
                            this.DrawBody(joints, jointPoints, dc, drawPen, this.inferredBonePen, this.trackedJointBrush, this.inferredJointBrush);
                            if (penIndex == this.bodyColors.Count())
                            {
                                penIndex = 0;
                            }
                        }
                    }
                }
                this.bodyDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, firstFrame.DepthFrameWidth, firstFrame.DepthFrameHeight));
            }
        }

        private void DrawBody(Dictionary<JointType, SerializableJoint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen bonePen, Pen inferredBonePen, Brush jointBrush, Brush inferredJointBrush)
        {
            // Draw the bones
            foreach (var bone in BodyStructure.Bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, bonePen, inferredBonePen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;
                TrackingState trackingState = joints[jointType].TrackingState;
                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = jointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = inferredJointBrush;
                }
                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        private void DrawBone(Dictionary<JointType, SerializableJoint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen, Pen inferredDrawingPen)
        {
            SerializableJoint joint0 = joints[jointType0];
            SerializableJoint joint1 = joints[jointType1];

            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            Pen drawPen = inferredDrawingPen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }
    }
}
