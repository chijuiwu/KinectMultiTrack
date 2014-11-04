using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;


namespace KinectSerializer
{
    [Serializable]
    class SerializableBodyFrame : ISerializable
    {
        public SerializableBodyFrame() { 
        
        }
    }
}
