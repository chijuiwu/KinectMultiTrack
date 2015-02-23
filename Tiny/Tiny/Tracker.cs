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
                public IPEndPoint ClientIP { get; private set; }
                public uint Id { get; private set; }
                public KinectCamera.Specification Specification { get; private set; }

                public KinectFOV(IPEndPoint clientIP, uint id, KinectCamera.Specification specification)
                {
                    this.ClientIP = clientIP;
                    this.Id = id;
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
                    if (kinect.UnprocessedFramesCount < Tracker.CALIBRATION_FRAMES*3)
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
                this.kinectsDict[clientIP] = new KinectCamera(clientIP.ToString(), (uint)this.kinectsDict.Count);
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
                bool trackingBegun = true;
                this.kinectsDict[clientIP].ProcessFrames(bodyframe);
                List<Result.KinectFOV> fovs = new List<Result.KinectFOV>();
                foreach (IPEndPoint kinectIP in this.kinectsDict.Keys)
                {
                    KinectCamera kinect = this.kinectsDict[kinectIP];
                    fovs.Add(new Result.KinectFOV(kinectIP, kinect.Id, kinect.CameraSpecification));
                    if (kinect.Skeletons.Count() == 0) {
                        trackingBegun = false;
                    }
                }
                if (!this.calibrated || !trackingBegun)
                {
                    return new Result(fovs, Enumerable.Empty<Result.Person>());
                }
                else
                {
                    return new Result(fovs, this.AssignSkeletonsToPeople(fovs));
                }
            }
        }

        private IEnumerable<Result.Person> AssignSkeletonsToPeople(IEnumerable<Result.KinectFOV> fovs)
        {
            HashSet<Tuple<Result.KinectFOV, TSkeleton>> skeletonsSet = new HashSet<Tuple<Result.KinectFOV, TSkeleton>>();
            foreach (Result.KinectFOV fov in fovs)
            {
                KinectCamera kinect = this.kinectsDict[fov.ClientIP];
                foreach (TSkeleton skeleton in kinect.Skeletons)
                {
                    skeletonsSet.Add(Tuple.Create(fov, skeleton));
                }
            }
            List<Result.Person> people = new List<Result.Person>();
            // TODO: Validate result
            while(skeletonsSet.Any())
            {
                IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> personSkeletons = this.GroupClosestSkeletons(skeletonsSet);
                HashSet<Result.SkeletonReplica> matches = new HashSet<Result.SkeletonReplica>();
                foreach (Tuple<Result.KinectFOV, TSkeleton> skeleton in personSkeletons)
                {
                    matches.Add(new Result.SkeletonReplica((uint)matches.Count, skeleton.Item1, skeleton.Item2));
                    skeletonsSet.Remove(skeleton);
                }
                people.Add(new Result.Person((uint)people.Count, matches));
            }
            return people;
        }

        // Match people in other Kinect FOV by their positions in Worldview
        // TODO: Deal with full occlusion
        // TODO: Check if joints are inferred
        private IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> GroupClosestSkeletons(IEnumerable<Tuple<Result.KinectFOV, TSkeleton>> skeletons)
        {
            List<Tuple<Result.KinectFOV, TSkeleton>> personSkeletons = new List<Tuple<Result.KinectFOV, TSkeleton>>() { skeletons.First() };
            // Find matches for the first skeleton
            Result.KinectFOV targetFOV = skeletons.First().Item1;
            TSkeleton targetSkeleton = skeletons.First().Item2;

            Dictionary<Result.KinectFOV, Tuple<TSkeleton, double>> similarSkeletonsDict = new Dictionary<Result.KinectFOV, Tuple<TSkeleton, double>>();
            foreach (Tuple<Result.KinectFOV, TSkeleton> skeleton in skeletons)
            {
                Result.KinectFOV otherFOV = skeleton.Item1;
                TSkeleton otherSkeleton = skeleton.Item2;
                // Look for skeletons in other FOV
                if (!otherFOV.Equals(targetFOV))
                {
                    TSkeleton.Position pos0 = targetSkeleton.CurrentPosition;
                    TSkeleton.Position pos1 = otherSkeleton.CurrentPosition;
                    double diff = WBody.CalculateDifferences(pos0.Worldview, pos1.Worldview);
                    // Keep track of the smallest difference skeletons in other FOVs
                    if (!similarSkeletonsDict.ContainsKey(otherFOV) || diff < similarSkeletonsDict[otherFOV].Item2)
                    {
                        similarSkeletonsDict[otherFOV] = Tuple.Create(otherSkeleton, diff);
                    }
                }
            }
            foreach (Result.KinectFOV fov in similarSkeletonsDict.Keys)
            {
                personSkeletons.Add(Tuple.Create(fov, similarSkeletonsDict[fov].Item1));
            }
            return personSkeletons;
        }
    }
}
