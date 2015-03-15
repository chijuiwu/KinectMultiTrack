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
using KinectMultiTrack.Experiments;

namespace KinectMultiTrack.UI
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        public class UserScenario
        {
            public int ScenarioId { get; private set; }
            public IEnumerable<string> Tasks { get; private set; }

            public UserScenario(int loggingId, IEnumerable<string> tasks)
            {
                this.ScenarioId = loggingId;
                this.Tasks = tasks;
            }
        }

        private Dictionary<RadioButton, UserScenario> userScenariosDict;
        private Dictionary<RadioButton, int> kinectConfigurationsDict;

        public int Kinect_Count { get; private set; }
        public bool User_Study_On { get; private set; }
        public int User_Study_Id { get; private set; }
        public UserScenario User_Scenario { get; private set; }
        public int Kinect_Configuration { get; private set; }

        public SetupDialog()
        {
            this.InitializeComponent();

            // Set up user scenarios with ids from the Logger
            this.userScenariosDict = new Dictionary<RadioButton, UserScenario>()
            {
                {this.User_Scenario_First_3, new UserScenario(Logger.SCENARIO_FIRST_3, Tasks.FIRST_3)},
                {this.User_Scenario_Stationary, new UserScenario(Logger.SCENARIO_STATIONARY, Tasks.STATIONARY)},
                {this.User_Scenario_Walk_Wei, new UserScenario(Logger.SCENARIO_WALK_WEI, Tasks.WALK_WEI)},
                {this.User_Scenario_Walk_Current, new UserScenario(Logger.SCENARIO_WALK_CURRENT, Tasks.WALK_CURRENT)},
                {this.User_Scenario_Interaction_1, new UserScenario(Logger.SCENARIO_INTERACTION_1, Tasks.INTERACTION_1)},
                {this.User_Scenario_Interaction_2, new UserScenario(Logger.SCENARIO_INTERACTION_2, Tasks.INTERACTION_2)},
                {this.User_Scenario_Occlusion_1, new UserScenario(Logger.SCENARIO_OCCLUSION_1, Tasks.OCCLUSION_1)},
                {this.User_Scenario_Free, new UserScenario(Logger.SCENARIO_FREE, Tasks.EMPTY)}
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
            this.EnableUserSetup(this.User_Study_OnOff.IsChecked.HasValue && this.User_Study_OnOff.IsChecked.Value);
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

        private bool GetStudyOn()
        {
            return this.User_Study_OnOff.IsChecked.HasValue && this.User_Study_OnOff.IsChecked.Value;
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

        private UserScenario GetUserScenario()
        {
            foreach (RadioButton setting in this.userScenariosDict.Keys)
            {
                if (setting.IsChecked.HasValue && setting.IsChecked.Value)
                {
                    return this.userScenariosDict[setting];
                }
            }
            return new UserScenario(Logger.NA, Tasks.EMPTY);
        }

        private int GetKinectConfiguration()
        {
            foreach (RadioButton setting in this.kinectConfigurationsDict.Keys)
            {
                if (setting.IsChecked.HasValue && setting.IsChecked.Value)
                {
                    return this.kinectConfigurationsDict[setting];
                }
            }
            return Logger.NA;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Kinect_Count = this.GetKinectCount();
            this.User_Study_On = this.GetStudyOn();
            this.User_Study_Id = this.GetStudyId();
            this.User_Scenario = this.GetUserScenario();
            this.Kinect_Configuration = this.GetKinectConfiguration();
            this.DialogResult = true;
        }
    }
}
