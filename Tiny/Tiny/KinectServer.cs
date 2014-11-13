using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;
using KinectSerializer;
using System.Diagnostics;
using System.Windows.Threading;

namespace Tiny
{
    class KinectServer
    {
        private TcpListener tcpListener;
        private Thread listenForConnectionThread;
        private WorldCamera worldCamera;
        public event BodyStreamHandler CombinedBodyStreamUpdate;
        public event BodyStreamHandler TrackingAlgorithmUpdate;
        public delegate void BodyStreamHandler(KinectServer server, IEnumerable<SerializableBodyFrame> bodyFrames);
        private CombinedBodyViewer combinedBodyViewer;
        private CombinedBodyViewer trackingBodyViewer;

        public KinectServer(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenForConnectionThread = new Thread(new ThreadStart(this.ListenForKinectStream));
            this.worldCamera = new WorldCamera();

            Thread combinedBodyViewerThread = new Thread(new ThreadStart(this.StartCombinedBodyViewerThread));
            combinedBodyViewerThread.SetApartmentState(ApartmentState.STA);
            combinedBodyViewerThread.Start();

            //this.trackingBodyViewer = new CombinedBodyViewer();
            //this.trackingBodyViewer.Show();
            //this.trackingBodyViewer.Label.Content = "Tracking Algorithm";
            //this.TrackingAlgorithmUpdate += this.trackingBodyViewer.UpdateBodyStreamDisplay;
        }

        private void StartCombinedBodyViewerThread()
        {
            this.combinedBodyViewer = new CombinedBodyViewer();
            this.combinedBodyViewer.Show();
            this.CombinedBodyStreamUpdate += this.combinedBodyViewer.UpdateBodyStreamDisplay;
            Dispatcher.Run();
        }

        public void Start()
        {
            this.listenForConnectionThread.Start();
            Debug.WriteLine("Kinect Server: Starting @ " + this.tcpListener.LocalEndpoint);
        }

        private void ListenForKinectStream()
        {
            this.tcpListener.Start();
            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(this.HandleKinectStream));
                clientThread.Start(client);
            }
        }

        private void HandleKinectStream(object obj)
        {
            TcpClient client = (TcpClient)obj;
            IPEndPoint clientIP = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream clientStream = client.GetStream();

            Debug.WriteLine("Handling connection from: " + clientIP);

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    this.worldCamera.AddOrUpdateClientCamera(clientIP, bodyFrame);
                    Thread visualUpdateThread = new Thread(new ThreadStart(this.StartVisualUpdateThread));
                    visualUpdateThread.Start();

                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
                    Debug.WriteLine("response length: " + response.Length);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Kinect Server: Exception");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    clientStream.Close();
                    client.Close();
                }
            }

            clientStream.Close();
            clientStream.Dispose();
            client.Close();

            this.worldCamera.RemoveClientCamera(clientIP);
        }

        private void StartVisualUpdateThread()
        {
            this.worldCamera.SynchronizeFrames();
            IEnumerable<SerializableBodyFrame> clientBodyFrames = this.worldCamera.ClientBodyFrames;
            //this.CombinedBodyStreamUpdate(this, this.worldCamera.ClientBodyFrames);
            //this.TrackingAlgorithmUpdate(this, this.worldCamera.ProcessedBodyFrames);
        }
    }
}
