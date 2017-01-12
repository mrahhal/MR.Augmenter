using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace MR.Augmenter
{
	/// <summary>
	/// The base class of <see cref="IAugmenter"/>.
	/// </summary>
	public abstract class AugmenterBase : IAugmenter
	{
		private ConcurrentDictionary<Type, List<TypeConfiguration>> _cache
			= new ConcurrentDictionary<Type, List<TypeConfiguration>>();
		private IReadOnlyDictionary<string, object> _emptyDictionary =
			new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

		public AugmenterBase(AugmenterConfiguration configuration)
		{
			configuration.Build();
			Configuration = configuration;
		}

		public AugmenterConfiguration Configuration { get; }

		public virtual object Augment<T>(
			T obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<Dictionary<string, object>> addState = null)
		{
			if (obj == null)
			{
				return null;
			}

			var type = obj.GetType();
			var typeConfigurations = ResolveTypeConfigurations(type);

			if (typeConfigurations == null && configure == null)
			{
				return obj;
			}

			typeConfigurations =
				typeConfigurations == null ?
				new List<TypeConfiguration>() :
				new List<TypeConfiguration>(typeConfigurations);

			if (configure != null)
			{
				var localTypeConfigration = new TypeConfiguration<T>();
				configure(localTypeConfigration);
				typeConfigurations.Add(localTypeConfigration);
			}

			var state =
				addState == null ?
				_emptyDictionary :
				CreateDictionaryAndAddState(addState);
			var context = new AugmentationContext(obj, typeConfigurations, state);
			return AugmentCore(context);
		}

		private IReadOnlyDictionary<string, object> CreateDictionaryAndAddState(
			Action<Dictionary<string, object>> addState)
		{
			var dictionary = new Dictionary<string, object>();
			addState(dictionary);
			return new ReadOnlyDictionary<string, object>(dictionary);
		}

		private List<TypeConfiguration> ResolveTypeConfigurations(Type type)
		{
			return _cache.GetOrAdd(type, t =>
			{
				var types = IncludeBaseTypes(type);
				var typeConfigurations = Configuration.TypeConfigurations
					.Where(c => types.Contains(c.Type))
					.ToList();

				if (!typeConfigurations.Any())
				{
					// Check if there are complex members anyway (as in the case of anon objects).
					var properties = type.GetTypeInfo().DeclaredProperties;
					foreach (var p in properties)
					{
						if (!p.PropertyType.GetTypeInfo().IsPrimitive)
						{
							var nestedTypeConfiguration = Configuration.TypeConfigurations
								.FirstOrDefault(c => c.Type == p.PropertyType);
							if (nestedTypeConfiguration != null)
							{
								typeConfigurations.Add(nestedTypeConfiguration);
							}
						}
					}

					if (!typeConfigurations.Any())
					{
						return null;
					}
				}

				return typeConfigurations;
			});
		}

		private List<Type> IncludeBaseTypes(Type type)
		{
			var list = new List<Type>();
			list.Add(type);

			TypeInfo pivot = type.GetTypeInfo();
			while (true)
			{
				pivot = pivot.BaseType.GetTypeInfo();
				if (pivot.AsType() == typeof(object))
				{
					break;
				}

				list.Add(pivot.AsType());
			}

			list.Reverse();
			return list;
		}

		protected abstract object AugmentCore(AugmentationContext context);

		protected bool ShouldIgnoreAugment(object value)
		{
			return value == AugmentationValue.Ignore;
		}
	}
}
