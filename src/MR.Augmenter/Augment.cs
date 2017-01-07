using System;

namespace MR.Augmenter
{
	public class Augment
	{
		internal Augment(string name, AugmentKind kind)
			: this(name, kind, null)
		{
		}

		public Augment(string name, AugmentKind kind, Func<object, object> valueFunc)
		{
			Name = name;
			Kind = kind;
			ValueFunc = valueFunc;
		}

		public string Name { get; }

		public AugmentKind Kind { get; }

		public Func<object, object> ValueFunc { get; protected set; }
	}
}
