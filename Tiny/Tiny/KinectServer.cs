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

namespace Tiny
{
    class KinectServer
    {
        private TcpListener tcpListener;
        private Thread listenForConnectionThread;
        private WorldCamera worldCamera;
        public event BodyStreamHandler BodyStreamUpdate;
        public delegate void BodyStreamHandler(KinectServer server, IEnumerable<SerializableBodyFrame> bodyFrames);
        public event TrackingHandler TrackingAlgorithmUpdate;
        public delegate void TrackingHandler(KinectServer server, SerializableBodyFrame bodyFrame);
        private CombinedBodyViewer combinedBodyViewer;
        private KinectBodyViewer trackingBodyViewer;

        public KinectServer(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenForConnectionThread = new Thread(new ThreadStart(this.ListenForKinectStream));
            this.worldCamera = new WorldCamera();

            this.combinedBodyViewer = new CombinedBodyViewer();
            this.combinedBodyViewer.Show();
            this.BodyStreamUpdate += this.combinedBodyViewer.UpdateBodyStreamDisplay;

            this.trackingBodyViewer = new KinectBodyViewer();
            this.trackingBodyViewer.Show();
            this.trackingBodyViewer.Label.Content = "Tracking Algorithm";
            this.TrackingAlgorithmUpdate += this.trackingBodyViewer.UpdateBodyStreamDisplay;
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

            KinectCamera clientCamera = new KinectCamera(clientIP);
            this.worldCamera.AddOrUpdateClientCamera(clientCamera);

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    clientCamera.updateBodyFrame(bodyFrame);
                    this.worldCamera.SynchronizeFrames();
                    this.BodyStreamUpdate(this, this.worldCamera.ClientBodyFrames);
                    this.TrackingAlgorithmUpdate(this, this.worldCamera.BodyFrame);

                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
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

            this.worldCamera.RemoveClientCamera(clientCamera);
        }

    }
}
