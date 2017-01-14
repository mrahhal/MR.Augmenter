using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MR.Augmenter
{
	[DebuggerDisplay("{Kind}: {Name}")]
	public class Augment
	{
		internal Augment(string name, AugmentKind kind)
			: this(name, kind, null)
		{
		}

		public Augment(string name, AugmentKind kind, Func<object, IReadOnlyDictionary<string, object>, object> valueFunc)
		{
			Name = name;
			Kind = kind;
			ValueFunc = valueFunc;
		}

		public string Name { get; }

		public AugmentKind Kind { get; }

		public Func<object, IReadOnlyDictionary<string, object>, object> ValueFunc { get; protected set; }
	}
}
