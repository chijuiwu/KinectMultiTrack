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
        // Assume one user per Kinect
        private ConcurrentDictionary<IPEndPoint, User> users;

        public UserTracker()
        {
            this.users = new ConcurrentDictionary<IPEndPoint, User>();
        }

        public IEnumerable<SerializableBodyFrame> UserLastKinectFrames
        {
            get
            {
                foreach (User user in users.Values)
                {
                    yield return user.LastFrame.Item1;
                }
            }
        }

        public IEnumerable<WorldView> UserLastWorldViews
        {
            get
            {
                foreach (User user in users.Values)
                {
                    if (user.CalibrationCompleted)
                    {
                        yield return user.LastFrame.Item2;
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
            }
            this.users[clientIP].IncomingBodyFrames.Enqueue(bodyFrame);
        }

        public void RemoveClient(IPEndPoint clientIP)
        {
            User user;
            if (this.users.TryRemove(clientIP, out user))
            {
                user.CloseBodyViewer();
            }
        }

        public void SynchronizeFrames()
        {
            foreach (User user in this.users.Values)
            {
                user.ProcessBodyFrame();
            }
        }
    }
}
