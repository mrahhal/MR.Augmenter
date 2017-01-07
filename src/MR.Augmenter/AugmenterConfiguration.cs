using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MR.Augmenter
{
	public class AugmenterConfiguration
	{
		internal List<TypeConfiguration> TypeConfigurations { get; } = new List<TypeConfiguration>();

		public bool Built { get; private set; }

		public void Configure<T>(Action<TypeConfiguration<T>> configuration)
		{
			var type = typeof(T);
			var typeConfiguration = TypeConfigurations.FirstOrDefault(c => c.Type == type) as TypeConfiguration<T>;
			if (typeConfiguration == null)
			{
				typeConfiguration = new TypeConfiguration<T>();
				TypeConfigurations.Add(typeConfiguration);
			}

			configuration(typeConfiguration);
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
				var properties = type.GetTypeInfo().DeclaredProperties;
				foreach (var p in properties)
				{
					if (!p.PropertyType.GetTypeInfo().IsPrimitive && !typeConfiguration.Augments.Any(a => a.Name == p.Name))
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
