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

namespace Tiny
{
    public partial class KinectServerWindow : Window
    {
        private KinectViewer kinectViewer1;
        private KinectViewer kinectViewer2;

        public KinectServerWindow()
        {
            this.kinectViewer1 = new KinectViewer();
            this.kinectViewer1.Show();
            this.kinectViewer2 = new KinectViewer();
            this.kinectViewer2.Show();

            this.InitializeComponent();
        }

        public void ProcessKinectBodyFrame(SerializableBodyFrame bodyFrame, int clientId)
        {
            Console.WriteLine("Time stamp: " + bodyFrame.TimeStamp);
            Console.WriteLine("DepthFrame width: " + bodyFrame.DepthFrameWidth);
            Console.WriteLine("DepthFrame height: " + bodyFrame.DepthFrameHeight);
            SerializableBody[] bodies = bodyFrame.Bodies;
            foreach (SerializableBody body in bodies)
            {
                if (body.IsTracked)
                {
                    Console.WriteLine("Tracked Body: " + body.TrackingId);
                }
            }
        }
    }
}
