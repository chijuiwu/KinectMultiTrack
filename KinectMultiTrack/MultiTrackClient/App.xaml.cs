using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MultiTrackClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public const string DEFAULT_ADDRESS = "138.251.213.82";
        public string ADDRESS = DEFAULT_ADDRESS;
        public const int DEFAULT_PORT = 12345;
        public int PORT = DEFAULT_PORT;

        private ClientUI client;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 2)
            {
                this.ADDRESS = e.Args[0];
                this.PORT = Convert.ToInt32(e.Args[1]);
            }
            Console.WriteLine("Configured client to talk to server at address: " + this.ADDRESS + " and port: " + this.PORT);
            this.client = new ClientUI(this.ADDRESS, this.PORT);
            this.client.Show();
        }

        public App()
        {

        }
    }
}
