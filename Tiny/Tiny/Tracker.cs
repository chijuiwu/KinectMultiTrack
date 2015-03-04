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
        public const uint MIN_CALIBRATION_FRAMES = 120;

        private readonly uint KINECT_COUNT;
        private bool systemCalibrated = false;
        private bool systemTracking = false;

        private ConcurrentDictionary<IPEndPoint, KinectClient> kinectClients;
        private const object syncFrameLock = new object();

        public Tracker(uint kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectClients = new ConcurrentDictionary<IPEndPoint, KinectClient>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectClient kinect;
            if (this.kinectClients.TryRemove(clientIP, out kinect))
            {
                kinect.DisposeUI();
            }
        }

        private bool KinectsCalibrationPossible()
        {
            if (this.kinectClients.Count == this.KINECT_COUNT)
            {
                foreach (KinectClient kinect in this.kinectClients.Values)
                {
                    // TODO: Remove HACK!!! Instead, show progress bar
                    if (kinect.UncalibratedFramesCount < Tracker.MIN_CALIBRATION_FRAMES * 3)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void TryCalibration()
        {
            if (this.KinectsCalibrationPossible())
            {
                foreach (KinectClient kinect in this.kinectClients.Values)
                {
                    kinect.Calibrate();
                }
                this.systemCalibrated = true;
            }
        }

        private void UpdateTrackingStatus()
        {
            foreach (KinectClient kinect in this.kinectClients.Values)
            {
                if (!kinect.IsTracking) {
                    this.systemTracking = false;
                    break;
                }
            }
            this.systemTracking = true;
        }

        public TrackerResult SynchronizeTracking(IPEndPoint source, SBodyFrame frame)
        {
            if (!this.kinectClients.ContainsKey(source))
            {
                // Pass in height and tilt angle
                this.kinectClients[source] = new KinectClient(source.ToString(), (uint)this.kinectClients.Count, 0.0, 0.0);
            }
            lock (syncFrameLock)
            {
                if (!this.systemCalibrated)
                {
                    this.TryCalibration();
                    this.UpdateTrackingStatus();
                }
                if (!this.systemCalibrated || !this.systemTracking)
                {
                    return TrackerResult.Empty;
                }
                this.kinectClients[source].ProcessFrames(frame);
                return this.GenerateResult();
            }
        }

        private TrackerResult GenerateResult()
        {
            List<TrackerResult.PotentialSkeleton> currentSkeletonsList = new List<TrackerResult.PotentialSkeleton>();
            foreach (IPEndPoint clientIP in this.kinectClients.Keys)
            {
                KinectClient kinect = this.kinectClients[clientIP];
                TrackerResult.KinectFOV fov = new TrackerResult.KinectFOV(clientIP, kinect.Id, kinect.CameraSpecification);
                foreach (MovingSkeleton skeleton in kinect.CurrentMovingSkeletons) {
                    TrackerResult.PotentialSkeleton potentialSkeleton = new TrackerResult.PotentialSkeleton(fov, skeleton);
                    currentSkeletonsList.Add(potentialSkeleton);
                }
            }

            List<TrackerResult.Person> trackedPeople = new List<TrackerResult.Person>();
            TrackerResult.Person newPerson;
            while ((newPerson = this.DetectSamePersonSkeletons(ref currentSkeletonsList)) != null) {
                // TODO: continue;
            }

            return null;
        }

        private TrackerResult.Person DetectSamePersonSkeletons(ref List<TrackerResult.PotentialSkeleton>)
        {
            return null;
        }

        private IEnumerable<TrackerResult.Person> AssignSkeletonsToPeople(IEnumerable<TrackerResult.KinectFOV> fovs)
        {
            List<TrackerResult.Person> people = new List<TrackerResult.Person>();
            
            // TODO: Validate result
            while(skeletonsSet.Any())
            {
                IEnumerable<Tuple<TrackerResult.KinectFOV, MovingSkeleton>> personSkeletons = this.GroupClosestSkeletons(skeletonsSet);
                HashSet<TrackerResult.PotentialSkeleton> matches = new HashSet<TrackerResult.PotentialSkeleton>();
                foreach (Tuple<TrackerResult.KinectFOV, MovingSkeleton> skeleton in personSkeletons)
                {
                    matches.Add(new TrackerResult.PotentialSkeleton((uint)matches.Count, skeleton.Item1, skeleton.Item2));
                    skeletonsSet.Remove(skeleton);
                }
                people.Add(new TrackerResult.Person((uint)people.Count, matches));
            }
            return people;
        }

        // Match people in other Kinect FOV by their positions in Worldview
        // TODO: Deal with full occlusion
        // TODO: Check if joints are inferred
        private IEnumerable<Tuple<TrackerResult.KinectFOV, MovingSkeleton>> GroupClosestSkeletons(IEnumerable<Tuple<TrackerResult.KinectFOV, MovingSkeleton>> skeletons)
        {
            List<Tuple<TrackerResult.KinectFOV, MovingSkeleton>> personSkeletons = new List<Tuple<TrackerResult.KinectFOV, MovingSkeleton>>() { skeletons.First() };
            // Find matches for the first skeleton
            TrackerResult.KinectFOV targetFOV = skeletons.First().Item1;
            MovingSkeleton targetSkeleton = skeletons.First().Item2;

            Dictionary<TrackerResult.KinectFOV, Tuple<MovingSkeleton, double>> similarSkeletonsDict = new Dictionary<TrackerResult.KinectFOV, Tuple<MovingSkeleton, double>>();
            foreach (Tuple<TrackerResult.KinectFOV, MovingSkeleton> skeleton in skeletons)
            {
                TrackerResult.KinectFOV otherFOV = skeleton.Item1;
                MovingSkeleton otherSkeleton = skeleton.Item2;
                // Look for skeletons in other FOV
                if (!otherFOV.Equals(targetFOV))
                {
                    MovingSkeleton.Position pos0 = targetSkeleton.CurrentPosition;
                    MovingSkeleton.Position pos1 = otherSkeleton.CurrentPosition;
                    double diff = WBody.CalculateDifferences(pos0.Worldview, pos1.Worldview);
                    // Keep track of the smallest difference skeletons in other FOVs
                    if (!similarSkeletonsDict.ContainsKey(otherFOV) || diff < similarSkeletonsDict[otherFOV].Item2)
                    {
                        similarSkeletonsDict[otherFOV] = Tuple.Create(otherSkeleton, diff);
                    }
                }
            }
            foreach (TrackerResult.KinectFOV fov in similarSkeletonsDict.Keys)
            {
                personSkeletons.Add(Tuple.Create(fov, similarSkeletonsDict[fov].Item1));
            }
            return personSkeletons;
        }
    }
}
