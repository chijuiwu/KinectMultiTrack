using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Net;

namespace Tiny
{
    public partial class TrackingUI : Window, INotifyPropertyChanged
    {
        private string trackingStatusText;

        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> bodyColors;
        private readonly double jointThickness = 3;
        private readonly double clipBoundsThickness = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        private KinectSensor kinectSensor;
        private CoordinateMapper coordinateMapper;

        public TrackingUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.TrackingStatusText = Properties.Resources.TRACKING_CALIBRATION;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // A person will have the same color
            this.bodyColors = new List<Pen>();
            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public string TrackingStatusText
        {
            get
            {
                return this.trackingStatusText;
            }
            set
            {
                if (this.trackingStatusText != value)
                {
                    this.trackingStatusText = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("TrackingStatusText"));
                    }
                }
            }
        }

        public void setCalibrationComplete(bool completed)
        {
            this.TrackingStatusText = completed ? Properties.Resources.TRACKING_START
                                                            : Properties.Resources.TRACKING_CALIBRATION;
        }

        internal void UpdateDisplay(Tracker.Result result)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
        }

        private void DisplayBodyFrames(Tracker.Result result)
        {
            result.

            if (!bodyFrames.Any()) return;

            if (bodyFrames.Count() == 0) return;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                WBodyFrame firstWorldView = bodyFrames.First();
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, firstWorldView.DepthFrameWidth, firstWorldView.DepthFrameHeight));
                int penIndex = 0;
                foreach (WBodyFrame worldView in bodyFrames)
                {
                    foreach (WBody body in worldView.Boides)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        // convert coordinate wrt first world view
                        KinectBody bodyKinect = WBody.GetKinectBody(body, firstWorldView.InitialAngle, firstWorldView.InitialCentrePosition);
                        foreach (JointType jointType in bodyKinect.Joints.Keys)
                        {
                            CameraSpacePoint position = bodyKinect.Joints[jointType];
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
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
            foreach (var bone in BodyStructure.Bones)
            {
                this.DrawBone(jointPoints, bone.Item1, bone.Item2, drawingContext, bonePen, inferredBonePen);
            }

            // Draw the joints
            foreach (JointType jointType in jointPoints.Keys)
            {
                Brush drawBrush = jointBrush;
                drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], jointThickness, jointThickness);
            }
        }

        private void DrawBone(IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen, Pen inferredDrawingPen)
        {
            Pen drawPen = drawingPen;
            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        internal void UpdateCalibrationStatus(bool completed)
        {
            this.setCalibrationComplete(completed);
        }
    }
}

