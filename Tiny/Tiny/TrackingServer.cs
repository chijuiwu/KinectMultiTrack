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

namespace Tiny
{
    public class TrackingServer
    {
        private TcpListener kinectListener;
        private Thread acceptKinectConnectionThread;

        private Tracker tracker;
        private MultipleKinectUI multipleKinectUI;
        private TrackingUI trackingUI;

        public event KinectFrameHandler MultipleKinectUpdate;
        public delegate void KinectFrameHandler(IEnumerable<Tuple<IPEndPoint, SerializableBodyFrame>> bodyFrames);
        
        public event WorldViewHandler TrackingUpdate;
        public delegate void WorldViewHandler(IEnumerable<Tuple<IPEndPoint, WorldView>> worldViews);

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
            this.MultipleKinectUpdate += this.multipleKinectUI.UpdateFrames;
            Dispatcher.Run();
        }

        private void StartTrackingUIThread()
        {
            this.trackingUI = new TrackingUI();
            this.trackingUI.Show();
            this.TrackingUpdate += this.trackingUI.UpdateTrackingDisplay;
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
            TcpClient client = (TcpClient)obj;
            IPEndPoint clientIP = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream clientStream = client.GetStream();

            Debug.WriteLine(Tiny.Properties.Resources.CONNECTION_START + clientIP);

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    
                    this.tracker.AddOrUpdateBodyFrame(clientIP, bodyFrame);
                    Thread uiUpdateThread = new Thread(new ThreadStart(this.StartUIUpdateThread));
                    uiUpdateThread.Start();

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
            clientStream.Close();
            clientStream.Dispose();
            client.Close();
        }

        private void StartUIUpdateThread()
        {
            Tracker.SyncFrameResult result = this.tracker.SynchronizeFrames(null);
            
            this.MultipleKinectUpdate(this.tracker.GetLatestRawFrames());
            this.TrackingUpdate(this.tracker.GetLatestFramesInWorldView(null));
        }
    }
}
