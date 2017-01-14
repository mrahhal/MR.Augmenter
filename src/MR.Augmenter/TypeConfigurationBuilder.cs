using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public class TypeConfigurationBuilder
	{
		private List<TypeConfiguration> _all;

		public TypeConfigurationBuilder(List<TypeConfiguration> all)
		{
			_all = all;
		}

		public TypeConfiguration Build(TypeConfiguration typeConfiguration, Type type)
		{
			if (typeConfiguration == null && type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (typeConfiguration != null && typeConfiguration.Type != type)
			{
				throw new ArgumentException("type should be the same as typeConfiguration.Type.");
			}

			var context = new Context(typeConfiguration, type);
			BuildOne(context, type);
			return context.Current;
		}

		private void BuildOne(Context context, Type type)
		{
			if (context.Current != null && context.Current.Built)
			{
				return;
			}

			var baseTypes = ReflectionHelper.IncludeBaseTypes(type);
			foreach (var baseType in baseTypes)
			{
				var tc = _all.FirstOrDefault(t => t.Type == baseType);
				if (tc != null)
				{
					context.EnsureCurrent();
					context.Current.BaseTypeConfigurations.Add(tc);
				}
				else
				{
					var scoped = context.CreateScoped(null, baseType);
					BuildOne(scoped, baseType);
					if (!scoped.Empty)
					{
						context.EnsureCurrent();
						context.Current.BaseTypeConfigurations.Add(scoped.Current);
					}
				}
			}

			var properties = type.GetTypeInfo().DeclaredProperties;
			foreach (var p in properties)
			{
				if (!ReflectionHelper.IsPrimitive(p.PropertyType))
				{
					var nestedTypeConfiguration = _all.FirstOrDefault(c => c.Type == p.PropertyType);
					var scoped = context.CreateScoped(nestedTypeConfiguration, p.PropertyType);
					BuildOne(scoped, p.PropertyType);
					if (!scoped.Empty)
					{
						context.EnsureCurrent();
						context.Current.NestedTypeConfigurations[p] = scoped.Current;
					}
				}
			}
		}

		private class Context
		{
			private Context()
			{
			}

			public Context(TypeConfiguration current, Type type)
			{
				Current = current;
				Type = type;
			}

			public TypeConfiguration Current { get; private set; }

			public Type Type { get; private set; }

			public bool Empty => Current == null;

			public void EnsureCurrent()
			{
				Current = Current ?? new TypeConfiguration(Type);
				Current.Built = true;
			}

			public Context CreateScoped(TypeConfiguration current, Type type)
			{
				return new Context
				{
					Current = current,
					Type = type
				};
			}
		}
	}
}
