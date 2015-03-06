using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Tiny
{
    public class TrackerResult
    {
        public long Timestamp { get; private set; }
        public IEnumerable<KinectFOV> FOVs { get; private set; }
        public IEnumerable<Person> People { get; private set; }
        public static TrackerResult Empty = new TrackerResult(Enumerable.Empty<KinectFOV>(), Enumerable.Empty<TrackerResult.Person>());

        public TrackerResult(IEnumerable<KinectFOV> fovs, IEnumerable<Person> people)
        {
            this.Timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            this.FOVs = fovs;
            this.People = people;
        }

        public static PotentialSkeleton GetLocalSkeletonReference(Person person)
        {
            foreach (PotentialSkeleton skeleton in person.Skeletons)
            {
                if (skeleton.FOV.ClientIP.Address.ToString().Equals("127.0.0.1"))
                {
                    return skeleton;
                }
            }
            return person.Skeletons.First();
        }

        public class KinectFOV
        {
            public uint Id { get; private set; }
            public IPEndPoint ClientIP { get; private set; }
            public KinectClient.Specification Specification { get; private set; }

            public KinectFOV(uint id, IPEndPoint clientIP, KinectClient.Specification specification)
            {
                this.Id = id;
                this.ClientIP = clientIP;
                this.Specification = specification;
            }
        }

        public class PotentialSkeleton
        {
            public uint Id { get; set; }
            public KinectFOV FOV { get; private set; }
            public MovingSkeleton Skeleton { get; private set; }

            public PotentialSkeleton(KinectFOV fov, MovingSkeleton skeleton)
                : this(UInt32.MaxValue, fov, skeleton)
            {
            }

            public PotentialSkeleton(uint id, KinectFOV fov, MovingSkeleton skeleton)
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
            public uint Id { get; set; }
            public IEnumerable<PotentialSkeleton> Skeletons { get; private set; }

            public Person(params PotentialSkeleton[] skeletons)
                : this(UInt32.MaxValue, skeletons)
            {

            }

            public Person(uint id, IEnumerable<PotentialSkeleton> skeletons)
            {
                this.Id = id;
                this.Skeletons = skeletons;
            }

            public MovingSkeleton FindSkeletonInFOV(KinectFOV fov)
            {
                foreach (PotentialSkeleton potentialSkeleton in this.Skeletons)
                {
                    if (potentialSkeleton.FOV.Equals(fov))
                    {
                        return potentialSkeleton.Skeleton;
                    }
                }
                return null;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[Skeletons: ").Append(this.Skeletons.Count()).Append("]: ");
                String prefix = "";
                foreach (PotentialSkeleton match in this.Skeletons)
                {
                    sb.Append(prefix);
                    prefix = ",";
                    sb.Append(match);
                }
                return sb.ToString();
            }
        }
    }
}
