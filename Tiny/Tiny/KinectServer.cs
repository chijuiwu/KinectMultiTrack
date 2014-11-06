using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;
using KinectSerializer;
using BodyFrameSerializer = KinectSerializer.BodyFrameSerializer;

namespace Tiny
{
    class KinectServer
    {
        private int port;
        private TcpListener tcpListener;
        private List<IPEndPoint> connectedClients;
        private Thread listenThread;

        private KinectServerWindow kinectServerWindow;

        public KinectServer(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.connectedClients = new List<IPEndPoint>();

            this.kinectServerWindow = new KinectServerWindow();
            this.kinectServerWindow.Show();
        }

        public void Start()
        {
            this.listenThread = new Thread(new ThreadStart(this.ListenForKinectStream));
            this.listenThread.Start();
            Console.WriteLine("Kinect Server: Starting @ port " + port + "...");
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
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            this.connectedClients.Add(endPoint);
            int clientId = this.connectedClients.IndexOf(endPoint);

            NetworkStream clientStream = client.GetStream();

            while (true)
            {
                try
                {
                    if (!client.Connected)
                        break;

                    while (!clientStream.DataAvailable)
                        ;

                    SerializableBodyFrame bodyFrame = BodyFrameSerializer.Deserialize(clientStream);
                    this.kinectServerWindow.ProcessKinectBodyFrame(bodyFrame, clientId);

                    byte[] response = Encoding.ASCII.GetBytes(Properties.Resources.SERVER_RESPONSE_OKAY);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Kinect Server: Exception when communicating with the client...");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    clientStream.Close();
                    client.Close();
                }
            }

            this.connectedClients.RemoveAt(clientId);
        }
        
    }
}
