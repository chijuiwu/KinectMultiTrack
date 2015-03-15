using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMultiTrack.Experiments
{

    public class Tasks {

        // Walk
        public static string LEFT = "Move Left";
        public static string RIGHT = "Move Right";
        public static string BACKWARD = "Move Backward";
        public static string FORWARD = "Move Forward";
        public static string START_POSITION = "Move to Starting Position";
        public static string CIRCLE = "Go Around the Circle(Clockwise)";

        // Interaction
        public static string PERSON_1_WALK = "Person 1 Walk Past Person 2";
        public static string PERSONS_WALK = "Two People Walk Past Each Other";

        // Occlusion
        public static string GO_AROUND_OCCLUSION = "Go Around the Occlusion Object";

        // Instruction
        public static string READY = "Ready";
        public static string DONE = "Done!!!";

        public const int STATIONARY_DURATION_SEC = 5;

        public static readonly List<string> WALK_WEI = new List<string>()
        {
            Tasks.READY,
            Tasks.FORWARD,
            Tasks.LEFT,
            Tasks.RIGHT,
            Tasks.BACKWARD,
            Tasks.BACKWARD,
            Tasks.LEFT,
            Tasks.BACKWARD,
            Tasks.RIGHT,
            Tasks.DONE
        };

        public static readonly List<string> WALK_CURRENT = new List<string>()
        {
            Tasks.READY,
            Tasks.START_POSITION,
            Tasks.CIRCLE,
            Tasks.DONE
        };

        public static readonly List<string> INTERACTION_1 = new List<string>()
        {
            Tasks.READY,
            Tasks.PERSON_1_WALK,
            Tasks.DONE
        };

        public static readonly List<string> INTERACTION_2 = new List<string>()
        {
            Tasks.READY,
            Tasks.PERSONS_WALK,
            Tasks.DONE
        };

        public static readonly List<string> OCCLUSION_1 = new List<string>()
        {
            Tasks.READY,
            Tasks.GO_AROUND_OCCLUSION,
            Tasks.DONE
        };
    }

}
