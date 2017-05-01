using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	[DebuggerDisplay("Augments: {Augments.Count}, Base: {BaseTypeConfigurations.Count}, Properties: {Properties.Count}")]
	public class TypeConfiguration
	{
		private static readonly List<Action<object, Dictionary<string, object>>> EmptyThunkList =
			new List<Action<object, Dictionary<string, object>>>();

		public TypeConfiguration(Type type)
		{
			Type = type;
		}

		public Type Type { get; }

		public bool Built { get; internal set; }

		internal List<Augment> Augments { get; } = new List<Augment>();

		internal List<Action<object, Dictionary<string, object>>> CustomThunks = EmptyThunkList;

		internal void AddCustomThunk(Action<object, Dictionary<string, object>> thunk)
		{
			if (CustomThunks == EmptyThunkList) CustomThunks = new List<Action<object, Dictionary<string, object>>>();
			CustomThunks.Add(thunk);
		}

		internal List<TypeConfiguration> BaseTypeConfigurations { get; } = new List<TypeConfiguration>();

		internal List<APropertyInfo> Properties { get; } = new List<APropertyInfo>();

		internal Lazy<Dictionary<PropertyInfo, NestedTypeConfiguration>> NestedConfigurations { get; } =
			new Lazy<Dictionary<PropertyInfo, NestedTypeConfiguration>>();
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
				return predicate(x, state) ? valueFunc(x, state) : AugmentationValue.Ignore;
			});
		}

		[Obsolete("Use Add instead.")]
		public void ConfigureAdd(string name, Func<T, IReadOnlyState, object> valueFunc)
		{
			Add(name, valueFunc);
		}

		public void Add(string name, Func<T, IReadOnlyState, object> valueFunc)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			Augments.Add(new Augment(name, AugmentKind.Add, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc(concrete, state);
			}));
		}

		public void ConfigureNested<TNested>(
			Expression<Func<T, TNested>> nestedMemberExpression,
			Action<NestedTypeConfiguration<T, TNested>> configure)
			where TNested : class
		{
			ConfigureNestedInternal(nestedMemberExpression, configure);
		}

		public void ConfigureNestedArray<TNested>(
			Expression<Func<T, IEnumerable<TNested>>> nestedMemberExpression,
			Action<NestedTypeConfiguration<T, TNested>> configure)
			where TNested : class
		{
			ConfigureNestedInternal(nestedMemberExpression, configure);
		}

		private void ConfigureNestedInternal<TNested>(
			LambdaExpression nestedMemberExpression,
			Action<NestedTypeConfiguration<T, TNested>> configure)
			where TNested : class
		{
			var pi = nestedMemberExpression.GetSimplePropertyAccess();
			var nestedTc = new NestedTypeConfiguration<T, TNested>();
			configure(nestedTc);
			NestedConfigurations.Value.Add(pi, nestedTc);
		}

		[Obsolete("Use Remove instead.")]
		public void ConfigureRemove(string name, Func<T, IReadOnlyState, object> valueFunc = null)
		{
			Remove(name, valueFunc);
		}

		public void Remove(string name, Func<T, IReadOnlyState, object> valueFunc = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			Augments.Add(new Augment(name, AugmentKind.Remove, (obj, state) =>
			{
				var concrete = (T)obj;
				return valueFunc?.Invoke(concrete, state);
			}));
		}

		public void Custom(Action<T, Dictionary<string, object>> customThunk)
		{
			if (customThunk == null) throw new ArgumentNullException(nameof(customThunk));

			AddCustomThunk((obj, d) =>
			{
				var concrete = (T)obj;
				customThunk(concrete, d);
			});
		}

		protected static bool Truthy(string key, IReadOnlyState state)
		{
			if (!state.TryGetValue(key, out object value))
			{
				return false;
			}
			return Truthy(value);
		}

		protected static bool Truthy(object value)
		{
			return value == Boxed.True || value != Boxed.False;
		}
	}
}
