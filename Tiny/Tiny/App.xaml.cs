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
        public const int kinects = 2;

        private TServer server;

        public App() {
            this.server = new TServer(App.port, App.kinects);
            this.server.Run();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.server.Stop();
        }
    }
}
