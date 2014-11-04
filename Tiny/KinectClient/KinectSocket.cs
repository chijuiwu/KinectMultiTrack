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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace KinectClient
{
    class KinectSocket
    {
        private string host;
        private int port;
        private IPEndPoint endPoint;

        private TcpClient connectionToServer;
        private NetworkStream clientStream;

        public KinectSocket(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.endPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);

            try
            {
                this.connectionToServer = new TcpClient();
                this.connectionToServer.Connect(this.endPoint);
                this.clientStream = connectionToServer.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine("Kinect Client: Exception...");
                Console.WriteLine(e.Message);
                this.clientStream = null;
                this.connectionToServer = null;
            }
        }

        public void CloseConnection()
        {
            if (this.clientStream != null)
            {
                this.clientStream.Close();
            }
            if (this.connectionToServer != null)
            {
                this.connectionToServer.Close();
            }
        }

        private bool CanWriteToServer()
        {
            if (this.connectionToServer == null || this.clientStream == null)
            {
                return false;
            }
            else
            {
                return connectionToServer.Connected && this.clientStream.CanWrite;
            }
        }

        public void SendKinectBodyFrame(TimeSpan timeStamp, Body[] bodies)
        {
            if (!this.CanWriteToServer()) return;

            Thread kinectStreamThread = new Thread(usused => SendBodyFrameAsThread((object)timeStamp, (object)bodies));
            kinectStreamThread.Start();
        }

        private void SendBodyFrameAsThread(object timeStamp, object bodies)
        {
            Debug.Assert(timeStamp.GetType() == typeof(TimeSpan));
            Debug.Assert(bodies.GetType)
            //BodyFrame bodyFrameObject = (BodyFrame) object;
            if (!this.CanWriteToServer()) return;

            try
            {
                Console.WriteLine("Kinect Client: Sending BodyFrame...");
                byte[] bodyInBinary = this.ObjectToByteArray(bodyFrame);
                this.clientStream.Write(bodyInBinary, 0, bodyInBinary.Length);
                this.clientStream.Flush();

                while (!clientStream.DataAvailable) ;

                byte[] responseRaw = new byte[connectionToServer.Available];
                this.clientStream.Read(responseRaw, 0, responseRaw.Length);
                string response = Encoding.ASCII.GetString(responseRaw, 0, responseRaw.Length);
                Console.WriteLine("Kinect Client: Received " + response + " from: " + this.endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Kinect Client: Exception when communicating with the server...");
                Console.WriteLine(e.Message);
            }
        }

        // Convert any object to byte[]
        // Source: http://stackoverflow.com/questions/4865104/convert-any-object-to-a-bytes
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            //return Encoding.ASCII.GetBytes("Kinect Body");
        }
    }
}
