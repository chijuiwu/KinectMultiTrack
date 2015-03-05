using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Tiny.WorldView;
using KinectSerializer;
using System.Diagnostics;
using System.Collections.Specialized;
using System.IO;

namespace Tiny
{
    public class Logger
    {
        private static readonly string NA = "N/A";
        private static readonly string SCENARIO = "Scenario";
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

        private static string scenario = Logger.NA;

        private static readonly string FILE_RAW = "..\\..\\..\\..\\Logs\\raw.csv";
        //private static readonly string FILE_DIFF = "..\\..\\..\\..\\Logs\\difference.csv";
        //private static readonly string FILE_AVG = "..\\..\\..\\..\\Logs\\average.csv";

        private static readonly List<string> HEADERS_RAW = Logger.GetHeaders();
        //private static readonly List<string> HEADERS_DIFF = TrackingLogger.GetHeaders();
        //private static readonly List<string> HEADERS_AVG = TrackingLogger.GetHeaders();

        private static readonly StreamWriter WRITER_RAW = Logger.OpenFileWriter(Logger.FILE_RAW, Logger.HEADERS_RAW);
        //private static readonly StreamWriter WRITER_DIFF = TrackingLogger.OpenFileWriter(TrackingLogger.FILE_DIFF, TrackingLogger.HEADERS_DIFF);
        //private static readonly StreamWriter WRITER_AVG = TrackingLogger.OpenFileWriter(TrackingLogger.FILE_AVG, TrackingLogger.HEADERS_AVG);

        private static readonly object WRITE_LOCK = new object();

        private static List<string> GetHeaders()
        {
            List<string> headers = new List<string>() { 
                Logger.SCENARIO,
                Logger.TRACKER_TIME, 
                Logger.PERSON,
                Logger.SKEL,
                Logger.SKEL_INIT_ANGLE,
                Logger.SKEL_INIT_DIST,
                Logger.SKEL_TIME,
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

        private static StreamWriter OpenFileWriter(string filename, List<string> headers)
        {
            Logger.CreateFile(filename, headers);
            return new StreamWriter(filename, true);
        }

        private static void CreateFile(string filepath, List<string> headers)
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

        public static void SetScenarioType(string scenario)
        {
            Logger.scenario = scenario;
        }

        public static void Write(TrackerResult result)
        {
            lock (WRITE_LOCK)
            {   
                foreach (TrackerResult.Person person in result.People)
                {
                    TrackerResult.PotentialSkeleton reference = TrackerResult.GetLocalSkeletonReference(person);
                    List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> skeletonCoordinates = new List<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>>();
                    foreach (TrackerResult.PotentialSkeleton skeleton in person.Skeletons)
                    {
                        KinectBody body = WBody.GetKinectBody(skeleton, reference.Skeleton);
                        skeletonCoordinates.Add(Tuple.Create(skeleton, body.Joints));
                    }
                    // Raw
                    Logger.WriteData(Logger.WRITER_RAW, result.Timestamp, person.Id, skeletonCoordinates);

                    //// Averages
                    //Dictionary<JointType, KinectJoint> averages = TrackingUtils.GetAverages(skeletonCoordinates.Select(t=>t.Item2));
                    //TrackingLogger.WriteData(TrackingLogger.WRITER_AVG, result.Timestamp, person.Id, averages);

                    //// Differences
                    //IEnumerable<Tuple<TResult.SkeletonMatch, Dictionary<JointType, KinectJoint>>> differences = TrackingUtils.GetDifferences(averages, skeletonCoordinates);
                    //TrackingLogger.WriteData(TrackingLogger.WRITER_DIFF, result.Timestamp, person.Id, skeletonCoordinates);
                }
            }
        }

        //private static void WriteData(StreamWriter writer, long timestamp, uint personId, Dictionary<JointType, KinectJoint> averages)
        //{
        //    // Timestamp_Tracker, Person#, Skeleton#, FOV#, Timestamp_Skeleton
        //    writer.Write(timestamp + ", " + personId + ", " + TLogger.NA + ", " + TLogger.NA + ", " + TLogger.NA);

        //    // Joint_X, Joint_Y, Joint_Z
        //    TLogger.WriteJointsData(writer, averages);
        //}

        private static void WriteData(StreamWriter writer, long timestamp, uint personId, IEnumerable<Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>>> coordinates)
        {
            foreach (Tuple<TrackerResult.PotentialSkeleton, Dictionary<JointType, KinectJoint>> coordinateTuple in coordinates)
            {
                TrackerResult.PotentialSkeleton replica = coordinateTuple.Item1;
                Dictionary<JointType, KinectJoint> joints = coordinateTuple.Item2;
                // Headers
                writer.Write(String.Format("{0}, {1}, {2}, ", Logger.scenario, timestamp, personId));
                writer.Write(String.Format("{0}, {1}, {2}, {3}, ", replica.Id, replica.Skeleton.InitialAngle, replica.Skeleton.InitialDistance, replica.Skeleton.Timestamp));
                writer.Write(String.Format("{0}, {1}, {2}", replica.FOV.Id, replica.FOV.Specification.TiltAngle, replica.FOV.Specification.Height));
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
            Logger.WRITER_RAW.Flush();
            //TrackingLogger.WRITER_DIFF.Flush();
            //TrackingLogger.WRITER_AVG.Flush();
        }

        public static void Close()
        {
            Logger.WRITER_RAW.Close();
            //TrackingLogger.WRITER_DIFF.Close();
            //TrackingLogger.WRITER_AVG.Close();
        }
    }
}
