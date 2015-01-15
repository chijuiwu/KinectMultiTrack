using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using System.IO;

namespace KinectSerializer
{
    [Serializable]
    public class SBodyFrame : ISerializable
    {
        public const string NameTimeStamp = "TimeStamp";
        public const string NameBodyList = "Bodies";
        public const string NameDepthFrameWidth = "DepthFrameWidth";
        public const string NameDepthFrameHeight = "DepthFrameHeight";

        private long timeStamp;
        private int depthFrameWidth;
        private int depthFrameHeight;
        private List<SBody> bodies;

        public SBodyFrame(TimeSpan relativeTime, FrameDescription depthFrameDescription)
            : this(relativeTime.Ticks, depthFrameDescription.Width, depthFrameDescription.Height)
        {
        }

        public SBodyFrame(long timeStamp, int depthFrameWidth, int depthFrameHeight)
        {
            this.timeStamp = timeStamp;
            this.depthFrameWidth = depthFrameWidth;
            this.depthFrameHeight = depthFrameHeight;
            this.bodies = new List<SBody>();
        }

        public void addSerializableBody(SBody body)
        {
            this.bodies.Add(body);
        }

        protected SBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.timeStamp = (long)info.GetValue(SBodyFrame.NameTimeStamp, typeof(long));
            this.bodies = (List<SBody>)info.GetValue(SBodyFrame.NameBodyList, typeof(List<SBody>));
            this.depthFrameWidth = (int)info.GetValue(SBodyFrame.NameDepthFrameWidth, typeof(int));
            this.depthFrameHeight = (int)info.GetValue(SBodyFrame.NameDepthFrameHeight, typeof(int));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SBodyFrame.NameTimeStamp, this.timeStamp, typeof(long));
            info.AddValue(SBodyFrame.NameBodyList, this.bodies, typeof(List<SBody>));
            info.AddValue(SBodyFrame.NameDepthFrameWidth, this.depthFrameWidth, typeof(int));
            info.AddValue(SBodyFrame.NameDepthFrameHeight, this.depthFrameHeight, typeof(int));
        }

        public long TimeStamp
        {
            get
            {
                return this.timeStamp;
            }
        }

        public List<SBody> Bodies
        {
            get
            {
                return this.bodies;
            }
        }

        public int DepthFrameWidth
        {
            get
            {
                return this.depthFrameWidth;
            }
        }

        public int DepthFrameHeight
        {
            get
            {
                return this.depthFrameHeight;
            }
        }

        public static SBodyFrame Copy(SBodyFrame bodyFrame)
        {
            SBodyFrame copy = new SBodyFrame(bodyFrame.TimeStamp, bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight);
            foreach (SBody body in bodyFrame.Bodies)
            {
                copy.addSerializableBody(SBody.Copy(body));
            }
            return copy;
        }
    }
}
