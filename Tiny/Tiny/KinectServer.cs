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
            ASCIIEncoding ascii = new ASCIIEncoding();
            UdpClient server = new UdpClient(this.port);
            Console.WriteLine("Starting server @ " + this.port + "...");
            while (true)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.port);
                byte[] receiveBytes = server.Receive(ref endPoint);
                string receiveString = ascii.GetString(receiveBytes, 0, receiveBytes.Length);
                Console.WriteLine("Received: " + receiveString + " from: " + endPoint);
                string okay = "Okay";
                byte[] sendBytes = ascii.GetBytes(okay);
                server.Send(sendBytes, sendBytes.Length, endPoint);
            }
        }
    }
}
