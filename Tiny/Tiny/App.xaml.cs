using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Tiny
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int port = 12345;
        public const int connections = 1;

        private KinectServer server;

        public App() {
            this.server = new KinectServer(App.port, App.connections);
            this.server.Start();
        }
    }
}
