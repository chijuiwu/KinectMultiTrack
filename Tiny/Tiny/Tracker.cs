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
            public class KinectFrame
            {
                private IPEndPoint clientIP;
                private SBodyFrame rawFrame;
                private WBodyFrame worldviewFrame;

                public IPEndPoint ClientIP
                {
                    get
                    {
                        return this.clientIP;
                    }
                }

                public SBodyFrame RawFrame
                {
                    get
                    {
                        return this.rawFrame;
                    }
                }

                public WBodyFrame WorldViewFrame
                {
                    get
                    {
                        return this.worldviewFrame;
                    }
                }

                public KinectFrame(IPEndPoint clientIP, SBodyFrame rawFrame, WBodyFrame worldviewFrame)
                {
                    this.clientIP = clientIP;
                    this.rawFrame = rawFrame;
                    this.worldviewFrame = worldviewFrame;
                }
            }

            private IEnumerable<KinectFrame> frames;


            public IEnumerable<Tuple<IPEndPoint, SBodyFrame>> RawFrames
            {
                get
                {
                    foreach (KinectFrame frame in this.frames)
                    {
                        yield return Tuple.Create(frame.ClientIP, frame.RawFrame);
                    }
                }
            }

            public IEnumerable<Tuple<IPEndPoint, WBodyFrame>> WorldviewFrames
            {
                get
                {
                    foreach (KinectFrame frame in this.frames)
                    {
                        yield return Tuple.Create(frame.ClientIP, frame.WorldViewFrame);
                    }
                }
            }

            public SyncResult(IEnumerable<KinectFrame> frames)
            {
                this.frames = frames;
            }
        }

        public Tracker(int kinectCount)
        {
            this.KINECT_COUNT = kinectCount;
            this.kinectsDict = new ConcurrentDictionary<IPEndPoint, KinectAgent>();
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            KinectAgent kinect;
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
                List<SyncResult.KinectFrame> frames = new List<SyncResult.KinectFrame>();
                foreach (IPEndPoint client in this.kinectsDict.Keys)
                {
                    SBodyFrame rawFrame = this.kinectsDict[client].CurrentRawFrame;
                    WBodyFrame worldviewFrame = this.kinectsDict[client].CurrentWorldviewFrame;
                    frames.Add(new SyncResult.KinectFrame(client, rawFrame, worldviewFrame));
                }
                return new SyncResult(frames);
            }
        }
    }
}
