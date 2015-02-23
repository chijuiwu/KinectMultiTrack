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
using SkeletonVis = Tiny.UI.SkeletonVisualizer;
using Tiny.WorldView;

namespace Tiny.UI
{
    public partial class TrackingUI : Window, INotifyPropertyChanged
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
        private string trackingStatusText;

        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> personColors;

        private KinectSensor kinectSensor;
        private CoordinateMapper coordinateMapper;

        public TrackingUI()
        {
            InitializeComponent();
            this.DataContext = this;

            this.referenceKinectIPs = new Dictionary<string, MenuItem>();
            this.currentReferenceKinectIP = "";
            this.currentViewMode = ViewMode.All;
            this.TrackingStatusText = Properties.Resources.TRACKING_CALIBRATION;
            
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // A person will have the same color
            this.personColors = new List<Pen>();
            this.personColors.Add(new Pen(Brushes.Red, 6));
            this.personColors.Add(new Pen(Brushes.Orange, 6));
            this.personColors.Add(new Pen(Brushes.Green, 6));
            this.personColors.Add(new Pen(Brushes.Blue, 6));
            this.personColors.Add(new Pen(Brushes.Indigo, 6));
            this.personColors.Add(new Pen(Brushes.Violet, 6));

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

        public void UpdateDisplay(Tracker.Result result)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
        }

        private Tracker.Result.KinectFOV GetReferenceKinectFOV(IEnumerable<Tracker.Result.KinectFOV> fovs)
        {
            Tracker.Result.KinectFOV referenceFOV = fovs.First();
            foreach (Tracker.Result.KinectFOV fov in fovs)
            {
                if (fov.ClientIP.ToString().Equals(this.currentReferenceKinectIP))
                {
                    referenceFOV = fov;
                }
            }
            return referenceFOV;
        }

        private void DisplayBodyFrames(Tracker.Result result)
        {
            if (!result.People.Any())
            {
                return;
            }
            Tracker.Result.KinectFOV referenceFOV = this.GetReferenceKinectFOV(result.FOVs);
            this.currentReferenceKinectIP = referenceFOV.ClientIP.ToString();

            int frameWidth = referenceFOV.Specification.DepthFrameWidth;
            int frameHeight = referenceFOV.Specification.DepthFrameHeight;

            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                this.DrawBackground(frameWidth, frameHeight, dc);
                int personIdx = 0;
                foreach (Tracker.Result.Person person in result.People)
                {
                    TSkeleton reference = person.FindSkeletonInFOV(referenceFOV);
                    // HACK
                    if (reference == null)
                    {
                        continue;
                    }
                    // All skeletons
                    HashSet<Dictionary<JointType, KinectJoint>> skeletonJointSet = new HashSet<Dictionary<JointType, KinectJoint>>();
                    foreach (Tracker.Result.SkeletonReplica match in person.Replicas)
                    {
                        skeletonJointSet.Add(TUtils.GetKinectJoints(match, reference));
                    }

                    Pen pen = this.personColors[personIdx++];
                    if (this.currentViewMode == ViewMode.Skeletons)
                    {
                        this.DrawSkeletons(skeletonJointSet, dc, pen);
                    }
                    else
                    {
                        // Average
                        Dictionary<JointType, KinectJoint> averageJoints = TUtils.GetAverages(skeletonJointSet);
                        if (this.currentViewMode == ViewMode.Average)
                        {
                            this.DrawAverageSkeletons(averageJoints, dc, TrackingUI.averageBonePen);
                        }
                        else if (this.currentViewMode == ViewMode.All)
                        {
                            this.DrawSkeletonsAndAverage(skeletonJointSet, averageJoints, dc, pen);
                        }
                    }
                }
            }
            this.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
        }

        private void DrawSkeletons(IEnumerable<Dictionary<JointType, KinectJoint>> skeletonJointsEnumeration, DrawingContext dc, Pen trackedBonePen)
        {
            foreach (Dictionary<JointType, KinectJoint> skeletonCoordinates in skeletonJointsEnumeration)
            {
                this.RenderJoints(skeletonCoordinates, dc, trackedBonePen);
            }
        }

        private void DrawAverageSkeletons(Dictionary<JointType, KinectJoint> averageCoordinates, DrawingContext dc, Pen trackedBonePen)
        {
            this.RenderJoints(averageCoordinates, dc, trackedBonePen);
        }
        private void DrawSkeletonsAndAverage(IEnumerable<Dictionary<JointType, KinectJoint>> skeletonJoints, Dictionary<JointType, KinectJoint> average, DrawingContext dc, Pen trackedBonePen)
        {
            this.DrawSkeletons(skeletonJoints, dc, trackedBonePen);
            this.DrawAverageSkeletons(average, dc, TrackingUI.averageBonePen);
        }

        private void RenderJoints(Dictionary<JointType, KinectJoint> coordinates, DrawingContext dc, Pen pen)
        {
            Dictionary<JointType, Tuple<Point, TrackingState>> drawableJoints = new Dictionary<JointType, Tuple<Point, TrackingState>>();
            foreach (JointType jt in coordinates.Keys)
            {
                CameraSpacePoint position = coordinates[jt].Coordinate;
                if (position.Z < 0)
                {
                    position.Z = 0.1f;
                }
                DepthSpacePoint joint2DPt = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                drawableJoints[jt] = Tuple.Create(new Point(joint2DPt.X, joint2DPt.Y), coordinates[jt].TrackingState);
            }
            this.DrawBody(drawableJoints, dc, pen);
        }

        private static readonly Brush backgroundBrush = Brushes.Black;

        // Joints
        private static readonly double jointThickness = 3;
        private static readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private static readonly Brush inferredJointBrush = Brushes.Yellow;

        // Bones
        private static readonly Pen defaultTrackedBonePen = new Pen(Brushes.Blue, 6);
        private static readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
        private static readonly Pen averageBonePen = new Pen(Brushes.White, 6);

        private void DrawBackground(int frameWidth, int frameHeight, DrawingContext dc)
        {
            dc.DrawRectangle(TrackingUI.backgroundBrush, null, new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawClipRegion(int frameWidth, int frameHeight, DrawingGroup dg)
        {
            dg.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, frameWidth, frameHeight));
        }

        private void DrawBody(Dictionary<JointType, Tuple<Point, TrackingState>> joints, DrawingContext dc)
        {
            this.DrawBody(joints, dc, TrackingUI.defaultTrackedBonePen);
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

        private void SetupBtn_Click(object sender, RoutedEventArgs e)
        {
            // Setup
        }
    }
}

