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
        private string referenceKinectIP;
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
            this.referenceKinectIP = null;
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
                this.UpdateReferenceKinectMenu(result.FOVs);
                this.DisplayBodyFrames(result);
            }));
        }

        private void UpdateReferenceKinectMenu(IEnumerable<Tracker.Result.KinectFOV> fovs)
        {
            this.ReferenceKinectMenu.Items.Clear();
            bool containsReferenceFOV = false;
            foreach (Tracker.Result.KinectFOV fov in fovs)
            {
                MenuItem kinectIPItem = new MenuItem();
                kinectIPItem.Header = fov.ClientIP.ToString();
                kinectIPItem.Click += KinectIPItem_Click;
                this.ReferenceKinectMenu.Items.Add(kinectIPItem);
                
                if (kinectIPItem.Header.ToString().Equals(this.referenceKinectIP))
                {
                    containsReferenceFOV = true;
                }
            }
            if (!containsReferenceFOV)
            {
                this.ReferenceKinectBtn.Content = "Reference Kinect";
            }
        }

        private void KinectIPItem_Click(object sender, RoutedEventArgs e)
        {
            string referenceKinectIP = (sender as MenuItem).Header.ToString();
            this.referenceKinectIP = referenceKinectIP;
            this.ReferenceKinectBtn.Content = referenceKinectIP;
        }

        private Tracker.Result.KinectFOV GetReferenceKinectFOV(IEnumerable<Tracker.Result.KinectFOV> fovs)
        {
            Tracker.Result.KinectFOV referenceFOV = fovs.First();
            foreach (Tracker.Result.KinectFOV fov in fovs)
            {
                if (fov.ClientIP.ToString().Equals(this.referenceKinectIP))
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
            KinectCamera.Dimension referenceDim = referenceFOV.Dimension;
            int frameWidth = referenceDim.DepthFrameWidth;
            int frameHeight = referenceDim.DepthFrameHeight;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SkeletonVis.DrawBackground(frameWidth, frameHeight, dc);

                IEnumerable<Tracker.Result.Person> people = result.People;
                int personIdx = 0;
                foreach (Tracker.Result.Person person in people)
                {
                    TrackingSkeleton referenceSkeleton = null;
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        if (match.FOV.Equals(referenceFOV))
                        {
                            referenceSkeleton = match.Skeleton;
                            break;
                        }
                    }
                    double referenceAngle = referenceSkeleton.InitialAngle;
                    WCoordinate referencePosition = referenceSkeleton.InitialPosition;

                    Pen personPen = this.personColors[personIdx++];
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        WBody worldviewBody = match.Skeleton.CurrentPosition.Worldview;
                        KinectSkeleton kinectSkeleton = WBody.TransformBodyToKinectSkeleton(worldviewBody, referenceAngle, referencePosition);
                        Dictionary<JointType, Tuple<TrackingState, Point>> jointPts = new Dictionary<JointType, Tuple<TrackingState, Point>>();
                        foreach (JointType jointType in kinectSkeleton.Joints.Keys)
                        {
                            TrackingState jointTS = worldviewBody.Joints[jointType].TrackingState;
                            CameraSpacePoint position = kinectSkeleton.Joints[jointType];
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                            jointPts[jointType] = Tuple.Create(jointTS, new Point(depthSpacePoint.X, depthSpacePoint.Y));
                        }
                        
                        SkeletonVis.DrawBody(jointPts, dc, personPen);
                    }
                }
                
                SkeletonVis.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button senderBtn = sender as Button;
            senderBtn.ContextMenu.IsEnabled = true;
            senderBtn.ContextMenu.PlacementTarget = senderBtn;
            senderBtn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            senderBtn.ContextMenu.IsOpen = true;
        }
    }
}

