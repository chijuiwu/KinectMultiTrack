using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;
using KinectSerializer;
using System.Diagnostics;
using System.Windows.Threading;
using Tiny.UI;

namespace Tiny
{
    public class TrackingServer
    {
        private TcpListener kinectListener;
        private Thread acceptKinectConnectionThread;

        private Tracker tracker;
        private MultipleKinectUI multipleKinectUI;
        private TrackingUI trackingUI;

        private TrackingLogger.TrackingLogger TrackingLogger;

        private event KinectCameraHandler NewKinectCameraConnected;
        private event KinectCameraHandler KinectCameraRemoved;
        private delegate void KinectCameraHandler(IPEndPoint kinectClientIP);

        private event KinectFrameHandler MultipleKinectUpdate;
        private delegate void KinectFrameHandler(Tracker.Result result);

        private event WorldViewHandler TrackingUpdate;
        private delegate void WorldViewHandler(Tracker.Result result);

        public TrackingServer(int port, int kinectCount)
        {
            this.kinectListener = new TcpListener(IPAddress.Any, port);
            this.acceptKinectConnectionThread = new Thread(new ThreadStart(this.AcceptKinectConnectionThread));
            
            this.tracker = new Tracker(kinectCount);

            Thread multipleKinectUIThread = new Thread(new ThreadStart(this.StartMultipleKinectUIThread));
            multipleKinectUIThread.SetApartmentState(ApartmentState.STA);
            multipleKinectUIThread.Start();

            Thread trackingUIThread = new Thread(new ThreadStart(this.StartTrackingUIThread));
            trackingUIThread.SetApartmentState(ApartmentState.STA);
            trackingUIThread.Start();

            this.TrackingLogger = new TrackingLogger();
        }

        // Run the tracking server
        public void Run()
        {
            this.acceptKinectConnectionThread.Start();
            Debug.WriteLine(Tiny.Properties.Resources.SERVER_START + this.kinectListener.LocalEndpoint);
        }

        private void StartMultipleKinectUIThread()
        {
            this.multipleKinectUI = new MultipleKinectUI();
            this.multipleKinectUI.Show();
            this.MultipleKinectUpdate += this.multipleKinectUI.UpdateDisplay;
            Dispatcher.Run();
        }

        private void StartTrackingUIThread()
        {
            this.trackingUI = new TrackingUI();
            this.trackingUI.Show();
            this.TrackingUpdate += this.trackingUI.UpdateDisplay;
            this.NewKinectCameraConnected += this.trackingUI.AddKinectCamera;
            this.KinectCameraRemoved += this.trackingUI.RemoveKinectCamera;
            Dispatcher.Run();
        }

        private void AcceptKinectConnectionThread()
        {
            this.kinectListener.Start();
            while (true)
            {
                TcpClient kinectClient = this.kinectListener.AcceptTcpClient();
                Thread kinectClientThread = new Thread(new ParameterizedThreadStart(this.HandleKinectConnectionThread));
                kinectClientThread.Start(kinectClient);
            }
        }

        private void HandleKinectConnectionThread(object obj)
        {
            TcpClient client = obj as TcpClient;
            IPEndPoint clientIP = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream clientStream = client.GetStream();
            
            Debug.WriteLine(Tiny.Properties.Resources.CONNECTION_START + clientIP);
            Boolean cameraRecorded = false;

            while (true)
            {
                if (!cameraRecorded && this.NewKinectCameraConnected != null)
                {
                    Thread addCameraThread = new Thread(() => this.NewKinectCameraConnected(clientIP));
                    addCameraThread.Start();
                    cameraRecorded = true;
                }

                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    Thread trackingUpdateThread = new Thread(() => this.StartTrackingUpdateThread(clientIP, bodyFrame));
                    trackingUpdateThread.Start();

                    // Response content is trivial
                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(Tiny.Properties.Resources.SERVER_EXCEPTION);
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    clientStream.Close();
                    client.Close();
                }
            }
            this.tracker.RemoveClient(clientIP);
            Thread removeCameraThread = new Thread(() => this.KinectCameraRemoved(clientIP));
            removeCameraThread.Start();
            clientStream.Close();
            clientStream.Dispose();
            client.Close();
        }

        private void StartTrackingUpdateThread(IPEndPoint clientIP, SBodyFrame bodyFrame)
        {
            Tracker.Result result = this.tracker.Synchronize(clientIP, bodyFrame);
            this.MultipleKinectUpdate(result);
            this.TrackingUpdate(result);

            //Thread loggingThread = new Thread(new ParameterizedThreadStart(this.StartLoggingThread));
            //loggingThread.Start(result);
        }

        private void StartLoggingThread(object obj)
        {
            Tracker.Result result = obj as Tracker.Result;
            this.TrackingLogger.Write(result);
        }
    }
}
