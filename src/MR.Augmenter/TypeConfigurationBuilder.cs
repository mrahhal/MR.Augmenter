﻿using System;
using System.Collections;
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

		public TypeConfiguration Build(TypeConfiguration typeConfiguration, Type type, bool alwaysBuild = false)
		{
			if (typeConfiguration == null && type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (typeConfiguration != null && typeConfiguration.Type != type)
			{
				throw new ArgumentException("type should be the same as typeConfiguration.Type.");
			}

			if (ReflectionHelper.IsPrimitive(type))
			{
				return null;
			}

			var context = new Context(
				alwaysBuild ? (typeConfiguration ?? new TypeConfiguration(type)) : typeConfiguration,
				type);
			BuildOne(context, type);
			return context.Current;
		}

		private void BuildOne(Context context, Type type)
		{
			if (context.Current != null && context.Current.Built)
			{
				return;
			}

			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
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
					context.AddBaseTypeConfiguration(tc);
				}
				else
				{
					var scoped = context.CreateScoped(null, baseType);
					BuildOne(scoped, baseType);
					if (!scoped.Empty)
					{
						context.EnsureCurrent();
						context.AddBaseTypeConfiguration(scoped.Current);
					}
					else
					{
						context.AddBaseTypeConfiguration(CreateConfigurationWithPropertiesOnly(baseType));
					}
				}
			}

			var properties = type.GetTypeInfo().DeclaredProperties;
			foreach (var p in properties)
			{
				if (p.GetMethod.IsStatic)
				{
					continue;
				}

				var tiw = TypeInfoResolver.ResolveTypeInfo(p.PropertyType);

				if (tiw.IsPrimitive)
				{
					context.Properties.Add(new APropertyInfo(p, tiw, null));
				}
				else if (tiw.Type == type)
				{
					// Detect self referencing type.

					context.EnsureCurrent();
					context.Properties.Add(new APropertyInfo(p, tiw, context.Current));
				}
				else
				{
					var nestedTypeConfiguration = _all.FirstOrDefault(c => c.Type == tiw.Type);
					var scoped = context.CreateScoped(
						tiw.IsWrapper ? (nestedTypeConfiguration ?? new TypeConfiguration(type)) : nestedTypeConfiguration,
						tiw.Type);

					BuildOne(scoped, tiw.Type);
					if (!scoped.Empty)
					{
						context.EnsureCurrent();
						context.Properties.Add(new APropertyInfo(p, tiw, scoped.Current));
					}
					else
					{
						context.Properties.Add(new APropertyInfo(p, tiw, null));
					}
				}
			}

			if (!context.Empty)
			{
				context.EnsureCurrent();
				context.Current.Properties.AddRange(context.Properties);
			}
		}

		private TypeConfiguration CreateConfigurationWithPropertiesOnly(Type baseType)
		{
			var tc = new TypeConfiguration(baseType);
			foreach (var pi in baseType.GetTypeInfo().DeclaredProperties)
			{
				tc.Properties.Add(
					new APropertyInfo(pi, TypeInfoResolver.ResolveTypeInfo(pi.PropertyType), null));
			}
			return tc;
		}

		private class Context
		{
			private List<TypeConfiguration> _typeConfigurations = new List<TypeConfiguration>();

			private Context()
			{
			}

			public Context(TypeConfiguration current, Type type)
			{
				Current = current;
				Type = type;
			}

			public List<APropertyInfo> Properties { get; private set; } = new List<APropertyInfo>();

			public TypeConfiguration Current { get; private set; }

			public Type Type { get; private set; }

			public bool Empty => Current == null;

			public void EnsureCurrent()
			{
				if (Current != null)
				{
					Current.Built = true;
				}

				if (!Empty)
				{
					return;
				}

				Current = new TypeConfiguration(Type);
				Current.Built = true;
				Current.BaseTypeConfigurations.AddRange(_typeConfigurations);
			}

			public void AddBaseTypeConfiguration(TypeConfiguration tc)
			{
				if (Empty)
				{
					_typeConfigurations.Add(tc);
				}
				else
				{
					Current.BaseTypeConfigurations.Add(tc);
				}
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
