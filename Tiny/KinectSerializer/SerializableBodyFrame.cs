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

        private long timeStamp;
        private SerializableBody[] bodies;

        public SerializableBodyFrame(TimeSpan timeSpan, Body[] bodies)
        {
            this.timeStamp = timeSpan.Ticks;
            this.bodies = new SerializableBody[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                this.bodies[i] = new SerializableBody(bodies[i]);
            }
        }

        protected SerializableBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.timeStamp = (long)info.GetValue(SerializableBodyFrame.NameTimeStamp, typeof(long));
            this.bodies = (SerializableBody[])info.GetValue(SerializableBodyFrame.NameBodyArray, typeof(SerializableBody[]));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBodyFrame.NameTimeStamp, this.timeStamp, typeof(long));
            info.AddValue(SerializableBodyFrame.NameBodyArray, this.bodies, typeof(SerializableBody[]));
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
    }
}
