using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MR.Augmenter
{
	[DebuggerDisplay("Augments: {Augments.Count}, Base: {BaseTypeConfigurations.Count}, Nested: {NestedTypeConfigurations.Count}")]
	public class TypeConfiguration
	{
		public TypeConfiguration(Type type)
		{
			Type = type;
		}

		public bool Built { get; internal set; }

		public Type Type { get; }

		internal List<Augment> Augments { get; } = new List<Augment>();

		public List<TypeConfiguration> BaseTypeConfigurations { get; } = new List<TypeConfiguration>();

		public Dictionary<PropertyInfo, NestedTypeConfigurationWrapper> NestedTypeConfigurations { get; } = new Dictionary<PropertyInfo, NestedTypeConfigurationWrapper>();
	}

	public class TypeConfiguration<T> : TypeConfiguration
	{
		public TypeConfiguration()
			: base(typeof(T))
		{
		}

		public void ConfigureAdd(string name, Func<T, IReadOnlyDictionary<string, object>, object> valueFunc)
		{
			Augments.Add(new Augment(name, AugmentKind.Add, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc(concrete, state);
			}));
		}

		public void ConfigureRemove(string name, Func<T, IReadOnlyDictionary<string, object>, object> valueFunc = null)
		{
			Augments.Add(new Augment(name, AugmentKind.Remove, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc?.Invoke(concrete, state);
			}));
		}
	}
}
