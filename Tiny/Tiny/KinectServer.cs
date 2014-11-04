using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Kinect;

namespace Tiny
{
    class KinectServer
    {
        private int port;
        private TcpListener tcpListener;
        private Thread listenThread;

        private List<IPEndPoint> connectedClients;

        public KinectServer(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, port);

            this.connectedClients = new List<IPEndPoint>();
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
                Thread serverThread = new Thread(new ParameterizedThreadStart(this.HandleKinectStream));
                serverThread.Start(client);
            }
        }

        private void HandleKinectStream(object obj)
        {
            TcpClient client = (TcpClient)obj;
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            this.connectedClients.Add(endPoint);

            NetworkStream clientStream = client.GetStream();

            while (true)
            {
                try
                {
                    if (!client.Connected) break;

                    while (!clientStream.DataAvailable) ;

                    byte[] bytes = new byte[client.Available];
                    clientStream.Read(bytes, 0, bytes.Length);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Console.WriteLine("Kinect Server: Received " + message + " from: " + endPoint);

                    string okay = "Okay";
                    byte[] response = Encoding.ASCII.GetBytes(okay);
                    clientStream.Write(response, 0, response.Length);
                    clientStream.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Kinect Server: Exception when communicating with the client...");
                    Console.WriteLine(e.Message);
                    clientStream.Close();
                    client.Close();
                }
            }

            this.connectedClients.Remove(endPoint);
        }

        private void ProcessKinectBody(Body body, int clientId)
        {

        }
    }
}
