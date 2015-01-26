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
using System.Net;
using System.Diagnostics;
using SkeletonVis = Tiny.UI.SkeletonVisualizer;

namespace Tiny.UI
{
    public partial class MultipleKinectUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        private DrawingImage bodyImageSource;
        private List<Pen> kinectColors;

        public MultipleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // HACK: Max 6 Kinects
            // Frames from a kinect have the same color
            this.kinectColors = new List<Pen>();
            this.kinectColors.Add(new Pen(Brushes.Red, 6));
            this.kinectColors.Add(new Pen(Brushes.Orange, 6));
            this.kinectColors.Add(new Pen(Brushes.Green, 6));
            this.kinectColors.Add(new Pen(Brushes.Blue, 6));
            this.kinectColors.Add(new Pen(Brushes.Indigo, 6));
            this.kinectColors.Add(new Pen(Brushes.Violet, 6));
        }
        public ImageSource BodyStreamImageSource
        {
            get
            {
                return this.bodyImageSource;
            }
        }

        public void UpdateDisplay(Tracker.Result result)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.DisplayBodyFrames(result);
            }));
        }

        private void DisplayBodyFrames(Tracker.Result result)
        {
            if (!result.People.Any())
            {
                return;
            }

            KinectCamera.Dimension firstFOVDim = result.FOVs.First().Dimension;
            int frameWidth = firstFOVDim.DepthFrameWidth;
            int frameHeight = firstFOVDim.DepthFrameHeight;

            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SkeletonVis.DrawBackground(frameWidth, frameHeight, dc);

                Dictionary<Tracker.Result.KinectFOV, Pen> kinectFOVPens = new Dictionary<Tracker.Result.KinectFOV, Pen>();
                int penCount = 0;
                foreach (Tracker.Result.Person person in result.People)
                {
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        Tracker.Result.KinectFOV fov = match.FOV;
                        Pen kinectPen;
                        if (!kinectFOVPens.ContainsKey(fov))
                        {
                            kinectPen = this.kinectColors[penCount++];
                            kinectFOVPens[fov] = kinectPen;
                        }
                        else
                        {
                            kinectPen = kinectFOVPens[fov];
                        }

                        TrackingSkeleton.Position skeletonPos = match.Skeleton.CurrentPosition;
                        SBody skeletonKinectBody = skeletonPos.Kinect;
                        if (skeletonKinectBody.IsTracked)
                        {
                            Dictionary<JointType, SJoint> joints = skeletonKinectBody.Joints;
                            Dictionary<JointType, Tuple<TrackingState, Point>> jointPts = new Dictionary<JointType, Tuple<TrackingState, Point>>();
                            foreach (JointType jointType in joints.Keys)
                            {
                                SJoint joint = joints[jointType];
                                Point jointPt = new Point(joint.DepthSpacePoint.X, joint.DepthSpacePoint.Y);
                                jointPts[jointType] = Tuple.Create(joint.TrackingState, jointPt);
                            }
                            SkeletonVis.DrawBody(jointPts, dc, kinectPen);
                        }
                    }
                }

                SkeletonVis.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
            }
        }
    }
}
