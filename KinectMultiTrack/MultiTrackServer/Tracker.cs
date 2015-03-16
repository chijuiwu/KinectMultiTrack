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
using KinectMultiTrack.WorldView;

namespace KinectMultiTrack
{
    public class Tracker
    {
        // TODO: allow dynamically adding new kinects
        private int expectedKinectsCount;
        // 4 Seconds
        public const int MIN_CALIBRATION_FRAMES = 120;
        public const int MIN_CALIBRATION_FRAMES_STORED = MIN_CALIBRATION_FRAMES;
        private readonly object syncTrackLock = new object();
        private bool systemCalibrated;
        private bool recentlyRecalibrated;

        private readonly ConcurrentDictionary<IPEndPoint, KinectClient> kinectClients;

        private TrackerResult currentResult;
        private int initialPeopleTracked;

        public event TrackerKinectHandler OnWaitingKinects;
        public delegate void TrackerKinectHandler(int kinects);
        public event TrackerCalibrationHandler OnCalibration;
        public delegate void TrackerCalibrationHandler(int framesRemaining);
        public event TrackerRecalibrationHandler OnRecalibration;
        public delegate void TrackerRecalibrationHandler(string msg);
        public event TrackerResultHandler OnResult;
        public delegate void TrackerResultHandler(TrackerResult result);

        public Tracker()
        {
            this.systemCalibrated = false;
            this.kinectClients = new ConcurrentDictionary<IPEndPoint, KinectClient>();
            this.currentResult = TrackerResult.Empty;
        }

        public void Configure(int kinectCount)
        {
            this.expectedKinectsCount = kinectCount;
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectClient kinect;
            this.kinectClients.TryRemove(clientIP, out kinect);
        }

        private int GetCalibrationFramesRemaining()
        {
            int mostFramesRequired = Int32.MinValue;
            foreach (KinectClient kinect in this.kinectClients.Values)
            {
                int kinectRemainingFrames = Tracker.MIN_CALIBRATION_FRAMES_STORED - kinect.CalibrationFrames.Count;
                if (kinectRemainingFrames > mostFramesRequired)
                {
                    mostFramesRequired = kinectRemainingFrames;
                }
            }
            return mostFramesRequired == Int32.MinValue ? 0 : mostFramesRequired;
        }

        private int GetKinectsRemaining()
        {
            return this.expectedKinectsCount - this.kinectClients.Count;
        }

        private bool KinectsMeetCalibrationRequirement()
        {
            foreach (KinectClient kinect in this.kinectClients.Values)
            {
                if (kinect.CalibrationFrames.Count < Tracker.MIN_CALIBRATION_FRAMES_STORED)
                {
                    return false;
                }
            }
            return true;
        }

        private void TryCalibration()
        {
            if (this.systemCalibrated)
            {
                return;
            }
            if (this.kinectClients.Count == this.expectedKinectsCount)
            {
                if (this.recentlyRecalibrated)
                {
                    this.recentlyRecalibrated = false;
                    return;
                }
                this.OnCalibration(this.GetCalibrationFramesRemaining());
                if (this.KinectsMeetCalibrationRequirement())
                {
                    this.kinectClients.Values.ToList<KinectClient>().ForEach(kinect => kinect.Calibrate());
                    this.systemCalibrated = true;
                }
            }
            else
            {
                this.OnWaitingKinects(this.GetKinectsRemaining());
            }
        }

        private void ReCalibration(string msg)
        {
            this.systemCalibrated = false;
            foreach (KinectClient kinect in this.kinectClients.Values)
            {
                kinect.PrepareRecalibration();
            }
            this.OnRecalibration(msg);
        }

        public void SynchronizeTracking(IPEndPoint source, SBodyFrame frame)
        {
            if (!this.kinectClients.ContainsKey(source))
            {
                // Pass in height and tilt angle
                this.kinectClients[source] = new KinectClient((uint)this.kinectClients.Count, source, 0.0, 0.0);
            }
            lock (this.syncTrackLock)
            {
                if (!this.systemCalibrated)
                {
                    this.TryCalibration();
                    if (this.systemCalibrated)
                    {
                        this.currentResult = this.PeopleDetection();
                        this.initialPeopleTracked = this.currentResult.People.Count();
                        Debug.WriteLine("initial people: " + this.initialPeopleTracked);
                    }
                }
                string unexpectedBehavior = "";
                if (this.ContainsExpectedMovements(source, frame, ref unexpectedBehavior))
                {
                    this.kinectClients[source].UpdateFrame(frame);
                }
                else
                {
                    this.ReCalibration(unexpectedBehavior);
                    this.currentResult = TrackerResult.Empty;
                    this.initialPeopleTracked = 0;
                    this.recentlyRecalibrated = true;
                }
                this.OnResult(this.currentResult);
            }
        }

        private bool RemainStationary(JointType jt, SBody currentBody, SBody previousBody, ref string msg)
        {
            if (currentBody == null || !previousBody.Joints.ContainsKey(jt) || !currentBody.Joints.ContainsKey(jt))
            {
                msg = "Missing"+jt;
                return false;
            }
            CameraSpacePoint currentHeadPt = currentBody.Joints[jt].CameraSpacePoint;
            CameraSpacePoint previousHeadPt = previousBody.Joints[jt].CameraSpacePoint;
            double difference = Math.Sqrt(Math.Pow(previousHeadPt.X - currentHeadPt.X, 2) + Math.Pow(previousHeadPt.Y - currentHeadPt.Y, 2) + Math.Pow(previousHeadPt.Z - currentHeadPt.Z, 2));
            if (difference > 0.1)
            {
                msg = ""+jt+" remain stationary";
                return false;
            }
            return true;
        }

