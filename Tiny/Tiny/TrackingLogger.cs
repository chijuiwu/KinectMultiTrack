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
        private static readonly string FILE_RAW = "..\\..\\..\\..\\Logs\\raw.csv";
        private static readonly string FILE_DIFFERENCE = "..\\..\\..\\..\\Logs\\difference.csv";
        private static readonly string FILE_AVERAGE = "..\\..\\..\\..\\Logs\\average.csv";

        private static readonly string NA = "N/A";

        private static readonly string TIMESTAMP = "Timestamp";
        private static readonly string PERSON = "Person#";
        private static readonly string SKELETON = "Skeleton#";
        private static readonly string FOV = "FOV#";
        private static readonly string X = "X";
        private static readonly string Y = "Y";
        private static readonly string Z = "Z";

        private List<string> raw;
        private List<string> differences;
        private List<string> averages;

        private readonly object syncWriteLock = new object();

        public TrackingLogger()
        {
            this.raw = TrackingLogger.GetRawHeaders();
            this.differences = TrackingLogger.GetDifferencesHeaders();
            this.averages = TrackingLogger.GetAveragesHeaders();

            TrackingLogger.CreateFile(TrackingLogger.FILE_RAW, this.raw);
            TrackingLogger.CreateFile(TrackingLogger.FILE_DIFFERENCE, this.differences);
            TrackingLogger.CreateFile(TrackingLogger.FILE_AVERAGE, this.averages);
        }

        private static void AddJointHeaders(ref List<string> headers)
        {
            foreach (JointType jt in BodyStructure.Joints)
            {
                string x = jt.ToString() + "_" + TrackingLogger.X;
                string y = jt.ToString() + "_" + TrackingLogger.Y;
                string z = jt.ToString() + "_" + TrackingLogger.Z;
                headers.Add(x);
                headers.Add(y);
                headers.Add(z);
            }
        }

        private static List<string> GetDefaultHeaders()
        {
            List<string> headers = new List<string>();
            headers.Add(TrackingLogger.TIMESTAMP);
            headers.Add(TrackingLogger.PERSON);
            headers.Add(TrackingLogger.SKELETON);
            headers.Add(TrackingLogger.FOV);
            TrackingLogger.AddJointHeaders(ref headers);
            return headers;
        }

        private static List<string> GetRawHeaders()
        {
            return TrackingLogger.GetDefaultHeaders();
        }

        private static List<string> GetDifferencesHeaders()
        {
            return TrackingLogger.GetDefaultHeaders();
        }

        private static List<string> GetAveragesHeaders()
        {
            List<string> averages = new List<string>();
            averages.Add(TrackingLogger.TIMESTAMP);
            averages.Add(TrackingLogger.PERSON);
            TrackingLogger.AddJointHeaders(ref averages);
            return averages;
        }

        private static void WriteHeaders(string filepath, List<string> headers)
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

        private static void CreateFile(string filepath, List<string> headers)
        {
            using (StreamReader r = new StreamReader(File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite)))
            {
                string header = r.ReadLine();
                if (header == null)
                {
                    TrackingLogger.WriteHeaders(filepath, headers);
                }
            }
        }

        public void Write(Tracker.Result result)
        {
            lock (syncWriteLock)
            {   
                foreach (Tracker.Result.Person person in result.People)
                {
                    List<Dictionary<JointType, KinectJoint>> jointsList = new List<Dictionary<JointType, KinectJoint>>();
                    List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> skeletonMatchJoints = new List<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>>();
                        
                    using (StreamWriter rawFile = new StreamWriter(TrackingLogger.FILE_RAW, true))
                    {
                        Tracker.Result.SkeletonMatch reference = TrackingUtils.GetLocalSkeletonReference(person);
                        foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                        {
                            Dictionary<JointType, KinectJoint> joints = TrackingUtils.GetKinectJoints(match, reference.Skeleton);
                            jointsList.Add(joints);
                            skeletonMatchJoints.Add(Tuple.Create(match, joints));
                            
                            // Timestamp, Person#, Skeleton#, FOV#
                            rawFile.Write(result.Timestamp + ", " + person.Id + ", " + match.Id + ", " + match.FOV.Id);
                            // Joint_X, Joint_Y, Joint_Z
                            string prefix = ",";
                            foreach (JointType jt in BodyStructure.Joints)
                            {
                                rawFile.Write(prefix);
                                if (joints.ContainsKey(jt))
                                {
                                    rawFile.Write(joints[jt].Coordinate.X + ", " + joints[jt].Coordinate.Y + ", " + joints[jt].Coordinate.Z);
                                }
                                else
                                {
                                    rawFile.Write(TrackingLogger.NA + ", " + TrackingLogger.NA + ", " + TrackingLogger.NA);
                                }
                            }
                            // newline
                            rawFile.WriteLine();
                        }
                    }
                    // Averages
                    Dictionary<JointType, KinectJoint> averages = TrackingUtils.GetAverages(jointsList);
                    this.WriteAverages(result.Timestamp, person.Id, averages);

                    // Differences
                    IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> differences = TrackingUtils.GetDifferences(averages, skeletonMatchJoints);
                    this.WriteDifferences(result.Timestamp, person.Id, differences);
                }
            }
        }

        private void WriteAverages(long timestamp, uint personId, Dictionary<JointType, KinectJoint> averages)
        {
            using (StreamWriter averageFile = new StreamWriter(TrackingLogger.FILE_AVERAGE, true))
            {
                // Timestamp, Person#
                averageFile.Write(timestamp + ", " + personId);

                // Joint_X, Joint_Y, Joint_Z
                string prefix = ",";
                foreach (JointType jt in BodyStructure.Joints)
                {
                    averageFile.Write(prefix);
                    if (averages.ContainsKey(jt))
                    {
                        averageFile.Write(averages[jt].Coordinate.X + ", " + averages[jt].Coordinate.Y + ", " + averages[jt].Coordinate.Z);
                    }
                    else
                    {
                        averageFile.Write(TrackingLogger.NA + ", " + TrackingLogger.NA + ", " + TrackingLogger.NA);
                    }
                }
                // newline
                averageFile.WriteLine();
            }
        }
        private void WriteDifferences(long timestamp, uint personId, IEnumerable<Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>>> differences)
        {
            using (StreamWriter differenceFile = new StreamWriter(TrackingLogger.FILE_DIFFERENCE, true))
            {
                foreach (Tuple<Tracker.Result.SkeletonMatch, Dictionary<JointType, KinectJoint>> coordinateTuple in differences)
                {
                    Tracker.Result.SkeletonMatch match = coordinateTuple.Item1;
                    Dictionary<JointType, KinectJoint> coordinates = coordinateTuple.Item2;

                    // Timestamp, Person#, Skeleton#, FOV#
                    differenceFile.Write(timestamp + ", " + personId + ", " + match.Id + ", " + match.FOV.Id);

                    // Joint_X, Joint_Y, Joint_Z
                    string prefix = ",";
                    foreach (JointType jt in BodyStructure.Joints)
                    {
                        differenceFile.Write(prefix);
                        if (coordinates.ContainsKey(jt))
                        {
                            differenceFile.Write(coordinates[jt].Coordinate.X + ", " + coordinates[jt].Coordinate.Y + ", " + coordinates[jt].Coordinate.Z);
                        }
                        else
                        {
                            differenceFile.Write(TrackingLogger.NA + ", " + TrackingLogger.NA + ", " + TrackingLogger.NA);
                        }
                    }
                    // newline
                    differenceFile.WriteLine();
                }
            }
        }
    }
}
