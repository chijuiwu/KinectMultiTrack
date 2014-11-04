using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;
using Microsoft.Kinect;

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
            SerializableBody[] copy = new SerializableBody[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                copy[i] = new SerializableBody(bodies[i]);
            }
        }

        public SerializableBodyFrame(SerializationInfo info, StreamingContext ctx)
        {
            this.timeStamp = (long)info.GetValue(SerializableBodyFrame.NameTimeStamp, typeof(long));
            this.bodies = (SerializableBody[])info.GetValue(SerializableBodyFrame.NameBodyArray, typeof(SerializableBody[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(SerializableBodyFrame.NameTimeStamp, this.timeStamp);
            info.AddValue(SerializableBodyFrame.NameBodyArray, this.bodies);
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
