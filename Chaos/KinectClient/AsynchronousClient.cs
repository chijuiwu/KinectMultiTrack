using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KinectClient
{
    class AsynchronousClient
    {
        private const string ip = "localhost";
        private const int port = 11000;

        public AsynchronousClient()
        {

        }

        public void StartClient()
        {
            try
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
        }
    }
}
