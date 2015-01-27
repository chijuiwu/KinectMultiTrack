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
using System.Windows.Shapes;
using KinectSerializer;
using Microsoft.Kinect;
using System.Diagnostics;
using SkeletonVis = Tiny.UI.SkeletonVisualizer;

namespace Tiny.UI
{
    public partial class SingleKinectUI : Window
    {
        private DrawingGroup bodyDrawingGroup;
        public DrawingImage BodyStreamImageSource { get; private set; }

        public SingleKinectUI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.bodyDrawingGroup = new DrawingGroup();
            this.BodyStreamImageSource = new DrawingImage(this.bodyDrawingGroup);
        }

        public void Dispose()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.Close();
            }));
        }

        internal void UpdateBodyFrame(SBodyFrame bodyFrame)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.PerformUpdateBodyFrame(bodyFrame);
            }));
        }

        private void PerformUpdateBodyFrame(SBodyFrame bodyFrame)
        {
            int frameWidth = bodyFrame.DepthFrameWidth;
            int frameHeight = bodyFrame.DepthFrameWidth;

            using (DrawingContext dc = this.bodyDrawingGroup.Open())
            {
                SkeletonVis.DrawBackground(frameWidth, frameHeight, dc);
                foreach (SBody body in bodyFrame.Bodies)
                {
                    if (body.IsTracked)
                    {
                        Dictionary<JointType, SJoint> joints = body.Joints;
                        Dictionary<JointType, Tuple<Point, TrackingState>> jointPts = new Dictionary<JointType, Tuple<Point, TrackingState>>();
                        foreach (JointType jt in joints.Keys)
                        {
                            Point point = new Point(joints[jt].DepthSpacePoint.X, joints[jt].DepthSpacePoint.Y);
                            jointPts[jt] = Tuple.Create(point, joints[jt].TrackingState);
                        }
                        SkeletonVis.DrawBody(jointPts, dc);
                    }
                }
                SkeletonVis.DrawClipRegion(frameWidth, frameHeight, this.bodyDrawingGroup);
            }
        }
    }
}
