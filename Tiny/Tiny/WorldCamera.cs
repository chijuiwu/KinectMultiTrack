using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectSerializer;
using System.Collections.Concurrent;
using System.Net;

namespace Tiny
{
    class WorldCamera
    {
        private SerializableBodyFrame bodyFrame;
        private ConcurrentDictionary<IPEndPoint, KinectCamera> connectedCameras;

        public SerializableBodyFrame BodyFrame
        {
            get
            {
                return this.connectedCameras.First(camera => true).Value.CurrentBodyFrame;
            }
        }

        public IEnumerable<SerializableBodyFrame> ClientBodyFrames
        {
            get
            {
                foreach (KinectCamera camera in this.connectedCameras.Values)
                {
                    yield return camera.CurrentBodyFrame;
                }
            }
        }

        public WorldCamera()
        {
            this.connectedCameras = new ConcurrentDictionary<IPEndPoint, KinectCamera>();
        }

        public void AddOrUpdateClientCamera(KinectCamera clientCamera)
        {
            this.connectedCameras.AddOrUpdate(clientCamera.ClientIP, clientCamera, (key, oldValue) => clientCamera);
        }

        public void RemoveClientCamera(KinectCamera clientCamera)
        {
            KinectCamera result;
            this.connectedCameras.TryRemove(clientCamera.ClientIP, out result);
        }

        public void SynchronizeFrames()
        {
            // TODO
        }


    }
}
