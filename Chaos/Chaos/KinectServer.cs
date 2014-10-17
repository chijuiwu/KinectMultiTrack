using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chaos
{
    class KinectServer
    {
        private int port;

        public KinectServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            UdpClient server = new UdpClient(this.port);
            while (true)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.port);
                Byte[] receiveBytes = server.Receive(ref endPoint);
                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("Received: " + receiveString + " from: " + endPoint);
                server.Send(new byte[] { 1 }, 1, endPoint);
            }
        }
    }
}
