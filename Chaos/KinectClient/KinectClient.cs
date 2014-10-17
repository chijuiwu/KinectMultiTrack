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
            UdpClient client = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);
            client.Connect(endPoint);
            try
            {
                while (true)
                {
                    client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
                    Byte[] receiveBytes = client.Receive(ref endPoint);
                    string receiveString = Encoding.ASCII.GetString(receiveBytes, 0, receiveBytes.Length);
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
