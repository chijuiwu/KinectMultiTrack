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
    public class TrackerUtils
    {
        public class Logger
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

            public Logger()
            {
                this.raw = Logger.GetRawHeaders();
                this.differences = Logger.GetDifferencesHeaders();
                this.averages = Logger.GetAveragesHeaders();

                Logger.CreateFile(Logger.FILE_RAW, this.raw);
                Logger.CreateFile(Logger.FILE_DIFFERENCE, this.differences);
                Logger.CreateFile(Logger.FILE_AVERAGE, this.averages);
            }

            private static void AddJointHeaders(ref List<string> headers)
            {
                foreach (JointType jt in BodyStructure.Joints)
                {
                    string x = jt.ToString() + "_" + Logger.X;
                    string y = jt.ToString() + "_" + Logger.Y;
                    string z = jt.ToString() + "_" + Logger.Z;
                    headers.Add(x);
                    headers.Add(y);
                    headers.Add(z);
                }
            }

            private static List<string> GetDefaultHeaders()
            {
                List<string> headers = new List<string>();
                headers.Add(Logger.TIMESTAMP);
                headers.Add(Logger.PERSON);
                headers.Add(Logger.SKELETON);
                headers.Add(Logger.FOV);
                Logger.AddJointHeaders(ref headers);
                return headers;
            }

            private static List<string> GetRawHeaders()
            {
                return Logger.GetDefaultHeaders();
            }

            private static List<string> GetDifferencesHeaders()
            {
                return Logger.GetDefaultHeaders();
            }

            private static List<string> GetAveragesHeaders()
            {
                List<string> averages = new List<string>();
                averages.Add(Logger.TIMESTAMP);
                averages.Add(Logger.PERSON);
                Logger.AddJointHeaders(ref averages);
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
                        Logger.WriteHeaders(filepath, headers);
                    }
                }
            }

            public void Write(Tracker.Result result)
            {
                lock (syncWriteLock)
                {
                    using (StreamWriter rawData = new StreamWriter(Logger.FILE_RAW, true))
                    {
                        foreach (Tracker.Result.Person person in result.People)
                        {
                            Dictionary<JointType, CameraSpacePoint> sumJoints = new Dictionary<JointType, CameraSpacePoint>();
                            Dictionary<JointType, int> jointsCount = new Dictionary<JointType, int>();

                            Tracker.Result.SkeletonMatch reference = person.SkeletonMatches.First();
                            Tracker.Result.KinectFOV referenceFOV = reference.FOV;
                            TrackingSkeleton referenceSkeleton = reference.Skeleton;

                            foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                            {
                                // Timestamp, Person#, Skeleton#, FOV#
                                rawData.Write(result.Timestamp + ", " + person.Id + ", " + match.Id + ", " + match.FOV.Id);

                                TrackingSkeleton matchSkeleton = match.Skeleton;
                                WBody position = matchSkeleton.CurrentPosition.Worldview;
                                KinectBody body = WBody.TransformToKinectBody(position, referenceSkeleton.InitialAngle, referenceSkeleton.InitialPosition);

                                string prefix = ", ";
                                foreach (JointType jt in BodyStructure.Joints)
                                {
                                    rawData.Write(prefix);
                                    if (body.Joints.ContainsKey(jt))
                                    {
                                        // Joint_X, Joint_Y, Joint_Z
                                        rawData.Write(body.Joints[jt].X + ", " + body.Joints[jt].Y + ", " + body.Joints[jt].Z);
                                        if (sumJoints.ContainsKey(jt))
                                        {
                                            CameraSpacePoint accumulatePt = new CameraSpacePoint();
                                            accumulatePt.X = sumJoints[jt].X + body.Joints[jt].X;
                                            accumulatePt.Y = sumJoints[jt].Y + body.Joints[jt].Y;
                                            accumulatePt.Z = sumJoints[jt].Z + body.Joints[jt].Z;
                                            sumJoints[jt] = accumulatePt;
                                            jointsCount[jt] += 1;
                                        }
                                        else
                                        {
                                            sumJoints[jt] = body.Joints[jt];
                                            jointsCount[jt] = 1;
                                        }
                                    }
                                    else
                                    {
                                        rawData.Write(Logger.NA + ", " + Logger.NA + ", " + Logger.NA);
                                    }
                                }
                                rawData.WriteLine();
                            } // Skeleton Match

                            // Averages
                            Dictionary<JointType, CameraSpacePoint> averages = new Dictionary<JointType, CameraSpacePoint>();
                            foreach (JointType jt in sumJoints.Keys)
                            {
                                CameraSpacePoint averageJoint = new CameraSpacePoint();
                                averageJoint.X = sumJoints[jt].X / jointsCount[jt];
                                averageJoint.Y = sumJoints[jt].Y / jointsCount[jt];
                                averageJoint.Z = sumJoints[jt].Z / jointsCount[jt];
                                averages[jt] = averageJoint;
                            }
                            this.WriteAverages(averages, result.Timestamp, person.Id);
                            this.WriteDifferences(averages, result.Timestamp, person);
                        } // Person
                    } // Raw Data
                }
            }

            private void WriteAverages(Dictionary<JointType, CameraSpacePoint> averages, long timestamp, uint personId)
            {
                using (StreamWriter averageData = new StreamWriter(Logger.FILE_AVERAGE, true))
                {
                    // Timestamp, Person#
                    averageData.Write(timestamp + ", " + personId);

                    string prefix = ", ";
                    foreach (JointType jt in BodyStructure.Joints)
                    {
                        if (averages[jt].X != 0 && averages[jt].Y != 0 && averages[jt].Z != 0)
                        {
                            averageData.Write(prefix);
                            CameraSpacePoint joint = averages[jt];
                            // Joint_X, Joint_Y, Joint_Z
                            averageData.Write(joint.X + ", " + joint.Y + ", " + joint.Z);
                        }
                        else
                        {
                            averageData.Write(Logger.NA + ", " + Logger.NA + ", " + Logger.NA);
                        }
                    }
                    averageData.WriteLine();
                }
            }
            private void WriteDifferences(Dictionary<JointType, CameraSpacePoint> averages, long timestamp, Tracker.Result.Person person)
            {
                using (StreamWriter differenceData = new StreamWriter(Logger.FILE_DIFFERENCE, true))
                {
                    Tracker.Result.SkeletonMatch reference = person.SkeletonMatches.First();
                    Tracker.Result.KinectFOV referenceFOV = reference.FOV;
                    TrackingSkeleton referenceSkeleton = reference.Skeleton;
                    double referenceAngle = referenceSkeleton.InitialAngle;
                    WCoordinate referencePosition = referenceSkeleton.InitialPosition;

                    foreach (Tracker.Result.SkeletonMatch match in person.SkeletonMatches)
                    {
                        // Timestamp, Person#, Skeleton#, FOV#
                        differenceData.Write(timestamp + ", " + person.Id + ", " + match.Id + ", " + match.FOV.Id);

                        TrackingSkeleton matchSkeleton = match.Skeleton;
                        WBody position = matchSkeleton.CurrentPosition.Worldview;
                        KinectBody body = WBody.TransformToKinectBody(position, referenceAngle, referencePosition);

                        string prefix = ", ";
                        foreach (JointType jt in BodyStructure.Joints)
                        {
                            differenceData.Write(prefix);
                            if (body.Joints.ContainsKey(jt))
                            {
                                CameraSpacePoint joint = body.Joints[jt];
                                CameraSpacePoint average = averages[jt];
                                CameraSpacePoint difference = new CameraSpacePoint();
                                // Square difference
                                difference.X = (float)Math.Pow(joint.X - average.X, 2);
                                difference.Y = (float)Math.Pow(joint.Y - average.Y, 2);
                                difference.Z = (float)Math.Pow(joint.Z - average.Z, 2);
                                // Joint_X, Joint_Y, Joint_Z
                                differenceData.Write(difference.X + ", " + difference.Y + ", " + difference.Z);
                            }
                            else
                            {
                                differenceData.Write(Logger.NA + ", " + Logger.NA + ", " + Logger.NA);
                            }
                        }
                        differenceData.WriteLine();
                    } // Skeleton Match
                }
            }
        }
    }
}
