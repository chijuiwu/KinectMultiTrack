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
using System.Diagnostics;

namespace KinectMultiTrack.UI
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        private Dictionary<RadioButton, int> userScenariosDict;
        private Dictionary<RadioButton, int> kinectConfigurationsDict;

        public int Kinect_Count { get; private set; }
        public bool User_Study_On { get; private set; }
        public int User_Study_Id { get; private set; }
        public int User_Scenario { get; private set; }
        public int Kinect_Configuration { get; private set; }

        public SetupDialog()
        {
            InitializeComponent();

            // Set up user scenarios with ids from the Logger
            this.userScenariosDict = new Dictionary<RadioButton, int>()
            {
                {this.User_Scenario_All, Logger.SCENARIO_ALL},
                {this.User_Scenario_Stationary, Logger.SCENARIO_STATIONARY},
                {this.User_Scenario_Walk_Wei, Logger.SCENARIO_WALK_WEI},
                {this.User_Scenario_Walk_Current, Logger.SCENARIO_WALK_CURRENT},
                {this.User_Scenario_Move_1, Logger.SCENARIO_MOVE_1},
                {this.User_Scenario_Move_2, Logger.SCENARIO_MOVE_2},
                {this.User_Scenario_Occlusion_1, Logger.SCENARIO_OCCLUSION}
            };

            // Set up Kinect configurations with ids from the Logger
            this.kinectConfigurationsDict = new Dictionary<RadioButton, int>()
            {
                {this.Kinect_Parallel, Logger.KINECT_PARALLEL},
                {this.Kinect_Right45, Logger.KINECT_RIGHT_45},
                {this.Kinect_Right90, Logger.KINECT_RIGHT_90},
                {this.Kinect_Left45, Logger.KINECT_LEFT_45},
                {this.Kinect_Left90, Logger.KINECT_LEFT_90}
            };
        }

        private void EnableUserSetup(bool enabled)
        {
            this.User_Study_Id_Entry.IsEnabled = enabled;
            foreach (RadioButton scenario in this.userScenariosDict.Keys)
            {
                scenario.IsEnabled = enabled;
            }
            foreach (RadioButton configuration in this.kinectConfigurationsDict.Keys)
            {
                configuration.IsEnabled = enabled;
            }
        }

        private void User_Study_OnOff_Click(object sender, RoutedEventArgs e)
        {
            bool enabled = (sender as CheckBox).IsChecked.HasValue && (sender as CheckBox).IsChecked.Value;
            this.EnableUserSetup(enabled);
            this.User_Study_On = enabled;
        }

        private int GetKinectCount()
        {
            int count;
            if (Int32.TryParse(this.Kinect_Count_Entry.Text, out count))
            {
                return count;
            }
            else
            {
                return Logger.NA;
            }
        }

        private int GetStudyId()
        {
            int id;
            if (Int32.TryParse(this.User_Study_Id_Entry.Text, out id))
            {
                return id;
            }
            else
            {
                return Logger.NA;
            }
        }

        private int GetSetupLoggerId(Dictionary<RadioButton, int> settingsGroup)
        {
            foreach (RadioButton setting in settingsGroup.Keys)
            {
                if (setting.IsChecked.HasValue && setting.IsChecked.Value)
                {
                    return settingsGroup[setting];
                }
            }
            return Logger.NA;
        }

        private int GetUserScenario()
        {
            return this.GetSetupLoggerId(this.userScenariosDict);
        }

        private int GetKinectConfiguration()
        {
            return this.GetSetupLoggerId(this.kinectConfigurationsDict);
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Kinect_Count = this.GetKinectCount();
            if (this.User_Study_On)
            {
                this.User_Study_Id = this.GetStudyId();
                this.User_Scenario = this.GetUserScenario();
                this.Kinect_Configuration = this.GetKinectConfiguration();
            }
            else
            {
                this.User_Study_Id = Logger.NA;
                this.User_Scenario = Logger.NA;
                this.Kinect_Configuration = Logger.NA;
            }
            this.DialogResult = true;
        }
    }
}
