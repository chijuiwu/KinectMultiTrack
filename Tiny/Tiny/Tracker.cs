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
        private bool allKinectCamerasCalibrated = false;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectCamera> kinectClients;

        private readonly object syncFrameLock = new object();

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectClients = new ConcurrentDictionary<IPEndPoint, KinectCamera>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectCamera kinect;
            if (this.kinectClients.TryRemove(clientIP, out kinect))
            {
                kinect.DisposeUI();
            }
        }

        private bool RequireCalibration()
        {
            bool required = true;
            if (this.kinectClients.Count == this.KINECT_COUNT && !this.allKinectCamerasCalibrated)
            {
                foreach (KinectCamera kinect in this.kinectClients.Values)
                {
                    // TODO: Remove HACK!!! so i have time to adjust position
                    if (kinect.UnprocessedFramesCount < Tracker.CALIBRATION_FRAMES * 3)
                    {
                        required = false;
                        break;
                    }
                }
            }
            return required;
        }

        public TResult SynchronizeTracking(IPEndPoint clientIP, SBodyFrame bodyframe)
        {
            if (!this.kinectClients.ContainsKey(clientIP))
            {
                this.kinectClients[clientIP] = new KinectCamera(clientIP.ToString(), (uint)this.kinectClients.Count);
            }
            lock (syncFrameLock)
            {
                if (!this.allKinectCamerasCalibrated && this.RequireCalibration())
                {
                    foreach (KinectCamera kinect in this.kinectClients.Values)
                    {
                        kinect.Calibrate();
                    }
                    this.allKinectCamerasCalibrated = true;
                }
                bool trackingBegun = true;
                this.kinectClients[clientIP].ProcessFrames(bodyframe);
                List<TResult.KinectFOV> fovs = new List<TResult.KinectFOV>();
                foreach (IPEndPoint kinectIP in this.kinectClients.Keys)
                {
                    KinectCamera kinect = this.kinectClients[kinectIP];
                    fovs.Add(new TResult.KinectFOV(kinectIP, kinect.Id, kinect.CameraSpecification));
                    if (kinect.Skeletons.Count() == 0) {
                        trackingBegun = false;
                    }
                }
                if (!this.allKinectCamerasCalibrated || !trackingBegun)
                {
                    return new TResult(fovs, Enumerable.Empty<TResult.Person>());
                }
                else
                {
                    return new TResult(fovs, this.AssignSkeletonsToPeople(fovs));
                }
            }
        }

        private IEnumerable<TResult.Person> AssignSkeletonsToPeople(IEnumerable<TResult.KinectFOV> fovs)
        {
            HashSet<Tuple<TResult.KinectFOV, TSkeleton>> skeletonsSet = new HashSet<Tuple<TResult.KinectFOV, TSkeleton>>();
            foreach (TResult.KinectFOV fov in fovs)
            {
                KinectCamera kinect = this.kinectClients[fov.ClientIP];
                foreach (TSkeleton skeleton in kinect.Skeletons)
                {
                    skeletonsSet.Add(Tuple.Create(fov, skeleton));
                }
            }
            List<TResult.Person> people = new List<TResult.Person>();
            // TODO: Validate result
            while(skeletonsSet.Any())
            {
                IEnumerable<Tuple<TResult.KinectFOV, TSkeleton>> personSkeletons = this.GroupClosestSkeletons(skeletonsSet);
                HashSet<TResult.SkeletonReplica> matches = new HashSet<TResult.SkeletonReplica>();
                foreach (Tuple<TResult.KinectFOV, TSkeleton> skeleton in personSkeletons)
                {
                    matches.Add(new TResult.SkeletonReplica((uint)matches.Count, skeleton.Item1, skeleton.Item2));
                    skeletonsSet.Remove(skeleton);
                }
                people.Add(new TResult.Person((uint)people.Count, matches));
            }
            return people;
        }

        // Match people in other Kinect FOV by their positions in Worldview
        // TODO: Deal with full occlusion
        // TODO: Check if joints are inferred
        private IEnumerable<Tuple<TResult.KinectFOV, TSkeleton>> GroupClosestSkeletons(IEnumerable<Tuple<TResult.KinectFOV, TSkeleton>> skeletons)
        {
            List<Tuple<TResult.KinectFOV, TSkeleton>> personSkeletons = new List<Tuple<TResult.KinectFOV, TSkeleton>>() { skeletons.First() };
            // Find matches for the first skeleton
            TResult.KinectFOV targetFOV = skeletons.First().Item1;
            TSkeleton targetSkeleton = skeletons.First().Item2;

            Dictionary<TResult.KinectFOV, Tuple<TSkeleton, double>> similarSkeletonsDict = new Dictionary<TResult.KinectFOV, Tuple<TSkeleton, double>>();
            foreach (Tuple<TResult.KinectFOV, TSkeleton> skeleton in skeletons)
            {
                TResult.KinectFOV otherFOV = skeleton.Item1;
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
            foreach (TResult.KinectFOV fov in similarSkeletonsDict.Keys)
            {
                personSkeletons.Add(Tuple.Create(fov, similarSkeletonsDict[fov].Item1));
            }
            return personSkeletons;
        }
    }
}
