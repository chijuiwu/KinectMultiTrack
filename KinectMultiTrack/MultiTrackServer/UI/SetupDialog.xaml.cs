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
using KinectMultiTrack.Studies;

namespace KinectMultiTrack.UI
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {

        private Dictionary<RadioButton, IEnumerable<UserTask>> userTasksDict;
        private Dictionary<RadioButton, int> kinectConfigurationsDict;

        public int Kinect_Count { get; private set; }
        public bool User_Study_On { get; private set; }
        public int User_Study_Id { get; private set; }
        public IEnumerable<UserTask> User_Task { get; private set; }
        public int Kinect_Configuration { get; private set; }

        public SetupDialog()
        {
            this.InitializeComponent();

            // Set up user scenarios with ids from the Logger
            this.userTasksDict = new Dictionary<RadioButton, IEnumerable<UserTask>>()
            {
                {this.User_Scenario_First_3, UserTask.TASK_FIRST_3},
                {this.User_Scenario_Stationary, UserTask.TASK_STATIONARY},
                {this.User_Scenario_Walk_Wei, UserTask.TASK_WALK_WEI},
                {this.User_Scenario_Walk_Current, UserTask.TASK_WALK_CURRENT},
                {this.User_Scenario_Interaction_1, UserTask.TASK_INTERACTION_1},
                {this.User_Scenario_Interaction_2, UserTask.TASK_INTERACTION_2},
                {this.User_Scenario_Occlusion_1, UserTask.TASK_OCCLUSION_1},
                {this.User_Scenario_Free, UserTask.TASK_FREE}
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
            foreach (RadioButton scenario in this.userTasksDict.Keys)
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

        private IEnumerable<UserTask> GetUserTask()
        {
            foreach (RadioButton setting in this.userTasksDict.Keys)
            {
                if (setting.IsChecked.HasValue && setting.IsChecked.Value)
                {
                    return this.userTasksDict[setting];
                }
            }
            return UserTask.TASK_EMPTY;
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
            this.User_Task = this.GetUserTask();
            this.Kinect_Configuration = this.GetKinectConfiguration();
            this.DialogResult = true;
        }
    }
}
