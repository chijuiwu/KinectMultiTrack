using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.IO.Compression;
using Microsoft.Kinect;

namespace KinectSerializer
{
    public class BodyFrameSerializer
    {

        public static byte[] Serialize(TimeSpan timeSpan, Body[] bodies)
        {
            BinaryFormatter bf = new BinaryFormatter();
            SerializableBodyFrame bodyFrame = new SerializableBodyFrame(timeSpan, bodies);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, bodyFrame);
                    return ms.ToArray();
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Serialization Exception...");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return new byte[]{};
            }
        }

        public static SerializableBodyFrame Deserialize(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    return (SerializableBodyFrame)bf.Deserialize(ms);
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Serialization Exception...");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
