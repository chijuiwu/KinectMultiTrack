using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectSerializer;
using System.Collections.Concurrent;
using System.Net;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Runtime.CompilerServices;

namespace Tiny
{
    public class Tracker
    {
        public const int CALIBRATION_FRAMES = 120;
        private bool calibrated = false;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectAgent> kinectsDict;

        private readonly object syncFrameLock = new object();

        public class Result
        {
            public class KinectFOV
            {
                public IPEndPoint ClientIP { get; private set; }
                public KinectAgent.Dimension Dimension { get; private set; }
                public IEnumerable<Person> People { get; private set; }

                public KinectFOV(IPEndPoint clientIP, KinectAgent.Dimension dimension, IEnumerable<Person> people)
                {
                    this.ClientIP = clientIP;
                    this.Dimension = dimension;
                    this.People = people;
                }
            }

            public class Match
            {
                public int Count { get; private set; }
                public IEnumerable<Person> People { get; private set; }

                public Match(IEnumerable<Person> people)
                {
                    this.Count = Helper.Count(people);
                    this.People = people;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[Match: ").Append(this.Count).Append("]: ");
                    String prefix = "";
                    foreach (Person p in this.People)
                    {
                        sb.Append(prefix);
                        prefix = ",";
                        sb.Append(p);
                    }
                    return sb.ToString();
                }
            }

            public IEnumerable<Result.KinectFOV> FOVs { get; private set; }
            public IEnumerable<Result.Match> Matches { get; private set; }

            public Result(IEnumerable<Result.KinectFOV> fovs, IEnumerable<Result.Match> matches)
            {
                this.FOVs = fovs;
                this.Matches = matches;
            }
        }

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectsDict = new ConcurrentDictionary<IPEndPoint, KinectAgent>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectAgent kinect;
            if (this.kinectsDict.TryRemove(clientIP, out kinect))
            {
                kinect.DisposeUI();
            }
        }

        private bool RequireCalibration()
        {
            if (this.kinectsDict.Count < this.KINECT_COUNT && this.calibrated)
            {
                return false;
            }
            else
            {
                foreach (KinectAgent kinect in this.kinectsDict.Values)
                {
                    if (kinect.UnprocessedFramesCount < Tracker.CALIBRATION_FRAMES)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public Result Synchronize(IPEndPoint clientIP, SBodyFrame bodyframe)
        {
            if (!this.kinectsDict.ContainsKey(clientIP))
            {
                this.kinectsDict[clientIP] = new KinectAgent();
            }
            lock (syncFrameLock)
            {
                if (this.RequireCalibration())
                {
                    foreach (KinectAgent kinect in this.kinectsDict.Values)
                    {
                        kinect.Calibrate();
                    }
                }
                // Get a copy of the current positions of users
                this.kinectsDict[clientIP].ProcessFrames(bodyframe);
                List<Result.KinectFOV> fovs = new List<Result.KinectFOV>();
                foreach (IPEndPoint kinectId in this.kinectsDict.Keys)
                {
                    KinectAgent.Dimension dimension = this.kinectsDict[kinectId].FrameDimension;
                    IEnumerable<Person> people = this.kinectsDict[kinectId].People;
                    fovs.Add(new Result.KinectFOV(kinectId, dimension, people));
                }
                IEnumerable<Result.Match> matches = this.MatchPeople(fovs);
                return new Result(fovs, matches);
            }
        }

        private IEnumerable<Result.Match> MatchPeople(IEnumerable<Result.KinectFOV> fovs)
        {
            Debug.WriteLine("Matching skeleton...");
            Debug.WriteLine("FOV: " + Helper.Count(fovs));
            List<Result.Match> matches = new List<Result.Match>();
            foreach (Result.KinectFOV fov in fovs)
            {
                Debug.WriteLine("Kinect: " + fov.ClientIP + " People: " + Helper.Count(fov.People));
                foreach (Person person in fov.People)
                {
                    // TODO: Validate result
                    Result.Match personInOtherFOVs = this.MatchPeopleByPosition(fovs, fov, person);
                    Debug.WriteLine(personInOtherFOVs);
                    matches.Add(personInOtherFOVs);
                }
            }
            return matches;
        }

        // Match people in other Kinect FOV by their positions in World View
        private Result.Match MatchPeopleByPosition(IEnumerable<Result.KinectFOV> fovs, Result.KinectFOV targetFOV, Person targetPerson)
        {
            // over all Kinect FOVs
            List<Person> personInOtherFOVs = new List<Person>();
            foreach (Result.KinectFOV fov in fovs)
            {
                if (!fov.Equals(targetFOV))
                {
                    // within one FOV
                    Person samePerson = null;
                    double minDist = Double.MaxValue;
                    foreach (Person p in fov.People)
                    {
                        if (!p.Equals(targetPerson))
                        {
                            WBody body0 = p.CurrentPosition.Worldview;
                            WBody body1 = targetPerson.CurrentPosition.Worldview;
                            double diff = WBody.GetCoordinateDifferences(body0, body1);
                            if (diff < minDist)
                            {
                                samePerson = p;
                                minDist = diff;
                            }
                        }
                    }
                    if (samePerson != null)
                    {
                        personInOtherFOVs.Add(samePerson);
                    }
                }
            }
            return new Result.Match(personInOtherFOVs);
        }
    }
}
