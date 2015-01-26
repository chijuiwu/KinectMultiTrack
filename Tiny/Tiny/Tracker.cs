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
using Tiny.WorldView;

namespace Tiny
{
    public class Tracker
    {
        public const int CALIBRATION_FRAMES = 120;
        private bool calibrated = false;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectCamera> kinectsDict;

        private readonly object syncFrameLock = new object();

        public class Result
        {
            public class KinectFOV
            {
                public IPEndPoint ClientIP { get; private set; }
                public KinectCamera.Dimension Dimension { get; private set; }

                public KinectFOV(IPEndPoint clientIP, KinectCamera.Dimension dimension)
                {
                    this.ClientIP = clientIP;
                    this.Dimension = dimension;
                }
            }

            public class SkeletonMatch
            {
                public KinectFOV FOV { get; private set; }
                public TrackingSkeleton Skeleton { get; private set; }

                public SkeletonMatch(KinectFOV fov, TrackingSkeleton skeleton)
                {
                    this.FOV = fov;
                    this.Skeleton = skeleton;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[");
                    sb.Append("FOV: ").Append(this.FOV.ClientIP).Append(", ");
                    sb.Append("Skeleton: ").Append(this.Skeleton);
                    sb.Append("]");
                    return sb.ToString();
                }
            }

            public class Person
            {
                public IEnumerable<SkeletonMatch> SkeletonMatches { get; private set; }

                public Person(IEnumerable<SkeletonMatch> skeletons)
                {
                    this.SkeletonMatches = skeletons;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[Skeletons: ").Append(this.SkeletonMatches.Count()).Append("]: ");
                    String prefix = "";
                    foreach (SkeletonMatch match in this.SkeletonMatches)
                    {
                        sb.Append(prefix);
                        prefix = ",";
                        sb.Append(match);
                    }
                    return sb.ToString();
                }
            }

            public IEnumerable<Result.KinectFOV> FOVs { get; private set; }
            public IEnumerable<Result.Person> People { get; private set; }

            public Result(IEnumerable<Result.KinectFOV> fovs, IEnumerable<Result.Person> people)
            {
                this.FOVs = fovs;
                this.People = people;
            }
        }

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectsDict = new ConcurrentDictionary<IPEndPoint, KinectCamera>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectCamera kinect;
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
                foreach (KinectCamera kinect in this.kinectsDict.Values)
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
                this.kinectsDict[clientIP] = new KinectCamera();
            }
            lock (syncFrameLock)
            {
                if (this.RequireCalibration())
                {
                    foreach (KinectCamera kinect in this.kinectsDict.Values)
                    {
                        kinect.Calibrate();
                    }
                }
                // Get a copy of the current positions of users
                this.kinectsDict[clientIP].ProcessFrames(bodyframe);
                List<Result.KinectFOV> fovs = new List<Result.KinectFOV>();
                foreach (IPEndPoint kinectId in this.kinectsDict.Keys)
                {
                    KinectCamera.Dimension dimension = this.kinectsDict[kinectId].FrameDimension;
                    fovs.Add(new Result.KinectFOV(kinectId, dimension));
                }
                IEnumerable<Result.Person> people = this.MatchSkeletonsAndPeople(fovs);
                return new Result(fovs, people);
            }
        }

        private IEnumerable<Result.Person> MatchSkeletonsAndPeople(IEnumerable<Result.KinectFOV> fovs)
        {
            Debug.WriteLine("Matching skeleton...");
            Debug.WriteLine("FOV: " + fovs.Count());
            List<Result.Person> personList = new List<Result.Person>();
            foreach (Result.KinectFOV fov in fovs)
            {
                KinectCamera kinect = this.kinectsDict[fov.ClientIP];
                Debug.WriteLine("Kinect: " + fov.ClientIP + " People: " + kinect.Skeletons.Count());
                foreach (TrackingSkeleton skeleton in kinect.Skeletons)
                {
                    // TODO: Validate result
                    // TODO: Deal with occlusion (One person may not appear in all FOVs)
                    Result.Person personInAllFOVs = this.FindPersonByPosition(fovs, fov, skeleton);
                    Debug.WriteLine(personInAllFOVs);
                    personList.Add(personInAllFOVs);
                }
            }
            return personList;
        }

        // Match people in other Kinect FOV by their positions in World View
        private Result.Person FindPersonByPosition(IEnumerable<Result.KinectFOV> fovs, Result.KinectFOV targetFOV, TrackingSkeleton targetSkeleton)
        {
            // over all Kinect FOVs
            List<Result.SkeletonMatch> personInAllFOVs = new List<Result.SkeletonMatch>() { new Result.SkeletonMatch(targetFOV, targetSkeleton) };
            foreach (Result.KinectFOV fov in fovs)
            {
                if (!fov.Equals(targetFOV))
                {
                    // within one FOV
                    TrackingSkeleton skeletonSamePerson = null;
                    double minDist = Double.MaxValue;

                    KinectCamera kinect = this.kinectsDict[fov.ClientIP];
                    foreach (TrackingSkeleton skeleton in kinect.Skeletons)
                    {
                        if (!skeleton.Equals(targetSkeleton))
                        {
                            WBody body0 = skeleton.CurrentPosition.Worldview;
                            WBody body1 = targetSkeleton.CurrentPosition.Worldview;
                            double diff = WBody.CalculateDifferences(body0, body1);
                            if (diff < minDist)
                            {
                                skeletonSamePerson = skeleton;
                                minDist = diff;
                            }
                        }
                    }
                    if (skeletonSamePerson != null)
                    {
                        personInAllFOVs.Add(new Result.SkeletonMatch(fov, skeletonSamePerson));
                    }
                }
            }
            return new Result.Person(personInAllFOVs);
        }
    }
}
