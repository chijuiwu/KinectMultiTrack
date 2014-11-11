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

namespace Tiny
{
    /// <summary>
    /// Interaction logic for KinectViewer.xaml
    /// </summary>
    public partial class KinectBodyViewer : Window
    {
        
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private Pen bodyColor;
        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public KinectBodyViewer()
        {
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            this.bodyColor = new Pen(Brushes.Blue, 6);

            this.InitializeComponent();
        }

        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public void DisplayBodyFrame(SerializableBodyFrame bodyFrame)
        {
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight));
                foreach (SerializableBody body in bodyFrame.Bodies)
                {
                    if (body.IsTracked)
                    {
                        Dictionary<JointType, SerializableJoint> joints = body.Joints;

                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                        foreach (JointType jointType in joints.Keys)
                        {
                            CameraSpacePoint position = joints[jointType].CameraSpacePoint;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            DepthSpacePoint depthSpacePoint = joints[jointType].DepthSpacePoint;
                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                        }
                        this.DrawBody(joints, jointPoints, dc, this.bodyColor, this.inferredBonePen, this.trackedJointBrush, this.inferredJointBrush);
                    }
                }
                this.bodyDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight));
            }
        }

        private void DrawBody(Dictionary<JointType, SerializableJoint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen bonePen, Pen inferredBonePen, Brush jointBrush, Brush inferredJointBrush)
        {
            // Draw the bones
            foreach (var bone in KinectBody.Bones)
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
