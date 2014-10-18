using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string host = "138.251.213.248";
        public const string localhost = "127.0.0.1";
        public const int port = 12345;

        public MainWindow()
        {
            InitializeComponent();
            KinectClient client = new KinectClient(localhost, port);
            client.Start();
        }
    }
}
