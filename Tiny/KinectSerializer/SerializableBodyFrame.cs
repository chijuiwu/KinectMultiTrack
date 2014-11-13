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
        private int depthFrameWidth;
        private int depthFrameHeight;
        private List<SerializableBody> bodies;

        public SerializableBodyFrame(TimeSpan relativeTime, FrameDescription depthFrameDescription)
            : this(relativeTime.Ticks, depthFrameDescription.Width, depthFrameDescription.Height)
        {
        }

        public SerializableBodyFrame(long timeStamp, int depthFrameWidth, int depthFrameHeight)
        {
            this.timeStamp = timeStamp;
            this.depthFrameWidth = depthFrameWidth;
            this.depthFrameHeight = depthFrameHeight;
            this.bodies = new List<SerializableBody>();
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

        public static SerializableBodyFrame Copy(SerializableBodyFrame bodyFrame)
        {
            SerializableBodyFrame copy = new SerializableBodyFrame(bodyFrame.TimeStamp, bodyFrame.DepthFrameWidth, bodyFrame.DepthFrameHeight);
            foreach (SerializableBody body in bodyFrame.Bodies)
            {
                copy.addSerializableBody(SerializableBody.Copy(body));
            }
            return copy;
        }
    }
}
