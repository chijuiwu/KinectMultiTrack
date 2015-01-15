using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Tiny
{
    class Person
    {
        private double initAngle;
        private WorldCoordinate initPosition;

        public Person()
        {
        }

        public double InitialAngle
        {
            get
            {
                return this.initAngle;
            }
            set
            {
                this.initAngle = value;
            }
        }

        public WorldCoordinate InitialPosition
        {
            get
            {
                return this.initPosition;
            }
            set
            {
                this.InitialPosition = value;
            }
        }

        //public SerializableBodyFrame LastKinectFrame
        //{
        //    get
        //    {
        //        if (this.processedBodyFrames.Count > 0)
        //        {
        //            Tuple<SerializableBodyFrame, WorldView> lastKinectFrameTuple;
        //            this.processedBodyFrames.TryPeek(out lastKinectFrameTuple);
        //            return lastKinectFrameTuple.Item1;
        //        }
        //        else if (this.calibrationBodyFrames.Count > 0)
        //        {
        //            SerializableBodyFrame lastKinectFrame;
        //            this.calibrationBodyFrames.TryPeek(out lastKinectFrame);
        //            return lastKinectFrame;
        //        }
        //        else if (this.incomingBodyFrames.Count > 0)
        //        {
        //            SerializableBodyFrame lastKinectFrame;
        //            this.incomingBodyFrames.TryPeek(out lastKinectFrame);
        //            return lastKinectFrame;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public WorldView LastWorldView
        //{
        //    get
        //    {
        //        if (this.processedBodyFrames.Count > 0)
        //        {

        //            Tuple<SerializableBodyFrame, WorldView> lastKinectFrameTuple;
        //            this.processedBodyFrames.TryPeek(out lastKinectFrameTuple);
        //            return lastKinectFrameTuple.Item2;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}
    }
}
