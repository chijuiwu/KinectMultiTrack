using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiny
{
    public class TrackerSetup
    {
        public int KinectsCount { get; private set; }
        public bool Logging { get; private set; }
        public int StudyId { get; private set; }
        public int Scenario { get; private set; }

        public TrackerSetup(int kinectsCount, bool log, int studyId, int scenario)
        {
            this.KinectsCount = kinectsCount;
            this.Logging = log;
            this.StudyId = studyId;
            this.Scenario = scenario;
        }
    }
}
