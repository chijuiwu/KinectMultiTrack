using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
using SkeletonVis = KinectMultiTrack.UI.SkeletonVisualizer;
using KinectMultiTrack.WorldView;
using System.Globalization;

namespace KinectMultiTrack.UI
{
    public partial class TrackingUI : Window
    {
        private Dictionary<string, MenuItem> referenceKinectIPs;
        private string currentReferenceKinectIP;
        private enum ViewMode
        {
            Skeletons,
            Average,
            All
        };
        private ViewMode currentViewMode;

        private DrawingGroup trackingDrawingGroup;
        private DrawingImage trackingViewSource;
        private double displayWidth, displayHeight;

        private KinectSensor kinectSensor;
        private CoordinateMapper coordinateMapper;

        // Background
        private static readonly Brush backgroundBrush = Brushes.Black;

        // Joints
        private static readonly double jointThickness = 3;
        private static readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private static readonly Brush inferredJointBrush = Brushes.Yellow;

        // Bones
        private static readonly Pen defaultTrackedBonePen = new Pen(Brushes.Blue, 6);
        private static readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private static readonly Pen averageBonePen = new Pen(Brushes.White, 6);

        public event TrackingSetupHandler OnTrackerSetup;
        public delegate void TrackingSetupHandler(TrackerSetup setup);

        public event TrackingUIHandler OnStartStop;
        public delegate void TrackingUIHandler(bool start);

        public static readonly string UNINITIALIZED = "Uninitialized";
        public static readonly string INITIALIZED = "Initialized";
        public static readonly string WAITING_KINECT = "Waiting for Kinects";
        public static readonly string CALIBRATING = "Calibrating";

        public TrackingUI(Server server)
        {
            InitializeComponent();
            this.DataContext = this;

            this.referenceKinectIPs = new Dictionary<string, MenuItem>();
            this.currentReferenceKinectIP = "";
            this.currentViewMode = ViewMode.All;
            
            this.trackingDrawingGroup = new DrawingGroup();
            this.trackingViewSource = new DrawingImage(this.trackingDrawingGroup);

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            this.Closing += this.TrackingUI_Closing;

            server.TrackerResultUpdate += this.server_TrackingUIUpdate;
        }

        private void server_TrackingUIUpdate(TrackerResult result)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
        }

        public ImageSource TrackingViewSource
        {
            get
            {
                return this.trackingViewSource;
            }
        }

