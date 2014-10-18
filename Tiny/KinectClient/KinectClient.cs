using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KinectSocket
{
    class KinectClient
    {
        private string host;
        private int port;

        public KinectClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Start()
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            UdpClient client = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);
            client.Connect(endPoint);
            try
            {
                while (true)
                {
                    string skeletonHeader = "Skeleton";
                    byte[] bytesSkeletonHeader = ascii.GetBytes(skeletonHeader);
                    Console.WriteLine("Sending Skeleton...");
                    client.Send(bytesSkeletonHeader, bytesSkeletonHeader.Length);
                    Byte[] receiveBytes = client.Receive(ref endPoint);
                    string receiveString = ascii.GetString(receiveBytes, 0, receiveBytes.Length);
                    Console.WriteLine("Received: " + receiveString + " from: " + endPoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Kinect Client...");
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
