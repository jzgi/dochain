﻿using System;
using SkyChain.Db;

namespace SkyChain.Chain
{
    public abstract class Activity
    {
        // the parent transaction descriptor
        internal TransactDef parent;

        // ordinal of the step
        internal short step;

        // last validation check
        internal DateTime last;

        public TransactDef Parent => parent;

        public short Step => step;

        public string Name { get; set; }

        public int Cycle { get; set; } = 30;

        public bool IsStart => step == 1;

        public bool IsEnd => step == parent.Size;

        /// <summary>
        /// only called from the validator thread 
        /// </summary>
        internal bool IsNewCycle(DateTime now)
        {
            var elapsed = (now - last).Seconds;
            if (elapsed > Cycle)
            {
                last = now;
                return true;
            }
            return false;
        }
        
        //
        //

        public virtual void OnDefine()
        {
        }

        /// <summary>
        /// Called in an inputter thread.
        /// </summary>
        /// <returns>true indicates succeeded.</returns>
        public virtual bool OnSubmit(ChainContext cc, DbContext dc)
        {
            return true;
        }

        /// <summary>
        /// Called in the validator thread of current chain node.
        /// </summary>
        /// <returns>A value indicating the progress, with a full of 100, a denial of 0</returns>
        public virtual short OnValidate(ChainContext cc)
        {
            return 100;
        }
    }
}