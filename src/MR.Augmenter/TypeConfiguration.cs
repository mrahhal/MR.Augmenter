using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MR.Augmenter
{
	[DebuggerDisplay("Augments: {Augments.Count}, Base: {BaseTypeConfigurations.Count}, Properties: {Properties.Count}")]
	public class TypeConfiguration
	{
		public TypeConfiguration(Type type)
		{
			Type = type;
		}

		public Type Type { get; }

		public bool Built { get; internal set; }

		internal List<Augment> Augments { get; } = new List<Augment>();

		internal List<TypeConfiguration> BaseTypeConfigurations { get; } = new List<TypeConfiguration>();

		internal List<APropertyInfo> Properties { get; } = new List<APropertyInfo>();
	}

	public class TypeConfiguration<T> : TypeConfiguration
	{
		public TypeConfiguration()
			: base(typeof(T))
		{
		}

		public void ConfigureAdd(string name, Func<T, IReadOnlyState, object> valueFunc)
		{
			Augments.Add(new Augment(name, AugmentKind.Add, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc(concrete, state);
			}));
		}

		public void ConfigureRemove(string name, Func<T, IReadOnlyState, object> valueFunc = null)
		{
			Augments.Add(new Augment(name, AugmentKind.Remove, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc?.Invoke(concrete, state);
			}));
		}
	}
}
