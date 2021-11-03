﻿using System;

namespace SkyChain.Chain
{
    /// <summary>
    /// Mark version of chain operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class VerAttribute : Attribute
    {
        readonly short major;

        protected VerAttribute(short major)
        {
            this.major = major;
        }
    }
}