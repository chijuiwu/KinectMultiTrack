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
        private bool calibrated = false;

        private readonly int KINECT_COUNT;
        private ConcurrentDictionary<IPEndPoint, KinectAgent> kinectAgentsDict;

        private readonly object syncFrameLock = new object();
        public class SyncFrameResult
        {
            private IEnumerable<Tuple<IPEndPoint, SerializableBodyFrame>> currentRawFrames;
            private IEnumerable<Tuple<IPEndPoint, WorldView>> currentFramesInWorldView;

            public SyncFrameResult(IEnumerable<Tuple<IPEndPoint, SerializableBodyFrame>> currentRawFrames, IEnumerable<Tuple<IPEndPoint, WorldView>> currentFramesInWorldView)
            {
                this.currentRawFrames = currentRawFrames;
                this.currentFramesInWorldView = currentFramesInWorldView;
            }
        }

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectAgentsDict = new ConcurrentDictionary<IPEndPoint, KinectAgent>();
        }

        public void AddOrUpdateBodyFrame(IPEndPoint clientIP, SerializableBodyFrame bodyFrame)
        {
            if (!this.kinectAgentsDict.ContainsKey(clientIP))
            {
                this.kinectAgentsDict[clientIP] = new KinectAgent();
            }
            this.kinectAgentsDict[clientIP].AddFrame(bodyFrame);
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectAgent kinectProfile;
            if (this.kinectAgentsDict.TryRemove(clientIP, out kinectProfile))
            {
                kinectProfile.DisposeUI();
            }
        }

        private bool RequireCalibration()
        {
            bool readyForCalibration = true;
            foreach (KinectAgent kinect in this.kinectAgentsDict.Values)
            {
                if (!kinect.ReadyForCalibration)
                {
                    readyForCalibration = false;
                }
            }
            if (readyForCalibration)
            {
                return this.kinectAgentsDict.Count >= this.KINECT_COUNT && !this.calibrated;
            }
            else
            {
                return false;
            }
        }

        public SyncFrameResult SynchronizeFrames(IPEndPoint referenceKinectFOV)
        {
            lock (syncFrameLock)
            {
                if (this.RequireCalibration())
                {
                    foreach (KinectAgent kinect in this.kinectAgentsDict.Values)
                    {
                        kinect.Calibrate();
                    }
                }
                List<Tuple<IPEndPoint, SerializableBodyFrame>> rawFramesList = new List<Tuple<IPEndPoint,SerializableBodyFrame>>();
                List<Tuple<IPEndPoint, WorldView>> worldviewFramesList = new List<Tuple<IPEndPoint, WorldView>>();
                foreach (IPEndPoint clientIP in this.kinectAgentsDict.Keys)
                {
                    this.kinectAgentsDict[clientIP].ProcessFrames();
                    SerializableBodyFrame rawFrame = this.kinectAgentsDict[clientIP].LatestRawFrame;
                    rawFramesList.Add(Tuple.Create(clientIP, rawFrame));
                    WorldView worldviewFrame = this.kinectAgentsDict[clientIP].LatestWorldviewFrame;
                    worldviewFramesList.Add(Tuple.Create(clientIP, worldviewFrame));
                }
                SyncFrameResult result = new SyncFrameResult(rawFramesList, worldviewFramesList);
                return result;
            }
        }
    }
}
