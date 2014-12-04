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

namespace Tiny
{
    class UserTracker
    {
        public const int CALIBRATION_FRAMES = 120;

        private readonly int EXPECTED_CONNECTIONS;
        private int currentConnections;

        // Assume one user per Kinect
        private ConcurrentDictionary<IPEndPoint, User> users;

        public UserTracker(int expectedConnections)
        {
            this.EXPECTED_CONNECTIONS = expectedConnections;
            this.currentConnections = 0;
            this.users = new ConcurrentDictionary<IPEndPoint, User>();
        }

        public IEnumerable<SerializableBodyFrame> UserLastKinectFrames
        {
            get
            {
                foreach (User user in users.Values)
                {
                    SerializableBodyFrame lastKinectFrame = user.LastKinectFrame;
                    if (lastKinectFrame != null)
                    {
                        yield return lastKinectFrame;
                    }
                }
            }
        }

        public IEnumerable<WorldView> UserLastWorldViews
        {
            get
            {
                foreach (User user in users.Values)
                {
                    WorldView lastWorldView = user.LastWorldView;
                    if (lastWorldView != null)
                    {
                        yield return lastWorldView;
                    }
                }
            }
        }

        public bool CalibrationCompleted
        {
            get
            {
                foreach (User user in users.Values)
                {
                    if (!user.CalibrationCompleted)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void AddOrUpdateBodyFrame(IPEndPoint clientIP, SerializableBodyFrame bodyFrame)
        {
            if (!this.users.ContainsKey(clientIP))
            {
                this.users[clientIP] = new User();
                this.currentConnections++;
            }
            this.users[clientIP].IncomingBodyFrames.Enqueue(bodyFrame);
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            User user;
            if (this.users.TryRemove(clientIP, out user))
            {
                user.CloseBodyViewer();
                this.currentConnections--;
            }
        }

        public void SynchronizeFrames()
        {
            if (this.users.Count >= this.EXPECTED_CONNECTIONS)
            {
                bool readyToCalibrate = true;
                foreach (User user in this.users.Values)
                {
                    if (!user.ReadyToCalibrate)
                    {
                        readyToCalibrate = false;
                        break;
                    }
                }
                if (readyToCalibrate)
                {
                    foreach (User user in this.users.Values)
                    {
                        user.CalibrateKinect();
                    }
                }
            }

            foreach (User user in this.users.Values)
            {
                user.ProcessBodyFrame();
            }
        }
    }
}
