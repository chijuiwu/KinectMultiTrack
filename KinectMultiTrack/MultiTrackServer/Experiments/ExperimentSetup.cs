using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMultiTrack.Experiments
{
    public enum Movement
    {
        Forward, Left, Right, Backward
    }

    public class ExperimentSetup {
        // Seconds
        public const int STATIONARY_DURATION = 10;

        public static readonly List<Movement> MOVEMENTS = ExperimentSetup.GetMovementsInWeiStudy();

        private static List<Movement> GetMovementsInWeiStudy()
        {
            return new List<Movement>(){
                Movement.Forward,
                Movement.Left,
                Movement.Right,
                Movement.Backward,
                Movement.Backward,
                Movement.Left,
                Movement.Backward,
                Movement.Right
            };
        }

        private static List<Movement> GetMovementsInCurrentStudy()
        {
            return new List<Movement>()
            {
                Movement.Right,
                Movement.Forward,
                Movement.Backward,
                Movement.Left,
                Movement.Left,
                Movement.Forward,
                Movement.Right,
                Movement.Backward
            };
        }
    }

}
