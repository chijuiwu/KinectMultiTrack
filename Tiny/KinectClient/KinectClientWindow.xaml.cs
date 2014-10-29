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
using System.Threading;
using System.Diagnostics;
using Microsoft.Kinect;

namespace KinectSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class KinectClientWindow : Window
    {
        private const string kinectServerAddress = "138.251.213.248";
        private const string localhost = "127.0.0.1";
        private const int kinectServerPort = 12345;
        private KinectClient client;
        private Thread kinectStreamThread;

        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;

        public KinectClientWindow()
        {
            InitializeComponent();
            this.client = new KinectClient(localhost, kinectServerPort);

            this.InitializeKinect();

            this.kinectStreamThread = new Thread(new ThreadStart(this.StartKinectStream));
            this.kinectStreamThread.Start();
        }

        private void InitializeKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.kinectSensor.IsAvailableChanged += this.kinectSensor_IsAvailableChanged;
        }

        private void kinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void StartKinectStream()
        {
            Stopwatch streamInterval = new Stopwatch();
            streamInterval.Start();
            while (true)
            {
                if (streamInterval.ElapsedMilliseconds >= 2000)
                {
                    //this.client.SpawnKinectBodySocket();
                    streamInterval.Restart();
                }
            }
        }
    }
}
