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

        public long TimeStamp { get; private set; }
        public int DepthFrameWidth { get; private set; }
        public int DepthFrameHeight { get; private set; }
        public List<SBody> Bodies { get; private set; }

        public SBodyFrame(TimeSpan relativeTime, FrameDescription depthFrameDescription)
            : this(relativeTime.Ticks, depthFrameDescription.Width, depthFrameDescription.Height)
        {
        }

        public SBodyFrame(long timeStamp, int depthFrameWidth, int depthFrameHeight)
        {
            this.TimeStamp = timeStamp;
            this.DepthFrameWidth = depthFrameWidth;
            this.DepthFrameHeight = depthFrameHeight;
            this.Bodies = new List<SBody>();
        }

        public void addSerializableBody(SBody body)
        {
            this.Bodies.Add(body);
        }

        protected SBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.TimeStamp = (long)info.GetValue(SBodyFrame.NameTimeStamp, typeof(long));
            this.Bodies = (List<SBody>)info.GetValue(SBodyFrame.NameBodyList, typeof(List<SBody>));
            this.DepthFrameWidth = (int)info.GetValue(SBodyFrame.NameDepthFrameWidth, typeof(int));
            this.DepthFrameHeight = (int)info.GetValue(SBodyFrame.NameDepthFrameHeight, typeof(int));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SBodyFrame.NameTimeStamp, this.TimeStamp, typeof(long));
            info.AddValue(SBodyFrame.NameBodyList, this.Bodies, typeof(List<SBody>));
            info.AddValue(SBodyFrame.NameDepthFrameWidth, this.DepthFrameWidth, typeof(int));
            info.AddValue(SBodyFrame.NameDepthFrameHeight, this.DepthFrameHeight, typeof(int));
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
