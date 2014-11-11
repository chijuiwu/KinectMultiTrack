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
    public class SerializableBodyFrame : ISerializable
    {
        public const string NameTimeStamp = "TimeStamp";
        public const string NameBodyList = "Bodies";
        public const string NameDepthFrameWidth = "DepthFrameWidth";
        public const string NameDepthFrameHeight = "DepthFrameHeight";

        private long timeStamp;
        private List<SerializableBody> bodies;
        private int depthFrameWidth;
        private int depthFrameHeight;

        public SerializableBodyFrame(TimeSpan relativeTime, FrameDescription depthFrameDescription)
        {
            this.timeStamp = relativeTime.Ticks;
            this.bodies = new List<SerializableBody>();
            this.depthFrameWidth = depthFrameDescription.Width;
            this.depthFrameHeight = depthFrameDescription.Height;
        }

        public void addSerializableBody(SerializableBody body)
        {
            this.bodies.Add(body);
        }

        protected SerializableBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.timeStamp = (long)info.GetValue(SerializableBodyFrame.NameTimeStamp, typeof(long));
            this.bodies = (List<SerializableBody>)info.GetValue(SerializableBodyFrame.NameBodyList, typeof(List<SerializableBody>));
            this.depthFrameWidth = (int)info.GetValue(SerializableBodyFrame.NameDepthFrameWidth, typeof(int));
            this.depthFrameHeight = (int)info.GetValue(SerializableBodyFrame.NameDepthFrameHeight, typeof(int));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBodyFrame.NameTimeStamp, this.timeStamp, typeof(long));
            info.AddValue(SerializableBodyFrame.NameBodyList, this.bodies, typeof(List<SerializableBody>));
            info.AddValue(SerializableBodyFrame.NameDepthFrameWidth, this.depthFrameWidth, typeof(int));
            info.AddValue(SerializableBodyFrame.NameDepthFrameHeight, this.depthFrameHeight, typeof(int));
        }

        public long TimeStamp
        {
            get
            {
                return this.timeStamp;
            }
        }

        public List<SerializableBody> Bodies
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
    }
}
