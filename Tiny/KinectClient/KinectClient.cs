using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using Microsoft.Kinect;

namespace KinectSocket
{
    class KinectClient
    {
        private string host;
        private int port;
        private IPEndPoint endPoint;

        public KinectClient(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.endPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);
        }

        public void SpawnKinectBodySocket(Body[] bodies)
        {
            Thread kinectStreamThread = new Thread(new ParameterizedThreadStart(this.SendKinectBodyData));
            kinectStreamThread.Start((object)bodies);
        }

        private void SendKinectBodyData(object bodies)
        {
            if (bodies i)
            try
            {
                TcpClient connectionToServer = new TcpClient();
                connectionToServer.Connect(this.endPoint);
                NetworkStream clientStream = connectionToServer.GetStream();

                Console.WriteLine("Kinect Client: Sending Skeleton...");
                string skeleton = "Skeleton";
                byte[] message = Encoding.ASCII.GetBytes(skeleton);
                clientStream.Write(message, 0, message.Length);
                clientStream.Flush();

                while (!clientStream.DataAvailable) ;

                byte[] responseRaw = new byte[connectionToServer.Available];
                clientStream.Read(responseRaw, 0, responseRaw.Length);
                string response = Encoding.ASCII.GetString(responseRaw, 0, responseRaw.Length);
                Console.WriteLine("Kinect Client: Received " + response + " from: " + this.endPoint);

                clientStream.Close();
                connectionToServer.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Kinect Client: Unable to connect to the server...");
            }
            catch (Exception e)
            {
                Console.WriteLine("Kinect Client: Exception...");
                Console.WriteLine(e.Message);
            }
        }
    }
}
