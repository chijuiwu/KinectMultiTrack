using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMultiTrack.Experiments
{

    public class Tasks {

        // Stand
        public static string STAND_STILL = "Stand still";

        // Walk
        public static string LEFT = "Move Left";
        public static string RIGHT = "Move Right";
        public static string BACKWARD = "Move Backward";
        public static string FORWARD = "Move Forward";
        public static string START_POSITION = "Move to the\nStarting Position";
        public static string CIRCLE = "Go Around the\nCircle (Clockwise)";

        // Interaction
        public static string PERSON_1_WALK = "Person 1\nWalk Past Person2";
        public static string PERSONS_WALK = "Two People\nWalk Past Each Other";

        // Occlusion
        public static string GO_AROUND_OCCLUSION = "Go Around the\nOcclusion Object";

        // Instruction
        public static string READY = "Ready";
        public static string DONE = "Done!!!";

        public const int STATIONARY_DURATION_SEC = 5;

        public static readonly IEnumerable<string> EMPTY = new List<string>();

        public static readonly IEnumerable<string> STATIONARY = new List<string>()
        {
            Tasks.READY,
            Tasks.STAND_STILL,
            Tasks.DONE
        };

        public static readonly IEnumerable<string> WALK_WEI = new List<string>()
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

        public static readonly IEnumerable<string> WALK_CURRENT = new List<string>()
        {
            Tasks.READY,
            Tasks.START_POSITION,
            Tasks.CIRCLE,
            Tasks.DONE
        };

        public static readonly IEnumerable<string> FIRST_3 = Tasks.STATIONARY.Concat(Tasks.WALK_WEI).Concat(Tasks.WALK_CURRENT);

        public static readonly IEnumerable<string> INTERACTION_1 = new List<string>()
        {
            Tasks.READY,
            Tasks.PERSON_1_WALK,
            Tasks.DONE
        };

        public static readonly IEnumerable<string> INTERACTION_2 = new List<string>()
        {
            Tasks.READY,
            Tasks.PERSONS_WALK,
            Tasks.DONE
        };

        public static readonly IEnumerable<string> OCCLUSION_1 = new List<string>()
        {
            Tasks.READY,
            Tasks.GO_AROUND_OCCLUSION,
            Tasks.DONE
        };
    }

}
