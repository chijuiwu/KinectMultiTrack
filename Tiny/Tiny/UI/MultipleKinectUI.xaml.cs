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
            // Frames from a kinect have the same color (Hack: max 6 Kinects)
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
            IEnumerable<Tracker.Result.KinectFOV> fovs = result.FOVs;
            if (!fovs.Any())
            {
                return;
            }

            KinectCamera.Dimension firstFOVDim = fovs.First().Dimension;
            int frameWidth = firstFOVDim.DepthFrameWidth;
            int frameHeight = firstFOVDim.DepthFrameHeight;

            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SkeletonVis.DrawBackground(frameWidth, frameHeight, dc);

                int kinectIdx = 0;
                foreach (Tracker.Result.KinectFOV fov in fovs)
                {
                    if (!fov.Skeletons.Any())
                    {
                        continue;
                    }
                    Pen kinectPen = this.kinectColors[kinectIdx++];
                    foreach (TrackingSkeleton person in fov.Skeletons)
                    {
                        TrackingSkeleton.Position position = person.CurrentPosition;
                        SBody kinectBody = position.Kinect;
                        if (kinectBody.IsTracked)
                        {
                            Dictionary<JointType, SJoint> joints = kinectBody.Joints;
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
