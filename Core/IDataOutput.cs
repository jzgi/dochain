﻿using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace Greatbone.Core
{
    ///
    /// Represents a sink for data output.
    ///
    public interface IDataOutput<out R> where R : IDataOutput<R>
    {
        R PutNull(string name);

        R PutRaw(string name, string raw);

        R Put(string name, bool v, string Label = null, bool Required = false, Func<bool, string> Options = null);

        R Put(string name, short v, string Label = null, string Placeholder = null, short Max = 0, short Min = 0, short Step = 0, bool ReadOnly = false, bool Required = false, IDictionary<short, string> Options = null);

        R Put(string name, int v, string Label = null, string Placeholder = null, int Max = 0, int Min = 0, int Step = 0, bool ReadOnly = false, bool Required = false, IDictionary<int, string> Options = null);

        R Put(string name, long v, string Label = null, string Placeholder = null, long Max = 0, long Min = 0, long Step = 0, bool ReadOnly = false, bool Required = false, IDictionary<long, string> Options = null);

        R Put(string name, double v, string Label = null, string Placeholder = null, double Max = 0, double Min = 0, double Step = 0, bool ReadOnly = false, bool Required = false);

        R Put(string name, decimal v, string Label = null, string Placeholder = null, decimal Max = 0, decimal Min = 0, decimal Step = 0, bool ReadOnly = false, bool Required = false);

        R Put(string name, JNumber v);

        R Put(string name, DateTime v, string Label = null, DateTime Max = default(DateTime), DateTime Min = default(DateTime), int Step = 0, bool ReadOnly = false, bool Required = false);

        R Put(string name, NpgsqlPoint v);

        R Put(string name, char[] v);

        R Put(string name, string v, string Label = null, string Placeholder = null, string Pattern = null, short Max = 0, short Min = 0, bool ReadOnly = false, bool Required = false, IDictionary<string, string> Options = null);

        R Put(string name, byte[] v);

        R Put(string name, ArraySegment<byte> v);

        R Put(string name, short[] v, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false, IDictionary<short, string> Options = null);

        R Put(string name, int[] v, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false, IDictionary<int, string> Options = null);

        R Put(string name, long[] v, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false, IDictionary<long, string> Options = null);

        R Put(string name, string[] v, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false, IDictionary<string, string> Options = null);

        R Put(string name, Dictionary<string, string> v, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false);

        R Put(string name, IData v, int proj = 0, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false);

        R Put<D>(string name, D[] v, int proj = 0, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false) where D : IData;

        R Put<D>(string name, List<D> v, int proj = 0, string Label = null, string Placeholder = null, bool ReadOnly = false, bool Required = false) where D : IData;

        R Put(string name, IDataInput v);
    }
}