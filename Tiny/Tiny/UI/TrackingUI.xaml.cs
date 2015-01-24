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
        private string trackingStatusText;

        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> bodyColors;

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
            IEnumerable<Tracker.Result.KinectFOV> fovs = result.FOVs;
            if (!fovs.Any())
            {
                return;
            }
            
            // HACK - draw people wrt first FOV
            Tracker.Result.KinectFOV firstFOV = fovs.First();
            KinectCamera.Dimension firstFOVDim = firstFOV.Dimension;
            int frameWidth = firstFOVDim.DepthFrameWidth;
            int frameHeight = firstFOVDim.DepthFrameHeight;
            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SkeletonVis.DrawBackground(frameWidth, frameHeight, dc);

                IEnumerable<Tracker.Result.Person> people = result.People;
                int personIdx = 0;
                foreach (Tracker.Result.Person person in people)
                {
                    // HACK - find skeleton in first FOV
                    TrackingSkeleton referenceSkeleton = null;
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        if (match.FOV.Equals(firstFOV))
                        {
                            referenceSkeleton = match.Skeleton;
                            break;
                        }
                    }
                    double referenceAngle = referenceSkeleton.InitialAngle;
                    WCoordinate referencePosition = referenceSkeleton.InitialPosition;

                    Pen personPen = this.bodyColors[personIdx++];
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

        internal void UpdateCalibrationStatus(bool completed)
        {
            this.setCalibrationComplete(completed);
        }
    }
}

