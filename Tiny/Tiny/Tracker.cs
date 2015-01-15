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
        private ConcurrentDictionary<IPEndPoint, KinectAgent> kinectsDict;

        private readonly object syncFrameLock = new object();

        public class SyncResult
        {
            public class FrameMapping
            {
                private SBodyFrame rawFrame;
                private WorldBodyFrame worldviewFrame;

                public FrameMapping(SBodyFrame rawFrame, WorldBodyFrame worldviewFrame)
                {
                    this.rawFrame = rawFrame;
                    this.worldviewFrame = worldviewFrame;
                }

                public SBodyFrame RawFrame
                {
                    get
                    {
                        return this.rawFrame;
                    }
                }

                public WorldBodyFrame WorldViewFrame
                {
                    get
                    {
                        return this.worldviewFrame;
                    }
                }
            }
            private IEnumerable<Tuple<IPEndPoint, FrameMapping>> frames;

            public SyncResult(IEnumerable<Tuple<IPEndPoint, FrameMapping>> frames)
            {
                this.frames = frames;
            }

            public IEnumerable<Tuple<IPEndPoint, SBodyFrame>> RawFrames
            {
                get
                {
                    foreach (Tuple<IPEndPoint, FrameMapping> frameTuple in this.frames)
                    {
                        yield return Tuple.Create(frameTuple.Item1, frameTuple.Item2.RawFrame);
                    }
                }
            }

            public IEnumerable<Tuple<IPEndPoint, WorldBodyFrame>> WorldviewFrames
            {
                get
                {
                    foreach (Tuple<IPEndPoint, FrameMapping> frameTuple in this.frames)
                    {
                        yield return Tuple.Create(frameTuple.Item1, frameTuple.Item2.WorldViewFrame);
                    }
                }
            }
        }

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectsDict = new ConcurrentDictionary<IPEndPoint, KinectAgent>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectAgent kinectProfile;
            if (this.kinectsDict.TryRemove(clientIP, out kinectProfile))
            {
                kinectProfile.DisposeUI();
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
                    if (!kinect.ReadyToCalibrate)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public SyncResult Synchronize(IPEndPoint clientIP, SBodyFrame bodyframe)
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
                this.kinectsDict[clientIP].ProcessFrames(bodyframe);
                List<Tuple<IPEndPoint, SyncResult.FrameMapping>> frames = new List<Tuple<IPEndPoint, SyncResult.FrameMapping>>();
                foreach (IPEndPoint client in this.kinectsDict.Keys)
                {
                    SBodyFrame rawFrame = this.kinectsDict[clientIP].CurrentRawFrame;
                    WorldBodyFrame worldviewFrame = this.kinectsDict[clientIP].CurrentWorldviewFrame;
                    frames.Add(Tuple.Create(client, new SyncResult.FrameMapping(rawFrame, worldviewFrame)));
                }
                return new SyncResult(frames);
            }
        }
    }
}
