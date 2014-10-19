using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Tiny
{
    class KinectServer
    {
        private int port;
        private TcpListener tcpListener;
        private Thread listenThread;

        public KinectServer(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
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
                TcpClient server = this.tcpListener.AcceptTcpClient();
                Thread serverThread = new Thread(new ParameterizedThreadStart(this.HandleKinectStream));
                serverThread.Start(server);
            }
        }

        private void HandleKinectStream(object obj)
        {
            TcpClient server = (TcpClient)obj;
            NetworkStream serverStream = server.GetStream();
            
            while (!serverStream.DataAvailable);

            byte[] bytes = new byte[server.Available];
            serverStream.Read(bytes, 0, bytes.Length);
            string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            IPEndPoint endPoint = (IPEndPoint)server.Client.RemoteEndPoint;
            Console.WriteLine("Kinect Server: Received " + message + " from: " + endPoint);

            string okay = "Okay";
            byte[] response = Encoding.ASCII.GetBytes(okay);
            serverStream.Write(response, 0, response.Length);
            serverStream.Flush();

            serverStream.Close();
            server.Close();
        }
    }
}
