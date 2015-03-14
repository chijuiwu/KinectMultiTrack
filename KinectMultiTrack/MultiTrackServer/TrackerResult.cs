using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace KinectMultiTrack
{
    public class TrackerResult
    {
        public long Timestamp { get; private set; }
        public IEnumerable<KinectFOV> FOVs { get; private set; }
        public IEnumerable<Person> People { get; private set; }
        public static TrackerResult Empty = new TrackerResult(Enumerable.Empty<KinectFOV>(), Enumerable.Empty<TrackerResult.Person>());

        public int DepthFrameWidth
        {
            get
            {
                if (this == TrackerResult.Empty)
                {
                    return 0;
                }
                else
                {
                    return this.FOVs.First().Specification.DepthFrameWidth;
                }
            }
        }
        public int DepthFrameHeight
        {
            get
            {
                if (this == TrackerResult.Empty)
                {
                    return 0;
                }
                else
                {
                    return this.FOVs.First().Specification.DepthFrameHeight;
                }
            }
        }

        public TrackerResult(IEnumerable<KinectFOV> fovs, IEnumerable<Person> people)
        {
            this.Timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            this.FOVs = fovs;
            this.People = people;
        }

        public static TrackerResult Copy(TrackerResult result)
        {
            if (result.Equals(TrackerResult.Empty))
            {
                return result;
            }
            List<Person> peopleCopy = new List<Person>();
            foreach (Person person in result.People)
            {
                peopleCopy.Add(Person.Copy(person));
            }
            return new TrackerResult(result.FOVs, peopleCopy);
        }

        public static PotentialSkeleton GetLocalSkeletonReference(Person person)
        {
            foreach (PotentialSkeleton skeleton in person.PotentialSkeletons)
            {
                if (skeleton.FOV.ClientIP.Address.ToString().Equals("127.0.0.1"))
                {
                    return skeleton;
                }
            }
            return person.PotentialSkeletons.First();
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
            public TrackingSkeleton Skeleton { get; private set; }

            public PotentialSkeleton(KinectFOV fov, TrackingSkeleton skeleton)
                : this(UInt32.MaxValue, fov, skeleton)
            {
            }

            public PotentialSkeleton(uint id, KinectFOV fov, TrackingSkeleton skeleton)
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

            public static PotentialSkeleton Copy(PotentialSkeleton pSkeleton)
            {
                return new PotentialSkeleton(pSkeleton.Id, pSkeleton.FOV, TrackingSkeleton.Copy(pSkeleton.Skeleton));
            }
        }

        public class Person
        {
            public uint Id { get; set; }
            public IEnumerable<PotentialSkeleton> PotentialSkeletons { get; private set; }

            public Person(params PotentialSkeleton[] skeletons)
                : this(UInt32.MaxValue, skeletons)
            {

            }

            public Person(uint id, IEnumerable<PotentialSkeleton> skeletons)
            {
                this.Id = id;
                this.PotentialSkeletons = skeletons;
            }

            public TrackingSkeleton GetSkeletonInFOV(KinectFOV fov)
            {
                foreach (PotentialSkeleton pSkeleton in this.PotentialSkeletons)
                {
                    if (pSkeleton.FOV.Equals(fov))
                    {
                        return pSkeleton.Skeleton;
                    }
                }
                return null;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[Skeletons: ").Append(this.PotentialSkeletons.Count()).Append("]: ");
                String prefix = "";
                foreach (PotentialSkeleton match in this.PotentialSkeletons)
                {
                    sb.Append(prefix);
                    prefix = ",";
                    sb.Append(match);
                }
                return sb.ToString();
            }

            public static Person Copy(Person person)
            {
                List<PotentialSkeleton> skeletonsCopy = new List<PotentialSkeleton>();
                foreach (PotentialSkeleton skeleton in person.PotentialSkeletons)
                {
                    skeletonsCopy.Add(PotentialSkeleton.Copy(skeleton));
                }
                return new Person(person.Id, skeletonsCopy);
            }
        }
    }
}
