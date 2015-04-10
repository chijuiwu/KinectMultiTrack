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
        public const int DEFAULT_PORT = 12345;
        public int PORT = DEFAULT_PORT;

        private Server server;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                this.PORT = Convert.ToInt32(e.Args[0]);
            }
            Console.WriteLine("Configured server to run at port: " + this.PORT);
            this.server = new Server(this.PORT);
        }

        public App() {
        }
    }
}
