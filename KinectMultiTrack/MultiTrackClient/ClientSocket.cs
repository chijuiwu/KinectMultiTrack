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
using KinectSerializer;

namespace KinectClient
{
    class ClientSocket
    {
        private IPEndPoint endPoint;
        private TcpClient connectionToServer;
        private Thread connectionThread;
        private NetworkStream serverStream;


        public ClientSocket(string host, int port)
        {
            this.endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            this.connectionThread = new Thread(new ThreadStart(this.ConnectionWorkerThread));
            this.connectionThread.Start();
        }

        private void ConnectionWorkerThread()
        {
            while(true)
            {
                if (this.CanWriteToServer())
                {
                    continue;
                }
                try
                {
                    this.connectionToServer = new TcpClient();
                    this.connectionToServer.Connect(this.endPoint);
                    this.serverStream = this.connectionToServer.GetStream();
                }
                catch (Exception)
                {
                    Debug.WriteLine("Connectio to the server failed. Re-trying...", "KinectClient");
                }
            }
        }

        public void CloseConnection()
        {
            Debug.WriteLine("here");
            if (this.connectionThread != null)
            {
                this.connectionThread.Abort();
            }
            if (this.serverStream != null)
            {
                this.serverStream.Close();
                this.serverStream.Dispose();
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
                return this.connectionToServer.Connected && this.serverStream.CanWrite;
            }
        }

        public void SendSerializedKinectBodyFrame(SBodyFrame serializableBodyFrame)
        {
            if (this.CanWriteToServer())
            {
                this.SendKinectBodyFrame(serializableBodyFrame);
            }
        }

        private void SendKinectBodyFrame(SBodyFrame serializableBodyFrame)
        {
            if (this.CanWriteToServer())
            {
                try
                {
                    byte[] bodyInBinary = BodyFrameSerializer.Serialize(serializableBodyFrame);
                    this.serverStream.Write(bodyInBinary, 0, bodyInBinary.Length);
                    this.serverStream.Flush();

                    byte[] responseRaw = new byte[256];
                    this.serverStream.Read(responseRaw, 0, responseRaw.Length);
                    string response = Encoding.ASCII.GetString(responseRaw, 0, responseRaw.Length);
                    Debug.WriteLine("OKAY", "Kinect Client");
                }
                catch (Exception)
                {
                    Debug.WriteLine("Failed to transmit data", "Kinect Client");
                }
            }
        }
    }
}
