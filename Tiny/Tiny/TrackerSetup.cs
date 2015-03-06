using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiny
{
    public class TrackerSetup
    {   
        public class KinectSetup
        {
            // Height, TiltAngle
            public IEnumerable<Tuple<float, float>> kinectsSpec { get; private set; }

            public KinectSetup(params Tuple<float, float>[] kinectsSpec)
            {
                this.kinectsSpec = kinectsSpec;
            }
        }

        public class UserSetup
        {
            public bool Log { get; private set; }
            public int Scenario { get; private set; }
            public int StudyId { get; private set; }

            public UserSetup(bool log, int scenario, int studyId)
            {
                this.Log = log;
                this.Scenario = scenario;
                this.StudyId = studyId;
            }
        }

        public KinectSetup kinectSetup { get; private set; }
        public UserSetup userSetup { get; private set; }

        public TrackerSetup(KinectSetup kinectSetup, UserSetup userSetup)
        {
            this.kinectSetup = kinectSetup;
            this.userSetup = userSetup;
        }
    }
}
