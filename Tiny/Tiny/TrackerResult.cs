using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Tiny
{
    public class TResult
    {
        
        public class KinectFOV
        {
            public IPEndPoint ClientIP { get; private set; }
            public uint Id { get; private set; }
            public KinectCamera.Specification Specification { get; private set; }

            public KinectFOV(IPEndPoint clientIP, uint id, KinectCamera.Specification specification)
            {
                this.ClientIP = clientIP;
                this.Id = id;
                this.Specification = specification;
            }
        }

        public class SkeletonReplica
        {
            public uint Id { get; private set; }
            public KinectFOV FOV { get; private set; }
            public TSkeleton Skeleton { get; private set; }

            public SkeletonReplica(uint id, KinectFOV fov, TSkeleton skeleton)
            {
                this.Id = id;
                this.FOV = fov;
                this.Skeleton = skeleton;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                sb.Append("FOV: ").Append(this.FOV.ClientIP).Append(", ");
                sb.Append("Skeleton: ").Append(this.Skeleton);
                sb.Append("]");
                return sb.ToString();
            }
        }

        public class Person
        {
            public uint Id { get; private set; }
            public IEnumerable<SkeletonReplica> Replicas { get; private set; }

            public Person(uint id, IEnumerable<SkeletonReplica> skeletons)
            {
                this.Id = id;
                this.Replicas = skeletons;
            }

            public TSkeleton FindSkeletonInFOV(KinectFOV fov)
            {
                foreach (SkeletonReplica match in this.Replicas)
                {
                    if (match.FOV.Equals(fov))
                    {
                        return match.Skeleton;
                    }
                }
                return null;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[Skeletons: ").Append(this.Replicas.Count()).Append("]: ");
                String prefix = "";
                foreach (SkeletonReplica match in this.Replicas)
                {
                    sb.Append(prefix);
                    prefix = ",";
                    sb.Append(match);
                }
                return sb.ToString();
            }
        }

        public long Timestamp { get; private set; }
        public IEnumerable<KinectFOV> FOVs { get; private set; }
        public IEnumerable<Person> People { get; private set; }

        public TResult(IEnumerable<KinectFOV> fovs, IEnumerable<Person> people)
        {
            this.Timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            this.FOVs = fovs;
            this.People = people;
        }
    }
}