        // Use more than the brain
        private bool ContainsExpectedMovements(IPEndPoint source, SBodyFrame frame, ref string msg)
        {
            if (!this.systemCalibrated)
            {
                if (this.kinectClients[source].FirstCalibrationFrame != null)
                {
                    foreach (SBody previousBody in this.kinectClients[source].FirstCalibrationFrame.Bodies)
                    {
                        SBody currentBody = frame.Bodies.Find(x => x.TrackingId == previousBody.TrackingId);
                        // Head
                        if (!this.RemainStationary(JointType.Head, currentBody, previousBody, ref msg))
                        {
                            return false;
                        }
                        // Left hand
                        if (!this.RemainStationary(JointType.HandLeft, currentBody, previousBody, ref msg))
                        {
                            return false;
                        }
                        // Right right
                        if (!this.RemainStationary(JointType.HandRight, currentBody, previousBody, ref msg))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                bool allMissing = true;
                foreach (TrackerResult.Person person in this.currentResult.People)
                {
                    bool personMissing = true;
                    foreach (TrackerResult.PotentialSkeleton potentialSkeletons in person.PotentialSkeletons)
                    {
                        if (potentialSkeletons.Skeleton.CurrentPosition != null)
                        {
                            personMissing = false;
                            break;
                        }
                    }
                    if (!personMissing)
                    {
                        allMissing = false;
                        break;
                    }
                }
                if (allMissing)
                {
                    msg = "Out of Bounds?";
                    return false;
                }
            }
            return true;
        }

        # region People Detection
        private TrackerResult PeopleDetection()
        {
            List<TrackerResult.KinectFOV> currentFOVsList = new List<TrackerResult.KinectFOV>();
            List<TrackerResult.PotentialSkeleton> currentSkeletonsList = new List<TrackerResult.PotentialSkeleton>();
            foreach (KinectClient kinect in this.kinectClients.Values)
            {
                TrackerResult.KinectFOV fov = KinectClient.ExtractFOVInfo(kinect);
                currentFOVsList.Add(fov);
                foreach (TrackingSkeleton skeleton in kinect.skeletonsList)
                {
                    currentSkeletonsList.Add(new TrackerResult.PotentialSkeleton(fov, skeleton));
                }
            }

            Debug.WriteLine("FOV: " + currentFOVsList.Count, "People Detection");
            Debug.WriteLine("Skeletons: " + currentSkeletonsList.Count, "People Detection");

            List<TrackerResult.Person> trackedPeopleList = new List<TrackerResult.Person>();
            while (true)
            {
                TrackerResult.Person person = this.FindPersonWithMultipleSkeletons(ref currentSkeletonsList);
                if (person != null)
                {
                    // Update person id!!
                    person.Id = (uint)trackedPeopleList.Count;
                    trackedPeopleList.Add(person);
                }
                else
                {
                    break;
                }
            }

            Debug.WriteLine("Tracked people after matching: " + trackedPeopleList.Count, "People Detection");

            // Add remaining skeletons as single-skeleton person
            foreach (TrackerResult.PotentialSkeleton potentialSkeleton in currentSkeletonsList)
            {
                if (potentialSkeleton.Skeleton.CurrentPosition != null)
                {
                    // Update skeleton id!!
                    potentialSkeleton.Id = 0;
                    TrackerResult.Person person = new TrackerResult.Person(potentialSkeleton);
                    // Update person id!!
                    person.Id = (uint)trackedPeopleList.Count;
                    trackedPeopleList.Add(person);
                }
            }

            Debug.WriteLine("Tracked people total: " + trackedPeopleList.Count, "People Detection");

            if (trackedPeopleList.Count == 0)
            {
                return TrackerResult.Empty;
            }
            else
            {
                return new TrackerResult(currentFOVsList, trackedPeopleList);
            }
        }

        // Create person based on different FOVs and proximity in worldview positions
        // TODO: extend for multiple FOVs (now: 2)
        private TrackerResult.Person FindPersonWithMultipleSkeletons(ref List<TrackerResult.PotentialSkeleton> skeletonsList)
        {
            // Find the two closest skeletons
            double globalDifference = Double.MaxValue;
            TrackerResult.PotentialSkeleton skeletonMatch1 = null, skeletonMatch2 = null;
            foreach (TrackerResult.PotentialSkeleton thisSkeleton in skeletonsList)
            {
                TrackerResult.KinectFOV fov1 = thisSkeleton.FOV;
                TrackingSkeleton skeleton1 = thisSkeleton.Skeleton;
                foreach (TrackerResult.PotentialSkeleton thatSkeleton in skeletonsList)
                {
                    TrackerResult.KinectFOV fov2 = thatSkeleton.FOV;
                    TrackingSkeleton skeleton2 = thatSkeleton.Skeleton;
                    if (!fov1.Equals(fov2))
                    {
                        WBody pos1 = skeleton1.CurrentPosition.Worldview;
                        WBody pos2 = skeleton2.CurrentPosition.Worldview;
                        double localDifference = WBody.CalculateDifferences(pos1, pos2);
                        if (localDifference < globalDifference)
                        {
                            globalDifference = localDifference;
                            skeletonMatch1 = thisSkeleton;
                            skeletonMatch2 = thatSkeleton;
                        }
                    }
                }
            }

            if (skeletonMatch1 != null && skeletonMatch2 != null)
            {
                skeletonsList.Remove(skeletonMatch1);
                skeletonsList.Remove(skeletonMatch2);
                skeletonMatch1.Id = 0;
                skeletonMatch2.Id = 1;
                return new TrackerResult.Person(skeletonMatch1, skeletonMatch2);
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
