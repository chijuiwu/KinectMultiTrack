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
        private List<Pen> personColors;

        public MultipleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.bodyImageSource = new DrawingImage(this.bodyDrawingGroup);
            // HACK: Max 6 Kinects
            // Frames from a kinect have the same color
            this.personColors = new List<Pen>();
            this.personColors.Add(new Pen(Brushes.Red, 6));
            this.personColors.Add(new Pen(Brushes.Orange, 6));
            this.personColors.Add(new Pen(Brushes.Green, 6));
            this.personColors.Add(new Pen(Brushes.Blue, 6));
            this.personColors.Add(new Pen(Brushes.Indigo, 6));
            this.personColors.Add(new Pen(Brushes.Violet, 6));
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
                int personIdx = 0;
                foreach (Tracker.Result.Person person in result.People)
                {
                    Pen personPen = this.personColors[personIdx++];

                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        TrackingSkeleton.Position skeletonPos = match.Skeleton.CurrentPosition;
                        SBody skeletonKinectBody = skeletonPos.Kinect;
                        if (skeletonKinectBody.IsTracked)
                        {
                            Dictionary<JointType, SJoint> joints = skeletonKinectBody.Joints;
                            Dictionary<JointType, Tuple<Point, TrackingState>> jointPts = new Dictionary<JointType, Tuple<Point, TrackingState>>();
                            foreach (JointType jt in joints.Keys)
                            {
                                Point point = new Point(joints[jt].DepthSpacePoint.X, joints[jt].DepthSpacePoint.Y);
                                jointPts[jt] = Tuple.Create(point, joints[jt].TrackingState);
                            }
                            SkeletonVis.DrawBody(jointPts, dc, personPen);
                        }
                    }
                }
                SkeletonVis.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
            }
        }
    }
}
