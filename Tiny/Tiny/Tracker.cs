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
        // 4 Seconds
        public const int CALIBRATION_FRAMES = 120;
        private bool calibrated = false;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectCamera> kinectsDict;

        private readonly object syncFrameLock = new object();

        public class Result
        {
            public class KinectFOV
            {
                public uint Id { get; private set; }
                public IPEndPoint ClientIP { get; private set; }
                public KinectCamera.Specification Specification { get; private set; }

                public KinectFOV(uint id, IPEndPoint clientIP, KinectCamera.Specification specification)
                {
                    this.Id = id;
                    this.ClientIP = clientIP;
                    this.Specification = specification;
                }
            }

            public class SkeletonReplica
            {
                public uint Id { get; private set; }
                public KinectFOV FOV { get; private set; }
                public TSkeleton Skeleton { get; private set; }

                public SkeletonReplica(uint id, KinectFOV fov, TSkeleton skeleton)
                {
                    this.Id = id;
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
                public uint Id { get; private set; }
                public IEnumerable<SkeletonReplica> Replicas { get; private set; }

                public Person(uint id, IEnumerable<SkeletonReplica> skeletons)
                {
                    this.Id = id;
                    this.Replicas = skeletons;
                }

                public TSkeleton FindSkeletonInFOV(KinectFOV fov)
                {
                    foreach (SkeletonReplica match in this.Replicas)
                    {
                        if (match.FOV.Equals(fov))
                        {
                            return match.Skeleton;
                        }
                    }
                    return null;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[Skeletons: ").Append(this.Replicas.Count()).Append("]: ");
                    String prefix = "";
                    foreach (SkeletonReplica match in this.Replicas)
                    {
                        sb.Append(prefix);
                        prefix = ",";
                        sb.Append(match);
                    }
                    return sb.ToString();
                }
            }

            public long Timestamp { get; private set; }
            public IEnumerable<Result.KinectFOV> FOVs { get; private set; }
            public IEnumerable<Result.Person> People { get; private set; }

            public Result(IEnumerable<Result.KinectFOV> fovs, IEnumerable<Result.Person> people)
            {
                this.Timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
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
            if (this.kinectsDict.Count == this.KINECT_COUNT && !this.calibrated)
            {
                foreach (KinectCamera kinect in this.kinectsDict.Values)
                {
                    // TODO: Remove HACK!!! so i have time to adjust position
                    if (kinect.UnprocessedFramesCount < Tracker.CALIBRATION_FRAMES*2)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Result Synchronize(IPEndPoint clientIP, SBodyFrame bodyframe)
        {
            if (!this.kinectsDict.ContainsKey(clientIP))
            {
                this.kinectsDict[clientIP] = new KinectCamera(clientIP.ToString());
            }
            lock (syncFrameLock)
            {
                if (this.RequireCalibration())
                {
                    foreach (KinectCamera kinect in this.kinectsDict.Values)
                    {
                        kinect.Calibrate();
                    }
                    this.calibrated = true;
                }
                this.kinectsDict[clientIP].ProcessFrames(bodyframe);
                List<Result.KinectFOV> fovs = new List<Result.KinectFOV>();
                foreach (IPEndPoint kinectIP in this.kinectsDict.Keys)
                {
                    fovs.Add(new Result.KinectFOV((uint)fovs.Count, kinectIP, this.kinectsDict[kinectIP].CameraSpecification));
                }
                if (!this.calibrated)
                {
                    return new Result(fovs, Enumerable.Empty<Result.Person>());
                }
                else
                {
                    return new Result(fovs, this.AssignSkeletonsToPeople(fovs));
                }
            }
        }

        // TODO: Check if joints are inferred
        private IEnumerable<Result.Person> AssignSkeletonsToPeople(IEnumerable<Result.KinectFOV> fovs)
        {
            //Debug.WriteLine("Matching skeleton...");
            //Debug.WriteLine("FOV: " + fovs.Count());
            List<Tuple<Result.KinectFOV, TSkeleton>> skeletonsList = new List<Tuple<Result.KinectFOV, TSkeleton>>();
            foreach (Result.KinectFOV fov in fovs)
            {
                KinectCamera kinect = this.kinectsDict[fov.ClientIP];
                //Debug.WriteLine("Kinect: " + fov.ClientIP + " People: " + kinect.Skeletons.Count());
                foreach (TSkeleton skeleton in kinect.Skeletons)
                {
                    skeletonsList.Add(Tuple.Create(fov, skeleton));
                }
            }
            //Debug.WriteLine("Total Skeletons: " + skeletonsList.Count);

            List<Result.Person> people = new List<Result.Person>();
            // TODO: Validate result
            // TODO: Deal with occlusion (One person may not appear in all FOVs)
            while(skeletonsList.Any())
            {
                IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> personSkeletons = this.GroupSkeletons(skeletonsList);
                //Debug.WriteLine("Matches Found: " + personSkeletons.Count());
                List<Result.SkeletonReplica> matches = new List<Result.SkeletonReplica>();
                foreach (Tuple<Result.KinectFOV, TSkeleton> skeleton in personSkeletons)
                {
                    //Debug.WriteLine("Match: [FOV: " + skeleton.Item1.ClientIP + ", Skeleton: " + skeleton.Item2 + "]");
                    matches.Add(new Result.SkeletonReplica((uint)matches.Count, skeleton.Item1, skeleton.Item2));
                    skeletonsList.Remove(skeleton);
                }
                Result.Person person = new Result.Person((uint)people.Count, matches);
                people.Add(person);
            }
            //Debug.WriteLine("Total People: " + people.Count);
            return people;
        }

        // Match people in other Kinect FOV by their positions in World View
        private IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> GroupSkeletons(IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> skeletons)
        {
            // Find matches for the first skeleton
            Result.KinectFOV targetFOV = skeletons.First().Item1;
            TSkeleton targetSkeleton = skeletons.First().Item2;
            List<Tuple<Result.KinectFOV, TSkeleton>> personSkeletons = new List<Tuple<Result.KinectFOV, TSkeleton>>() { skeletons.First() };

            Dictionary<Result.KinectFOV, Tuple<TSkeleton, double>> mostSimilarSkeletons = new Dictionary<Result.KinectFOV, Tuple<TSkeleton, double>>();
            foreach (Tuple<Result.KinectFOV, TSkeleton> skeleton in skeletons)
            {
                Result.KinectFOV otherFOV = skeleton.Item1;
                TSkeleton otherSkeleton = skeleton.Item2;
                if (!otherFOV.Equals(targetFOV) && !otherSkeleton.Equals(targetSkeleton))
                {
                    TSkeleton.Position pos0 = targetSkeleton.CurrentPosition;
                    TSkeleton.Position pos1 = otherSkeleton.CurrentPosition;
                    // HACK
                    if (pos0 == null || pos1 == null)
                    {
                        mostSimilarSkeletons[otherFOV] = Tuple.Create(otherSkeleton, Double.MinValue);
                        continue;
                    }
                    double diff = WBody.CalculateDifferences(pos0.Worldview, pos1.Worldview);
                    if ((mostSimilarSkeletons.ContainsKey(otherFOV) && diff < mostSimilarSkeletons[otherFOV].Item2) || !mostSimilarSkeletons.ContainsKey(otherFOV))
                    {
                        mostSimilarSkeletons[otherFOV] = Tuple.Create(otherSkeleton, diff);
                    }
                }
            }
            foreach (Result.KinectFOV fov in mostSimilarSkeletons.Keys)
            {
                personSkeletons.Add(Tuple.Create(fov, mostSimilarSkeletons[fov].Item1));
            }
            return personSkeletons;
        }
    }
}
