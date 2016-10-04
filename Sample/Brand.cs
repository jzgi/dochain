﻿using Greatbone.Core;

namespace Greatbone.Sample
{
    public class Brand : IPersist
    {
        public string Id;

        public string Name;

        public char[] Credential { get; set; }

        public long ModifiedOn { get; set; }

        public string Key => Id;

        public void Load(ISource sc)
        {
        }

        public void Save<R>(ISink<R> sk) where R : ISink<R>
        {
        }
    }
}