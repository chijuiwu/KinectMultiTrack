using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectMultiTrack.WorldView;
using KinectSerializer;
using System.Diagnostics;
using System.Collections.Specialized;
using System.IO;

namespace KinectMultiTrack
{
    public class Logger
    {
        public static readonly int NA = -9999;

        public static readonly int SCENARIO_NA = -9999; // Placeholder
        public static readonly int SCENARIO_STATIONARY = 1;
        public static readonly int SCENARIO_WALK_WEI = 2;
        public static readonly int SCENARIO_WALK_CURRENT = 3;
        public static readonly int SCENARIO_INTERACTION_1_P1 = 4;
        public static readonly int SCENARIO_INTERACTION_1_P2 = 8;
        public static readonly int SCENARIO_INTERACTION_2 = 5;
        public static readonly int SCENARIO_OCCLUSION_1 = 6;
        public static readonly int SCENARIO_FREE = 7;
        
        public static readonly int KINECT_PARALLEL = 0;
        public static readonly int KINECT_RIGHT_45 = 1;
        public static readonly int KINECT_RIGHT_90 = 2;
        public static readonly int KINECT_LEFT_45 = 3;
        public static readonly int KINECT_LEFT_90 = 4;

        private static readonly string STUDY = "Study";
        private static readonly string KINECT_CONFIG = "KinectConfig";
        private static readonly string USER_SCENARIO = "Scenario";
        private static readonly string TRACKER_TIME = "Tracker_Time";
        private static readonly string PERSON = "Person#";
        private static readonly string SKEL = "Skeleton#";
        private static readonly string SKEL_TIME = "Skeleton_Time";
        private static readonly string SKEL_INIT_ANGLE = "Skeleton_Init_Angle";
        private static readonly string SKEL_INIT_DIST = "Skeleton_Init_Dist";
        private static readonly string KINECT = "Kinect#";
        private static readonly string KINECT_TILT_ANGLE = "Kinect_Tilt_Angle";
        private static readonly string KINECT_HEIGHT = "Kinect_Height";
        private static readonly string X = "X";
        private static readonly string Y = "Y";
        private static readonly string Z = "Z";

        public static int CURRENT_STUDY_ID = Logger.NA;
        public static int CURRENT_USER_SCENARIO = Logger.NA;
        public static int CURRENT_KINECT_CONFIGURATION = Logger.NA;

        private static readonly string DATA_DIR = "..\\..\\..\\..\\Data\\";
        private static readonly string DATA_FILENAME_FORMAT = "Study_{0}_Kinect_{1}_Time_{2}.csv";
        private static readonly List<string> HEADERS = Logger.GetHeaders();
        private static StreamWriter WRITER;

        private static readonly object syncLogLock = new object();

        private static List<string> GetHeaders()
        {
            List<string> headers = new List<string>() { 
                Logger.STUDY,
                Logger.KINECT_CONFIG,
                Logger.USER_SCENARIO,
                Logger.TRACKER_TIME, 
                Logger.PERSON,
                Logger.SKEL,
                Logger.SKEL_TIME,
                Logger.SKEL_INIT_ANGLE,
                Logger.SKEL_INIT_DIST,
                Logger.KINECT,
                Logger.KINECT_TILT_ANGLE,
                Logger.KINECT_HEIGHT
            };
            Logger.AddJointHeaders(ref headers);
            return headers;
        }

        private static void AddJointHeaders(ref List<string> headers)
        {
            foreach (JointType jt in SkeletonStructure.Joints)
            {
                string x = jt.ToString() + "_" + Logger.X;
                string y = jt.ToString() + "_" + Logger.Y;
                string z = jt.ToString() + "_" + Logger.Z;
                headers.Add(x);
                headers.Add(y);
                headers.Add(z);
            }
        }

        public static void OpenFile(int studyId, int kinectConfiguration)
        {
            if (Logger.WRITER != null)
            {
                Logger.WRITER.Close();
            }
            Logger.CURRENT_STUDY_ID = studyId;
            Logger.CURRENT_KINECT_CONFIGURATION = kinectConfiguration;
            string filepath = Logger.DATA_DIR + String.Format(Logger.DATA_FILENAME_FORMAT, Logger.CURRENT_STUDY_ID, Logger.CURRENT_KINECT_CONFIGURATION, DateTime.Now.ToString("h_mm_ss_tt"));
            Logger.WRITER = Logger.OpenFileWriter(filepath);
        }

