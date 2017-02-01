using System;
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

		public Augment(string name, AugmentKind kind, Func<object, IReadOnlyState, object> valueFunc)
		{
			Name = name;
			Kind = kind;
			ValueFunc = valueFunc;
		}

		public string Name { get; }

		public AugmentKind Kind { get; }

		public Func<object, IReadOnlyState, object> ValueFunc { get; protected set; }
	}
}
