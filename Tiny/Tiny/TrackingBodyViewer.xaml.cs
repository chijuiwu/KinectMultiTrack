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
    public partial class TrackingBodyViewer : Window
    {
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> bodyColors;
        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public TrackingBodyViewer()
        {
            this.InitializeComponent();
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

        internal void UpdateTrackingDisplay(KinectServer server, IEnumerable<WorldView> worldViews)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(worldViews);
            }));
        }

        private void DisplayBodyFrames(IEnumerable<WorldView> worldViews)
        {
            if (worldViews.Count() == 0) return;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                WorldView firstWorldView = worldViews.First();
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, firstWorldView.DepthFrameWidth, firstWorldView.DepthFrameHeight));
                int penIndex = 0;
                foreach (WorldView worldView in worldViews)
                {
                    foreach (WorldBody body in worldView.WorldBoides)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];
                        Dictionary<JointType, WorldCoordinate> joints = body.Joints;
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                        foreach (JointType jointType in joints.Keys)
                        {
                            WorldCoordinate worldCoordinate = joints[jointType];
                            jointPoints[jointType] = new Point(worldCoordinate.X, worldCoordinate.Y);
                        }
                        this.DrawBody(jointPoints, dc, drawPen, this.inferredBonePen, this.trackedJointBrush, this.inferredJointBrush);
                        if (penIndex == this.bodyColors.Count())
                        {
                            penIndex = 0;
                        }
                    }
                }
                this.bodyDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, firstWorldView.DepthFrameWidth, firstWorldView.DepthFrameHeight));
            }
        }

        private void DrawBody(IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen bonePen, Pen inferredBonePen, Brush jointBrush, Brush inferredJointBrush)
        {
            // Draw the bones
            foreach (var bone in KinectBody.Bones)
            {
                this.DrawBone(jointPoints, bone.Item1, bone.Item2, drawingContext, bonePen, inferredBonePen);
            }

            // Draw the joints
            foreach (JointType jointType in jointPoints.Keys)
            {
                Brush drawBrush = jointBrush;
                drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
            }
        }

        private void DrawBone(IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen, Pen inferredDrawingPen)
        {
            Pen drawPen = drawingPen;
            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }
    }
}