        private static StreamWriter OpenFileWriter(string filepath)
        {
            Logger.CreateFileIfNotExists(filepath, Logger.HEADERS);
            return new StreamWriter(filepath, true);
        }

        private static void CreateFileIfNotExists(string filepath, List<string> headers)
        {
            using (StreamReader r = new StreamReader(File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
            {
                string header = r.ReadLine();
                if (header == null)
                {
                    using (StreamWriter w = new StreamWriter(filepath, true))
                    {
                        string prefix = "";
                        foreach (string h in headers)
                        {
                            w.Write(prefix + h);
                            prefix = ", ";
                        }
                        w.WriteLine();
                    }
                }
            }
        }

        public static void SynchronizeLogging(TrackerResult result, int userScenario)
        {
            lock (Logger.syncLogLock)
            {
                Logger.CURRENT_USER_SCENARIO = userScenario;
                foreach (TrackerResult.Person person in result.People)
                {
                    TrackerResult.PotentialSkeleton reference = TrackerResult.GetLocalSkeletonReference(person);
                    double referenceAngle = reference.Skeleton.InitialAngle;
                    WCoordinate referencePosition = reference.Skeleton.InitialCenterPosition;
                    List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> skeletonCoordinates = new List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>>();
                    foreach (TrackerResult.PotentialSkeleton skeleton in person.PotentialSkeletons)
                    {
                        if (skeleton.Skeleton.CurrentPosition != null)
                        {
                            KinectBody body = WBody.TransformWorldToKinectBody(skeleton.Skeleton.CurrentPosition.Worldview, referenceAngle, referencePosition);
                            skeletonCoordinates.Add(Tuple.Create(skeleton, body.Joints));
                        }
                    }
                    Logger.WriteData(Logger.WRITER, result.Timestamp, person.Id, skeletonCoordinates);
                }
                Logger.Flush();
            }
        }

        private static void WriteData(StreamWriter writer, long timestamp, uint personId, IEnumerable<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> coordinates)
        {
            foreach (Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>> coordinateTuple in coordinates)
            {
                TrackerResult.PotentialSkeleton pSkeleton = coordinateTuple.Item1;
                Dictionary<JointType, KinectJoint> joints = coordinateTuple.Item2;
                // Headers
                writer.Write(String.Format("{0}, {1}, {2}, {3}, {4}, ", Logger.CURRENT_STUDY_ID, Logger.CURRENT_KINECT_CONFIGURATION, Logger.CURRENT_USER_SCENARIO, timestamp, personId));
                writer.Write(String.Format("{0}, {1}, {2}, {3}, ", pSkeleton.Id, pSkeleton.Skeleton.Timestamp, pSkeleton.Skeleton.InitialAngle, pSkeleton.Skeleton.InitialDistance));
                writer.Write(String.Format("{0}, {1}, {2}", pSkeleton.FOV.ClientIP.Address, pSkeleton.FOV.Specification.TiltAngle, pSkeleton.FOV.Specification.Height));
                // Joint_X, Joint_Y, Joint_Z
                Logger.WriteJointsData(writer, joints);
            }
        }

        private static void WriteJointsData(StreamWriter writer, Dictionary<JointType, KinectJoint> joints)
        {
            // Joint_X, Joint_Y, Joint_Z
            string prefix = ", ";
            foreach (JointType jt in SkeletonStructure.Joints)
            {
                writer.Write(prefix);
                if (joints.ContainsKey(jt))
                {
                    writer.Write(String.Format("{0}, {1}, {2}", joints[jt].Position.X, joints[jt].Position.Y, joints[jt].Position.Z));
                }
                else
                {
                    writer.Write(String.Format("{0}, {1}, {2}", Logger.NA, Logger.NA, Logger.NA));
                }
            }
            // newline
            writer.WriteLine();
        }

        public static void Flush()
        {
            Logger.WRITER.Flush();
        }

        public static void Close()
        {
            Logger.WRITER.Close();
        }
    }
}
