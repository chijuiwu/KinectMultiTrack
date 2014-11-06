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
        public const string NameBodyArray = "Bodies";
        public const string NameDepthFrameWidth = "DepthFrameWidth";
        public const string NameDepthFrameHeight = "DepthFrameHeight";

        private long timeStamp;
        private SerializableBody[] bodies;
        private int depthFrameWidth;
        private int depthFrameHeight;

        public SerializableBodyFrame(TimeSpan timeSpan, Body[] bodies, int depthFrameWidth, int depthFrameHeight)
        {
            this.timeStamp = timeSpan.Ticks;
            this.bodies = new SerializableBody[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                this.bodies[i] = new SerializableBody(bodies[i]);
            }
            this.depthFrameWidth = depthFrameWidth;
            this.depthFrameHeight = depthFrameHeight;
        }

        protected SerializableBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.timeStamp = (long)info.GetValue(SerializableBodyFrame.NameTimeStamp, typeof(long));
            this.bodies = (SerializableBody[])info.GetValue(SerializableBodyFrame.NameBodyArray, typeof(SerializableBody[]));
            this.depthFrameWidth = (int)info.GetValue(SerializableBodyFrame.NameDepthFrameWidth, typeof(int));
            this.depthFrameHeight = (int)info.GetValue(SerializableBodyFrame.NameDepthFrameHeight, typeof(int));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBodyFrame.NameTimeStamp, this.timeStamp, typeof(long));
            info.AddValue(SerializableBodyFrame.NameBodyArray, this.bodies, typeof(SerializableBody[]));
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

        public SerializableBody[] Bodies
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
