﻿using Greatbone.Core;

namespace Greatbone.Sample
{
	public class Brand : IZone, ISerial
	{
		public string Id;

		public string Name;

		public char[] Credential { get; set; }

		public long ModifiedOn { get; set; }

		public string Key => Id;

		public void From(IReader r)
		{
		}

		public void To(IWriter w)
		{
		}
	}
}