        private void TrackingUI_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowProgressText(TrackingUI.UNINITIALIZED);
        }

        private void TrackingUI_Closing(object sender, CancelEventArgs e)
        {
            this.OnStartStop(false);
        }

        private void SetupBtn_Click(object sender, RoutedEventArgs e)
        {
            SetupDialog setup = new SetupDialog();
            setup.Owner = this;
            setup.ShowDialog();
            if (setup.DialogResult.HasValue && setup.DialogResult.Value)
            {
                // Kinect
                int kinectsCount = Int32.Parse(setup.Kinect_Count.Text);

                // User
                bool log = Convert.ToBoolean(setup.User_Study_OnOff.IsChecked);
                int studyId = Convert.ToInt32(setup.User_Study_Id.Text);
                int scenario = Logger.NA;
                if (Convert.ToBoolean(setup.User_Scenario_All.IsChecked))
                {
                    scenario = Logger.ALL;
                }
                else if (Convert.ToBoolean(setup.User_Scenario_Stationary.IsChecked))
                {
                    scenario = Logger.STATIONARY;
                }
                else if (Convert.ToBoolean(setup.User_Scenario_Walk_Wei.IsChecked))
                {
                    scenario = Logger.WALK;
                }

                this.OnTrackerSetup(new TrackerSetup(kinectsCount, log, studyId, scenario));
                this.ShowProgressText(TrackingUI.INITIALIZED);
            }
            else
            {
                MessageBox.Show("CANCEL");
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            this.OnStartStop(true);
            this.ShowProgressText(TrackingUI.WAITING_KINECT);
        }

        public void OnCalibratedStarted()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ShowProgressText(TrackingUI.CALIBRATING);
            }));
        }

        private void ShowProgressText(string text)
        {
            using (DrawingContext dc = this.trackingDrawingGroup.Open())
            {
                dc.DrawText(new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.White), new Point(0, 0));
            }
        }

        public void ProcessTrackerResult(TrackerResult result)
        {
            //    this.trackingDrawingGroup.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { DisplayBodyFrames(result); }));
            
        }

        private TrackerResult.KinectFOV GetReferenceKinectFOV(IEnumerable<TrackerResult.KinectFOV> fovs)
        {
            TrackerResult.KinectFOV referenceFOV = fovs.First();
            foreach (TrackerResult.KinectFOV fov in fovs)
            {
                if (fov.ClientIP.ToString().Equals(this.currentReferenceKinectIP))
                {
                    referenceFOV = fov;
                }
            }
            return referenceFOV;
        }

        private void DisplayBodyFrames(TrackerResult result)
        {
            if (result.Equals(TrackerResult.Empty))
            {
                return;
            }

            this.displayWidth = this.TrackingUIViewBox.ActualWidth;
            this.displayHeight = this.TrackingUIViewBox.ActualHeight;
            using (DrawingContext dc = this.trackingDrawingGroup.Open())
            {
                this.DrawBackground(TrackingUI.backgroundBrush, displayWidth, displayHeight, dc);
            }

            TrackerResult.KinectFOV referenceFOV = this.GetReferenceKinectFOV(result.FOVs);
            this.currentReferenceKinectIP = referenceFOV.ClientIP.ToString();

            int frameWidth = referenceFOV.Specification.DepthFrameWidth;
            int frameHeight = referenceFOV.Specification.DepthFrameHeight;

            using (DrawingContext dc = this.trackingDrawingGroup.Open())
            {
                this.DrawBackground(TrackingUI.backgroundBrush, displayWidth, displayHeight, dc);
                int personIdx = 0;
                foreach (TrackerResult.Person person in result.People)
                {
                    MovingSkeleton referenceSkeleton = person.FindSkeletonInFOV(referenceFOV);
                    // HACK
                    if (referenceSkeleton == null)
                    {
                        continue;
                    }
                    double referenceAngle = referenceSkeleton.InitialAngle;
                    WCoordinate referencePosition = referenceSkeleton.InitialCenterPosition;
                    // All skeletons
                    List<KinectBody> bodies = new List<KinectBody>();
                    foreach (TrackerResult.PotentialSkeleton pSkeleton in person.Skeletons)
                    {
                        bodies.Add(WBody.TransformWorldToKinectBody(pSkeleton.Skeleton.CurrentPosition.Worldview, referenceAngle, referencePosition));
                    }

                    Pen personPen = Common.PersonColors[personIdx++];
                    if (this.currentViewMode == ViewMode.Skeletons || this.currentViewMode == ViewMode.All)
                    {
                        this.DrawSkeletons(bodies, dc, personPen);
                    }
                    if (this.currentViewMode == ViewMode.Average || this.currentViewMode == ViewMode.All)
                    {
                        KinectBody averageBody = KinectBody.GetAverageBody(bodies);
                        this.DrawSkeletons(new List<KinectBody>() { averageBody }, dc, TrackingUI.averageBonePen);
                    }
                }
            }
            this.DrawClipRegion(frameWidth, frameHeight, this.trackingDrawingGroup);
        }

        private void DrawSkeletons(IEnumerable<KinectBody> bodies, DrawingContext dc, Pen trackedBonePen)
        {
            foreach (KinectBody body in bodies)
            {
                Dictionary<JointType, Tuple<Point, TrackingState>> drawableJoints = new Dictionary<JointType, Tuple<Point, TrackingState>>();
                foreach (JointType jt in body.Joints.Keys)
                {
                    CameraSpacePoint position = body.Joints[jt].Position;
                    if (position.Z < 0)
                    {
                        position.Z = 0.1f;
                    }
                    DepthSpacePoint joint2DPt = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                    drawableJoints[jt] = Tuple.Create(new Point(joint2DPt.X, joint2DPt.Y), body.Joints[jt].TrackingState);
                }
                this.DrawBody(drawableJoints, dc, trackedBonePen);
            }
        }

        private void DrawBackground(Brush color, double width, double height, DrawingContext dc)
        {
            dc.DrawRectangle(color, null, new Rect(0.0, 0.0, width, height));
        }

        private void DrawClipRegion(int frameWidth, int frameHeight, DrawingGroup dg)
        {
            dg.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc, Pen bonePen)
        {
            // Draw bones
            foreach (var bone in SkeletonStructure.Bones)
            {
                JointType jt0 = bone.Item1;
                JointType jt1 = bone.Item2;
                if (!joints.ContainsKey(jt0) || !joints.ContainsKey(jt1))
                {
                    continue;
                }
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
                    this.DrawBone(jointPt0, jointPt1, dc, TrackingUI.inferredBonePen);
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
                    this.DrawJoint(coordinate, dc, TrackingUI.trackedJointBrush, TrackingUI.jointThickness);
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    this.DrawJoint(coordinate, dc, TrackingUI.inferredJointBrush, TrackingUI.jointThickness);
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

        private void ReferenceKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            Button referenceKinectBtn = sender as Button;
            referenceKinectBtn.ContextMenu.IsEnabled = true;
            referenceKinectBtn.ContextMenu.PlacementTarget = referenceKinectBtn;
            referenceKinectBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            referenceKinectBtn.ContextMenu.IsOpen = true;
        }

        internal void AddKinectCamera(IPEndPoint clientIP)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                MenuItem kinectIPItem = new MenuItem();
                kinectIPItem.Header = clientIP.ToString();
                kinectIPItem.Click += ReferenceKinectItem_Click;
                this.ReferenceKinectMenu.Items.Add(kinectIPItem);
                this.referenceKinectIPs[clientIP.ToString()] = kinectIPItem;
            }));
        }

        internal void RemoveKinectCamera(IPEndPoint clientIP)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ReferenceKinectMenu.Items.Remove(this.referenceKinectIPs[clientIP.ToString()]);
                this.referenceKinectIPs.Remove(clientIP.ToString());
                if (this.currentReferenceKinectIP.Equals(clientIP))
                {
                    this.ReferenceKinectBtn.Content = "Reference Kinect";
                    this.currentReferenceKinectIP = "";
                }
            }));
        }

        private void ReferenceKinectItem_Click(object sender, RoutedEventArgs e)
        {
            string referenceKinectIP = (sender as MenuItem).Header.ToString();
            this.currentReferenceKinectIP = referenceKinectIP;
            this.ReferenceKinectBtn.Content = referenceKinectIP;
        }

        private void ViewModeBtn_Click(object sender, RoutedEventArgs e)
        {
            Button viewModeBtn = sender as Button;
            viewModeBtn.ContextMenu.IsEnabled = true;
            viewModeBtn.ContextMenu.PlacementTarget = viewModeBtn;
            viewModeBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            viewModeBtn.ContextMenu.IsOpen = true;
        }

        private void ViewMode_Skeletons_Click(object sender, RoutedEventArgs e)
        {
            this.currentViewMode = ViewMode.Skeletons;
            this.ViewModeBtn.Content = ViewMode.Skeletons;
        }

        private void ViewMode_Average_Click(object sender, RoutedEventArgs e)
        {
            this.currentViewMode = ViewMode.Average;
            this.ViewModeBtn.Content = ViewMode.Average;
        }

        private void ViewMode_All_Click(object sender, RoutedEventArgs e)
        {
            this.currentViewMode = ViewMode.All;
            this.ViewModeBtn.Content = ViewMode.All;
        }
    }
}

