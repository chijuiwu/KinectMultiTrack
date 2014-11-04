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
using BodyFrameSerializer = KinectSerializer.BodyFrameSerializer;

namespace KinectClient
{
    class KinectSocket
    {
        private string host;
        private int port;
        private IPEndPoint endPoint;

        private TcpClient connectionToServer;
        private NetworkStream serverStream;

        public KinectSocket(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.endPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);

            try
            {
                this.connectionToServer = new TcpClient();
                this.connectionToServer.Connect(this.endPoint);
                this.serverStream = connectionToServer.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine("Kinect Client: Exception...");
                Console.WriteLine(e.Message);
                this.serverStream = null;
                this.connectionToServer = null;
            }
        }

        public void CloseConnection()
        {
            if (this.serverStream != null)
            {
                this.serverStream.Close();
            }
            if (this.connectionToServer != null)
            {
                this.connectionToServer.Close();
            }
        }

        private bool CanWriteToServer()
        {
            if (this.connectionToServer == null || this.serverStream == null)
            {
                return false;
            }
            else
            {
                return connectionToServer.Connected && this.serverStream.CanWrite;
            }
        }

        public void SendKinectBodyFrame(TimeSpan timeSpan, Body[] bodies)
        {
            if (!this.CanWriteToServer()) return;

            Thread kinectStreamThread = new Thread(usused => SendBodyFrameAsThread((object)timeSpan, (object)bodies));
            kinectStreamThread.Start();
        }

        private void SendBodyFrameAsThread(object timeSpan, object bodies)
        {
            Debug.Assert(timeSpan.GetType() == typeof(TimeSpan));
            Debug.Assert(bodies.GetType() == typeof(Body[]));
        
            if (!this.CanWriteToServer()) return;

            BinaryFormatter bf = new BinaryFormatter();
            TimeSpan timeSpanObject = (TimeSpan)timeSpan;
            Body[] bodiesObject = (Body[])bodies;

            try
            {
                //Console.WriteLine("Kinect Client: Sending BodyFrame...");
                byte[] bodyInBinary = BodyFrameSerializer.Serialize(timeSpanObject, bodiesObject);
                Console.WriteLine("body bin length: " + bodyInBinary.Length);
                this.serverStream.Write(bodyInBinary, 0, bodyInBinary.Length);
                this.serverStream.Flush();

                while (!serverStream.DataAvailable) ;

                byte[] responseRaw = new byte[connectionToServer.Available];
                this.serverStream.Read(responseRaw, 0, responseRaw.Length);
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
