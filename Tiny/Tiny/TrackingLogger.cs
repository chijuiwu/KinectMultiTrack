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
    public class TrackingLogger
    {
        private static readonly string NA = "N/A";
        private static readonly string TIMESTAMP = "Timestamp";
        private static readonly string PERSON = "Person#";
        private static readonly string SKELETON = "Skeleton#";
        private static readonly string FOV = "FOV#";
        private static readonly string X = "X";
        private static readonly string Y = "Y";
        private static readonly string Z = "Z";

        private static readonly string FILE_RAW = "..\\..\\..\\..\\Logs\\raw.csv";
        private static readonly string FILE_DIFF = "..\\..\\..\\..\\Logs\\difference.csv";
        private static readonly string FILE_AVG = "..\\..\\..\\..\\Logs\\average.csv";

        private static readonly List<string> HEADERS_RAW = TrackingLogger.GetHeaders();
        private static readonly List<string> HEADERS_DIFF = TrackingLogger.GetHeaders();
        private static readonly List<string> HEADERS_AVG = TrackingLogger.GetHeaders();

        private static readonly StreamWriter WRITER_RAW = TrackingLogger.OpenFileWriter(TrackingLogger.FILE_RAW, TrackingLogger.HEADERS_RAW);
        private static readonly StreamWriter WRITER_DIFF = TrackingLogger.OpenFileWriter(TrackingLogger.FILE_DIFF, TrackingLogger.HEADERS_DIFF);
        private static readonly StreamWriter WRITER_AVG = TrackingLogger.OpenFileWriter(TrackingLogger.FILE_AVG, TrackingLogger.HEADERS_AVG);

        private static readonly object WRITE_LOCK = new object();

        private static List<string> GetHeaders()
        {
            List<string> headers = new List<string>() { TrackingLogger.TIMESTAMP, TrackingLogger.PERSON, TrackingLogger.SKELETON, TrackingLogger.FOV };
            TrackingLogger.AddJointHeaders(ref headers);
            return headers;
        }

        private static void AddJointHeaders(ref List<string> headers)
        {
            foreach (JointType jt in SkeletonStructure.Joints)
            {
                string x = jt.ToString() + "_" + TrackingLogger.X;
                string y = jt.ToString() + "_" + TrackingLogger.Y;
                string z = jt.ToString() + "_" + TrackingLogger.Z;
                headers.Add(x);
                headers.Add(y);
                headers.Add(z);
            }
        }

        private static StreamWriter OpenFileWriter(string filename, List<string> headers)
        {
            TrackingLogger.CreateFile(filename, headers);
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

        public static void Write(Tracker.Result result)
        {
            lock (WRITE_LOCK)
            {   
                foreach (Tracker.Result.Person person in result.People)
                {
                    Tracker.Result.SkeletonMatch reference = TrackingUtils.GetLocalSkeletonReference(person);
                    List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> skeletonCoordinates = new List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>>();
                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        Dictionary<JointType, KinectJoint> joints = TrackingUtils.GetKinectJoints(match, reference.Skeleton);
                        skeletonCoordinates.Add(Tuple.Create(match, joints));
                    }
                    // Raw
                    TrackingLogger.WriteData(TrackingLogger.WRITER_RAW, result.Timestamp, person.Id, skeletonCoordinates);

                    //// Averages
                    //Dictionary<JointType, KinectJoint> averages = TrackingUtils.GetAverages(skeletonCoordinates.Select(t=>t.Item2));
                    //TrackingLogger.WriteData(TrackingLogger.WRITER_AVG, result.Timestamp, person.Id, averages);

                    //// Differences
                    //IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> differences = TrackingUtils.GetDifferences(averages, skeletonCoordinates);
                    //TrackingLogger.WriteData(TrackingLogger.WRITER_DIFF, result.Timestamp, person.Id, skeletonCoordinates);
                }
            }
        }

        private static void WriteData(StreamWriter writer, long timestamp, uint personId, Dictionary<JointType, KinectJoint> averages)
        {
            // Timestamp, Person#, Skeleton#, FOV#
            writer.Write(timestamp + ", " + personId + ", " + TrackingLogger.NA + ", " + TrackingLogger.NA);

            // Joint_X, Joint_Y, Joint_Z
            TrackingLogger.WriteJointsData(writer, averages);
        }

        private static void WriteData(StreamWriter writer, long timestamp, uint personId, IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> coordinates)
        {
            foreach (Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>> coordinateTuple in coordinates)
            {
                Tracker.Result.SkeletonMatch match = coordinateTuple.Item1;
                Dictionary<JointType, KinectJoint> joints = coordinateTuple.Item2;

                // Timestamp, Person#, Skeleton#, FOV#
                writer.Write(timestamp + ", " + personId + ", " + match.Id + ", " + match.FOV.Id);

                // Joint_X, Joint_Y, Joint_Z
                TrackingLogger.WriteJointsData(writer, joints);
            }
        }

        private static void WriteJointsData(StreamWriter writer, Dictionary<JointType, KinectJoint> joints)
        {
            // Joint_X, Joint_Y, Joint_Z
            string prefix = ",";
            foreach (JointType jt in SkeletonStructure.Joints)
            {
                writer.Write(prefix);
                if (joints.ContainsKey(jt))
                {
                    writer.Write(joints[jt].Coordinate.X + ", " + joints[jt].Coordinate.Y + ", " + joints[jt].Coordinate.Z);
                }
                else
                {
                    writer.Write(TrackingLogger.NA + ", " + TrackingLogger.NA + ", " + TrackingLogger.NA);
                }
            }
            // newline
            writer.WriteLine();
        }

        public static void Flush()
        {
            TrackingLogger.WRITER_RAW.Flush();
            TrackingLogger.WRITER_DIFF.Flush();
            TrackingLogger.WRITER_AVG.Flush();
        }

        public static void Close()
        {
            TrackingLogger.WRITER_RAW.Close();
            TrackingLogger.WRITER_DIFF.Close();
            TrackingLogger.WRITER_AVG.Close();
        }
    }
}
