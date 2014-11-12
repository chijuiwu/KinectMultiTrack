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
        private ConcurrentBag<KinectCamera> connectedCameras;

        private KinectServerWindow kinectServerWindow;

        public KinectServer(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenForConnectionThread = new Thread(new ThreadStart(this.ListenForKinectStream));
            this.connectedCameras = new ConcurrentBag<KinectCamera>();

            this.kinectServerWindow = new KinectServerWindow();
            this.kinectServerWindow.Show();
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
            this.connectedCameras.Add(clientCamera);

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    clientCamera.updateBodyFrame(bodyFrame);

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

            this.connectedCameras.TryTake(out clientCamera);
        }

    }
}
