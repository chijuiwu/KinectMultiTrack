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
            All,
            Average,
            Skeletons,
        };
        private ViewMode currentViewMode;

        private DrawingGroup trackingUIDrawingGroup;
        private DrawingImage trackingUIViewSource;
        private DrawingGroup multipleUIDrawingGroup;
        private DrawingImage multipleUIViewSource;

        private KinectSensor kinectSensor;
        private CoordinateMapper coordinateMapper;

        public event TrackingUISetupHandler OnSetup;
        public delegate void TrackingUISetupHandler(int kinectCount, bool studyOn, int userStudyId, int userSenario, int kinectConfiguration);
        public event TrackingUIHandler OnStartStop;
        public delegate void TrackingUIHandler(bool start);

        private bool studyOn;

        private static readonly string UNINITIALIZED = "Uninitialized";
        private static readonly string INITIALIZED = "Initialized";
        private static readonly string KINECT_FORMAT = "Waiting for Kinects...{0}";
        private static readonly string CALIBRATION_FORMAT = "Calibrating...\n{0} frames remaining";
        private static readonly string RE_CALIBRATION_FORMAT = "Confused!!! Recalibrating...\n{0}";

        public TrackingUI()
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.referenceKinectIPs = new Dictionary<string, MenuItem>();
            this.currentReferenceKinectIP = "";
            this.currentViewMode = ViewMode.All;
            
            this.trackingUIDrawingGroup = new DrawingGroup();
            this.trackingUIViewSource = new DrawingImage(this.trackingUIDrawingGroup);
            this.multipleUIDrawingGroup = new DrawingGroup();
            this.multipleUIViewSource = new DrawingImage(this.multipleUIDrawingGroup);

            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            this.studyOn = false;

            this.Closing += this.TrackingUI_Closing;
        }


        public void Server_AddKinectCamera(IPEndPoint clientIP)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                MenuItem kinectIPItem = new MenuItem();
                kinectIPItem.Header = clientIP.ToString();
                kinectIPItem.Click += KinectFOVItem_Click;
                this.KinectFOVMenu.Items.Add(kinectIPItem);
                this.referenceKinectIPs[clientIP.ToString()] = kinectIPItem;
            }));
        }

        public void Server_RemoveKinectCamera(IPEndPoint clientIP)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.KinectFOVMenu.Items.Remove(this.referenceKinectIPs[clientIP.ToString()]);
                this.referenceKinectIPs.Remove(clientIP.ToString());
                if (this.currentReferenceKinectIP.Equals(clientIP))
                {
                    this.KinectFOVBtn.Content = "Reference Kinect";
                    this.currentReferenceKinectIP = "";
                }
            }));
        }

        public void Tracker_OnWaitingKinects(int kinects)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ShowProgressText(String.Format(TrackingUI.KINECT_FORMAT, kinects));
            }));
        }

        public void Tracker_OnCalibration(uint framesRemaining)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ShowProgressText(String.Format(TrackingUI.CALIBRATION_FORMAT, framesRemaining));
            }));
        }

        public void Tracker_OnReCalibration(string msg)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.ShowProgressText(String.Format(TrackingUI.RE_CALIBRATION_FORMAT, msg));
            }));
        }

        public void Tracker_OnResult(TrackerResult result)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
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
                this.studyOn = setup.User_Study_On;
                this.StartBtn.IsEnabled = true;
                this.OnSetup(setup.Kinect_Count, setup.User_Study_On, setup.User_Study_Id, setup.User_Scenario, setup.Kinect_Configuration);
                this.ShowProgressText(TrackingUI.INITIALIZED);
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SetupBtn.IsEnabled = false;
            this.StopBtn.IsEnabled = true;
            this.RecalibrateBtn.IsEnabled = true;
            this.KinectFOVBtn.IsEnabled = true;
            this.ViewModeBtn.IsEnabled = true;
            this.OnStartStop(true);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SetupBtn.IsEnabled = true;
            this.StartBtn.IsEnabled = true;
            this.StopBtn.IsEnabled = false;
            this.RecalibrateBtn.IsEnabled = false;
            this.KinectFOVBtn.IsEnabled = false;
            this.ViewModeBtn.IsEnabled = false;
            this.OnStartStop(false);
        }

        private void RecalibrateBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowProgressText(string text)
        {
            using (DrawingContext dc = this.trackingUIDrawingGroup.Open())
            {
                FormattedText txt = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.White);
                txt.TextAlignment = TextAlignment.Center;
                dc.DrawText(txt, new Point(0, 0));
            }
        }

        #region UI viewing bindings Important!!!
        public ImageSource TrackingUI_Viewsource
        {
            get
            {
                return this.trackingUIViewSource;
            }
        }

        public ImageSource MultipleUI_Viewsource
        {
            get
            {
                return this.multipleUIViewSource;
            }
        }
        #endregion

        private TrackerResult.KinectFOV UpdateReferenceKinectFOV(IEnumerable<TrackerResult.KinectFOV> fovs)
        {
            TrackerResult.KinectFOV referenceFOV = fovs.First();
            foreach (TrackerResult.KinectFOV fov in fovs)
            {
                if (fov.ClientIP.ToString().Equals(this.currentReferenceKinectIP))
                {
                    referenceFOV = fov;
                    break;
                }
            }
            this.currentReferenceKinectIP = referenceFOV.ClientIP.ToString();
            return referenceFOV;
        }

        private void DisplayBodyFrames(TrackerResult result)
        {
            if (result.Equals(TrackerResult.Empty))
            {
                return;
            }
            this.RefreshTrackingUI(result);
            if (this.studyOn)
            {
            }
            else
            {
                this.RefreshMultipleUI(result);
            }
        }

        private void RefreshTrackingUI(TrackerResult result)
        {
            double trackingUIWidth = this.TrackingUI_Viewbox.ActualWidth;
            double trackingUIHeight = this.TrackingUI_Viewbox.ActualHeight;

            int frameWidth = result.DepthFrameWidth;
            int frameHeight = result.DepthFrameHeight;

            TrackerResult.KinectFOV referenceFOV = this.UpdateReferenceKinectFOV(result.FOVs);

            using (DrawingContext dc = this.trackingUIDrawingGroup.Open())
            {
                this.DrawBackground(Colors.BACKGROUND_TRACKING, trackingUIWidth, trackingUIHeight, dc);

                int personIdx = 0;
                foreach (TrackerResult.Person person in result.People)
                {
                    TrackingSkeleton refSkeleton = person.GetSkeletonInFOV(referenceFOV);

                    List<KinectBody> bodies = new List<KinectBody>();
                    foreach (TrackerResult.PotentialSkeleton pSkeleton in person.PotentialSkeletons)
                    {
                        this.DrawClippedEdges(pSkeleton.Skeleton.CurrentPosition.Kinect, frameWidth, frameHeight, dc);
                        bodies.Add(WBody.TransformWorldToKinectBody(pSkeleton.Skeleton.CurrentPosition.Worldview, refSkeleton.InitialAngle, refSkeleton.InitialCenterPosition));
                    }

                    Pen skeletonColor = Colors.SKELETON[personIdx++];
                    if (this.currentViewMode == ViewMode.Skeletons || this.currentViewMode == ViewMode.All)
                    {
                        this.DrawSkeletons(bodies, dc, skeletonColor);
                    }
                    if (this.currentViewMode == ViewMode.Average || this.currentViewMode == ViewMode.All)
                    {
                        this.DrawSkeletons(new List<KinectBody>() { KinectBody.GetAverageBody(bodies) }, dc, Colors.AVG_BONE);
                    }
                }
                this.DrawClipRegion(frameWidth, frameHeight, this.trackingUIDrawingGroup);
            }
        }

        private void RefreshMultipleUI(TrackerResult result)
        {
            double multipleUIWidth = this.MultipleUI_Viewbox.ActualWidth;
            double multipleUIHeight = this.MultipleUI_Viewbox.ActualHeight;

            int frameWidth = result.DepthFrameWidth;
            int frameHeight = result.DepthFrameHeight;

            using (DrawingContext dc = this.multipleUIDrawingGroup.Open())
            {
                this.DrawBackground(Colors.BACKGROUND_MULTIPLE, multipleUIWidth, multipleUIHeight, dc);

                int personIdx = 0;
                foreach (TrackerResult.Person person in result.People)
                {
                    Pen skeletonColor = Colors.SKELETON[personIdx++];
                    foreach (TrackerResult.PotentialSkeleton pSkeleton in person.PotentialSkeletons)
                    {
                        SBody body = pSkeleton.Skeleton.CurrentPosition.Kinect;
                        Dictionary<JointType, DrawableJoint> jointPts = new Dictionary<JointType, DrawableJoint>();
                        foreach (JointType jt in body.Joints.Keys)
                        {
                            Point point = new Point(body.Joints[jt].DepthSpacePoint.X, body.Joints[jt].DepthSpacePoint.Y);
                            jointPts[jt] = new DrawableJoint(point, body.Joints[jt].TrackingState);
                        }
                        this.DrawBody(jointPts, dc, skeletonColor);
                    }
                }
                this.DrawClipRegion(frameWidth, frameHeight, this.multipleUIDrawingGroup);
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

        private void DrawClippedEdges(SBody body, int frameWidth, int frameHeight, DrawingContext dc)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                dc.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, frameHeight - Colors.CLIP_BOUNDS_THICKNESS, frameWidth, Colors.CLIP_BOUNDS_THICKNESS));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                dc.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, frameWidth, Colors.CLIP_BOUNDS_THICKNESS));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                dc.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, Colors.CLIP_BOUNDS_THICKNESS, frameHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                dc.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(frameWidth - Colors.CLIP_BOUNDS_THICKNESS, 0, Colors.CLIP_BOUNDS_THICKNESS, frameHeight));
            }
        }

        private void DrawSkeletons(IEnumerable<KinectBody> bodies, DrawingContext dc, Pen skeletonColor)
        {
            foreach (KinectBody body in bodies)
            {
                Dictionary<JointType, DrawableJoint> jointPts = new Dictionary<JointType, DrawableJoint>();
                foreach (JointType jt in body.Joints.Keys)
                {
                    CameraSpacePoint position = body.Joints[jt].Position;
                    if (position.Z < 0)
                    {
                        position.Z = 0.1f;
                    }
                    DepthSpacePoint jointPt2D = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                    jointPts[jt] = new DrawableJoint(new Point(jointPt2D.X, jointPt2D.Y), body.Joints[jt].TrackingState);
                }
                this.DrawBody(jointPts, dc, skeletonColor);
            }
        }

        private void DrawBody(Dictionary<JointType, DrawableJoint> joints, DrawingContext dc, Pen skeletonColor)
        {
            // Draw bones
            foreach (var bone in SkeletonStructure.Bones)
            {
                JointType jt1st = bone.Item1;
                JointType jt2nd = bone.Item2;
                if (!joints.ContainsKey(jt1st) || !joints.ContainsKey(jt2nd))
                {
                    continue;
                }
                if (joints[jt1st].TrackingState == TrackingState.NotTracked || joints[jt2nd].TrackingState == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (joints[jt1st].TrackingState == TrackingState.Tracked && joints[jt2nd].TrackingState == TrackingState.Tracked)
                {
                    this.DrawBone(joints[jt1st].Point, joints[jt2nd].Point, dc, skeletonColor);
                }
                else
                {
                    this.DrawBone(joints[jt1st].Point, joints[jt2nd].Point, dc, Colors.INFERRED_BONE);
                }
            }
            // Draw joints
            foreach (DrawableJoint joint in joints.Values)
            {
                if (joint.TrackingState == TrackingState.NotTracked)
                {
                    continue;
                }
                else if (joint.TrackingState == TrackingState.Tracked)
                {
                    this.DrawJoint(joint.Point, dc, Colors.TRACKED_JOINT, Colors.JOINT_THICKNESS);
                }
                else if (joint.TrackingState == TrackingState.Inferred)
                {
                    this.DrawJoint(joint.Point, dc, Colors.INFERRED_JOINT, Colors.JOINT_THICKNESS);
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

        private void KinectFOVBtn_Click(object sender, RoutedEventArgs e)
        {
            Button referenceKinectBtn = sender as Button;
            referenceKinectBtn.ContextMenu.IsEnabled = true;
            referenceKinectBtn.ContextMenu.PlacementTarget = referenceKinectBtn;
            referenceKinectBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            referenceKinectBtn.ContextMenu.IsOpen = true;
        }


        private void KinectFOVItem_Click(object sender, RoutedEventArgs e)
        {
            string referenceKinectIP = (sender as MenuItem).Header.ToString();
            this.currentReferenceKinectIP = referenceKinectIP;
            this.KinectFOVBtn.Content = referenceKinectIP;
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

