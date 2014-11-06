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

namespace Tiny
{
    public partial class KinectServerWindow : Window
    {
        private KinectBodyViewer kinectViewer1;
        private KinectBodyViewer kinectViewer2;

        public KinectServerWindow()
        {
            this.kinectViewer1 = new KinectBodyViewer();
            this.kinectViewer1.Show();
            this.kinectViewer2 = new KinectBodyViewer();
            this.kinectViewer2.Show();

            this.InitializeComponent();
        }

        public void DisplayKinectBodyFrame(SerializableBodyFrame bodyFrame, int clientId)
        {
            Console.WriteLine("Client id: " + clientId);
            Console.WriteLine("Time stamp: " + bodyFrame.TimeStamp);
            SerializableBody[] bodies = bodyFrame.Bodies;
            foreach (SerializableBody body in bodies)
            {
                if (body.IsTracked)
                {
                    Console.WriteLine("Trackeding ID: " + body.TrackingId);
                    Joint head = body.Joints[JointType.Head];
                    Console.WriteLine("Head: " + head.Position.X + ", " + head.Position.Y + ", " + head.Position.Z);
                }
            }

            if (clientId == 0)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.kinectViewer1.DisplayBodyFrame(bodyFrame);
                }));
            }
            else
            {
                this.kinectViewer2.DisplayBodyFrame(bodyFrame);
            }
        }
    }
}
