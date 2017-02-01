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

		public void ExposeIf(string name, string key)
		{
			ExposeIf(name, (x, state) => Truthy(key, state));
		}

		public void ExposeIf(string name, Func<T, IReadOnlyState, bool> predicate)
		{
			Remove(name, (x, state) =>
			{
				return predicate(x, state) ? AugmentationValue.Ignore : null;
			});
		}

		public void AddIf(string name, string key, Func<T, IReadOnlyState, object> valueFunc)
		{
			AddIf(name, (x, state) => Truthy(key, state), valueFunc);
		}

		public void AddIf(string name, Func<T, IReadOnlyState, bool> predicate, Func<T, IReadOnlyState, object> valueFunc)
		{
			Add(name, (x, state) =>
			{
				if (!predicate(x, state))
				{
					return AugmentationValue.Ignore;
				}
				return valueFunc(x, state);
			});
		}

		[Obsolete("Use Add instead.")]
		public void ConfigureAdd(string name, Func<T, IReadOnlyState, object> valueFunc)
		{
			Add(name, valueFunc);
		}

		public void Add(string name, Func<T, IReadOnlyState, object> valueFunc)
		{
			Augments.Add(new Augment(name, AugmentKind.Add, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc(concrete, state);
			}));
		}

		[Obsolete("Use Remove instead.")]
		public void ConfigureRemove(string name, Func<T, IReadOnlyState, object> valueFunc = null)
		{
			Remove(name, valueFunc);
		}

		public void Remove(string name, Func<T, IReadOnlyState, object> valueFunc = null)
		{
			Augments.Add(new Augment(name, AugmentKind.Remove, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc?.Invoke(concrete, state);
			}));
		}

		protected static bool Truthy(string key, IReadOnlyState state)
		{
			object value;
			if (!state.TryGetValue(key, out value))
			{
				return false;
			}
			return value == Boxed.True || value != Boxed.False;
		}
	}
}
