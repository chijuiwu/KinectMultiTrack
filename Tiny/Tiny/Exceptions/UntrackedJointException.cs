﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMultiTrack.Exceptions
{
    public class UntrackedJointException : Exception
    {
        public UntrackedJointException(string message)
            : base(message)
        {
        }

        public UntrackedJointException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}