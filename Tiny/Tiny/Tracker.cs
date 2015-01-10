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
    class Tracker
    {
        public const int CALIBRATION_FRAMES = 120;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectProfile> kinectProfilesDict;

        private readonly object syncFrameLock = new object();

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectProfilesDict = new ConcurrentDictionary<IPEndPoint, KinectProfile>();
        }

        public IEnumerable<Tuple<IPEndPoint, SerializableBodyFrame>> GetLatestRawFrames()
        {
            foreach (IPEndPoint clientIP in this.kinectProfilesDict.Keys)
            {
                SerializableBodyFrame latestFrame = this.kinectProfilesDict[clientIP].LatestFrame;
                yield return Tuple.Create(clientIP, latestFrame);
            }
        }

        public IEnumerable<Tuple<IPEndPoint, WorldView>> GetLatestFramesInWorldView(IPEndPoint referenceKinectFOV)
        {

            foreach (IPEndPoint clientIP in this.kinectProfilesDict.Keys)
            {
                WorldView latestWorldView = this.kinectProfilesDict[clientIP].LatestWorldView;
                yield return Tuple.Create(clientIP, latestWorldView);
            }
        }

        public void AddOrUpdateBodyFrame(IPEndPoint clientIP, SerializableBodyFrame bodyFrame)
        {
            if (!this.kinectProfilesDict.ContainsKey(clientIP))
            {
                this.kinectProfilesDict[clientIP] = new KinectProfile();
            }
            this.kinectProfilesDict[clientIP].AddFrame(bodyFrame);
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectProfile kinectProfile;
            if (this.kinectProfilesDict.TryRemove(clientIP, out kinectProfile))
            {
                kinectProfile.DisposeUI();
            }
        }

        public void SynchronizeFrames()
        {
            lock (syncFrameLock)
            {
                if (this.users.Count >= this.EXPECTED_CONNECTIONS)
                {
                    bool readyToCalibrate = true;
                    foreach (Person user in this.users.Values)
                    {
                        if (!user.ReadyToCalibrate)
                        {
                            readyToCalibrate = false;
                            break;
                        }
                    }
                    if (readyToCalibrate)
                    {
                        foreach (IPEndPoint userIP in this.users.Keys)
                        {
                            Person user = this.users[userIP];
                            Debug.WriteLine("user IP: " + userIP);
                            user.CalibrateKinect();
                        }
                    }
                }

                foreach (Person user in this.users.Values)
                {
                    user.ProcessBodyFrame();
                }
            }
        }
    }
}
