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
using System.Windows.Shapes;
using System.Windows.Controls;

namespace KinectMultiTrack.UI
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        List<Control> userSetupControlsList;

        public SetupDialog()
        {
            InitializeComponent();
            this.userSetupControlsList = new List<Control>()
            {
                this.User_Study_Id,
                this.User_Scenario_All,
            };
        }

        private void EnableUserSetup(bool enabled)
        {
            foreach (Control userSetup in this.userSetupControlsList)
            {
                if (enabled)
                {
                    userSetup.IsEnabled = true;
                }
                else
                {
                    userSetup.IsEnabled = false;
                }
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void User_Study_Click(object sender, RoutedEventArgs e)
        {
            if (this.User_Study_OnOff.IsEnabled)
            {
                this.EnableUserSetup(true);
            }
            else
            {
                this.EnableUserSetup(false);
            }
        }
    }
}
