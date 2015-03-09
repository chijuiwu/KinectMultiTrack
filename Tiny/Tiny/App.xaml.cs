using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KinectMultiTrack
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int serverPort = 12345;

        private Server server;

        public App() {
            this.server = new Server(App.serverPort);
        }
    }
}
