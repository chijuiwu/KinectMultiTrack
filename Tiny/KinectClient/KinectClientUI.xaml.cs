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
using System.Threading;
using System.Diagnostics;
using Microsoft.Kinect;
using KinectSerializer;

namespace KinectClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class KinectClientUI : Window, INotifyPropertyChanged
    {
        private const string kinectServerAddress = "138.251.213.82";
        private const string localhost = "127.0.0.1";
        private const int kinectServerPort = 12345;
        private KinectSocket kinectSocket;

        // Kinect position and angle
        private const double kinectAngle = 0;
        private const double kinectHeight = 100;

        private KinectSensor kinectSensor;
        private string kinectStatusText;
        private CoordinateMapper coordinateMapper;
        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private Pen bodyColor;
        private FrameDescription depthFrameDescription;
        private int displayWidth;
        private int displayHeight;

        private const double JointThickness = 3;
        private const double ClipBoundsThickness = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        public KinectClientUI()
        {
            InitializeComponent();

            this.kinectSocket = new KinectSocket(kinectServerAddress, kinectServerPort);

            this.DataContext = this;
            this.KinectStatusText = Properties.Resources.KinectUninitialized;
            this.InitializeKinect();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public string KinectStatusText
        {
            get
            {
                return this.kinectStatusText;
            }
            set
            {
                if (this.kinectStatusText != value)
                {
                    this.kinectStatusText = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("KinectStatusText"));
                    }
                }
            }
        }

        private void InitializeKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.IsAvailableChanged += this.kinectSensor_IsAvailableChanged;
            this.kinectSensor.Open();

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.displayWidth = this.depthFrameDescription.Width;
            this.displayHeight = this.depthFrameDescription.Height;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bodies = new Body[this.kinectSensor.BodyFrameSource.BodyCount];

            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            this.bodyColor = new Pen(Brushes.Blue, 6);
        }

        private void kinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.KinectStatusText = this.kinectSensor.IsAvailable ? Properties.Resources.KinectRunning
                                                            : Properties.Resources.KinectNotAvailable;
        }

        private void KinectClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.bodyFrameReader_FrameArrived;
            }
        }

        private void KinectClientWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.kinectSocket.CloseConnection();
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            // serializing KinectBodyFrame
            SBodyFrame serializableBodyFrame = null;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                    // serialize KinectBodyFrame
                    serializableBodyFrame = new SBodyFrame(bodyFrame.RelativeTime, this.depthFrameDescription);
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.bodyDrawingGroup.Open())
                {
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    foreach (Body body in this.bodies)
                    {
                        if (body.IsTracked)
                        {
                            // serialize KinectBody
                            SBody serializableBody = new SBody(true, body.TrackingId);

                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                            IReadOnlyDictionary<JointType, JointOrientation> jointOrientations = body.JointOrientations;

                            Dictionary<JointType, Point> drawableJointPts = new Dictionary<JointType, Point>();
                            foreach (JointType jointType in joints.Keys)
                            {
                                Joint joint = joints[jointType];
                                TrackingState trackingState = joint.TrackingState;
                                JointOrientation orientation = jointOrientations[jointType];
                                CameraSpacePoint csPt = joint.Position;
                                if (csPt.Z < 0)
                                {
                                    csPt.Z = 0.1f;
                                }
                                DepthSpacePoint dsPt = this.coordinateMapper.MapCameraPointToDepthSpace(csPt);
                                drawableJointPts[jointType] = new Point(dsPt.X, dsPt.Y);
                                // serialize KinectJoint
                                serializableBody.Joints[jointType] = new SJoint(trackingState, jointType, orientation, csPt, dsPt);
                            }
                            this.DrawBody(joints, drawableJointPts, dc, this.bodyColor, this.inferredBonePen, this.trackedJointBrush, this.inferredJointBrush);
                            // construct serializable KinectBodyFrame
                            serializableBodyFrame.addSerializableBody(serializableBody);
                        }
                    }
                    this.bodyDrawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
                // send
                this.kinectSocket.SendSerializedKinectBodyFrame(serializableBodyFrame);
            }
        }

        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen bonePen, Pen inferredBonePen, Brush jointBrush, Brush inferredJointBrush)
        {
            // Draw the bones
            foreach (var bone in SkeletonStructure.Bones)
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

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen, Pen inferredDrawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

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
