using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using Microsoft.Kinect;

namespace KinectSerializer
{
    public class BodyFrameSerializer
    {

        public static BinaryFormatter bf = new BinaryFormatter();

        public static byte[] Serialize(TimeSpan timeSpan, Body[] bodies)
        {
            SerializableBodyFrame bodyFrame = new SerializableBodyFrame(timeSpan, bodies);
            using (MemoryStream ms = new MemoryStream())
            {
                BodyFrameSerializer.bf.Serialize(ms, bodyFrame);
                return ms.ToArray();
            }
        }

        public static SerializableBodyFrame Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return (SerializableBodyFrame)BodyFrameSerializer.bf.Deserialize(ms);
            }
        }
    }
}
