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
        public const int MIN_CALIBRATION_FRAMES_STORED = MIN_CALIBRATION_FRAMES * 2;
        private readonly object syncTrackLock = new object();
        private bool systemCalibrated;

        private readonly ConcurrentDictionary<IPEndPoint, KinectClient> kinectClients;

        private TrackerResult currentResult;

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
                }
                this.OnResult(TrackerResult.Copy(this.currentResult));
            }
        }

        private bool ContainsExpectedMovements(IPEndPoint source, SBodyFrame frame, ref string msg)
        {
            if (this.kinectClients[source].CurrentSkeletonCount > 0)
            {
                if (frame.Bodies.Count() != this.kinectClients[source].CurrentSkeletonCount)
                {
                    msg = "Intruder? Out of boundary?";
                    return false;
                }
                if (!this.systemCalibrated)
                {
                    foreach (SBody body in this.kinectClients[source].FirstCalibrationFrame.Bodies)
                    {
                        SBody currentBody = frame.Bodies.Find(x => x.TrackingId == body.TrackingId);
                        if (!body.Joints.ContainsKey(JointType.Head) || !currentBody.Joints.ContainsKey(JointType.Head))
                        {
                            msg = "Missing head";
                            return false;
                        }
                        CameraSpacePoint firstHeadPt = body.Joints[JointType.Head].CameraSpacePoint;
                        CameraSpacePoint currentHeadPt = currentBody.Joints[JointType.Head].CameraSpacePoint;
                        double difference = Math.Sqrt(Math.Pow(firstHeadPt.X - currentHeadPt.X, 2) + Math.Pow(firstHeadPt.Y - currentHeadPt.Y, 2) + Math.Pow(firstHeadPt.Z - currentHeadPt.Z, 2));
                        if (difference > 0.05)
                        {
                            msg = "Head movement?";
                            return false;
                        }
                    }
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

            Debug.WriteLine("Tracked people: " + trackedPeopleList.Count, "People Detection");

            // Add remaining skeletons as single-skeleton person
            foreach (TrackerResult.PotentialSkeleton potentialSkeleton in currentSkeletonsList)
            {
                // Update skeleton id!!
                potentialSkeleton.Id = 0;
                TrackerResult.Person person = new TrackerResult.Person(potentialSkeleton);
                // Update person id!!
                person.Id = (uint)trackedPeopleList.Count;
                trackedPeopleList.Add(person);
            }

            return new TrackerResult(currentFOVsList, trackedPeopleList);
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
