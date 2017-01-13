using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public class AugmenterConfiguration
	{
		public bool Built { get; private set; }

		internal List<TypeConfiguration> TypeConfigurations { get; } = new List<TypeConfiguration>();

		public Func<IDictionary<string, object>, IServiceProvider, Task> ConfigureGlobalState { get; set; }

		public void Configure<T>(Action<TypeConfiguration<T>> configure)
		{
			var type = typeof(T);
			var typeConfiguration = TypeConfigurations.FirstOrDefault(c => c.Type == type) as TypeConfiguration<T>;
			if (typeConfiguration == null)
			{
				typeConfiguration = new TypeConfiguration<T>();
				TypeConfigurations.Add(typeConfiguration);
			}

			configure?.Invoke(typeConfiguration);
		}

		public void Build()
		{
			if (Built)
			{
				return;
			}

			Built = true;
			BuildCore();
		}

		private void BuildCore()
		{
			foreach (var typeConfiguration in TypeConfigurations)
			{
				var type = typeConfiguration.Type;

				var baseTypeConfigurations = typeConfiguration.BaseTypeConfigurations;
				var baseTypes = ReflectionHelper.IncludeBaseTypes(type);
				foreach (var baseType in baseTypes)
				{
					var tc = TypeConfigurations.Where(t => t.Type == baseType).FirstOrDefault();
					if (tc != null)
					{
						baseTypeConfigurations.Add(tc);
					}
				}

				var properties = type.GetTypeInfo().DeclaredProperties;
				foreach (var p in properties)
				{
					if (!ReflectionHelper.IsPrimitive(p.PropertyType))
					{
						var nestedTypeConfiguration = TypeConfigurations.FirstOrDefault(c => c.Type == p.PropertyType);
						if (nestedTypeConfiguration != null)
						{
							typeConfiguration.NestedTypeConfigurations[p] = nestedTypeConfiguration;
						}
					}
				}
			}
		}
	}
}
