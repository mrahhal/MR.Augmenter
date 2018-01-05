using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MR.Augmenter
{
	public class AugmenterConfiguration
	{
		public bool Built { get; private set; }

		internal List<TypeConfiguration> TypeConfigurations { get; } = new List<TypeConfiguration>();

		internal List<Assembly> Assemblies { get; } = new List<Assembly>();

		public Func<IState, IServiceProvider, Task> ConfigureGlobalState { get; set; }

		public void AddAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			Assemblies.Add(assembly);
		}

		public void Configure<T>(Action<TypeConfiguration<T>> configure)
		{
			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			if (Built)
			{
				throw new InvalidOperationException("The configuration has already been built.");
			}

			var type = typeof(T);
			var typeConfiguration = TypeConfigurations.FirstOrDefault(c => c.Type == type) as TypeConfiguration<T>;
			if (typeConfiguration == null)
			{
				typeConfiguration = new TypeConfiguration<T>();
				TypeConfigurations.Add(typeConfiguration);
			}

			configure(typeConfiguration);
		}

		public void Build()
		{
			if (Built)
			{
				return;
			}

			Built = true;

			CollectAssemblyDefinedTypeConfigurations();
			BuildCore();
		}

		private void CollectAssemblyDefinedTypeConfigurations()
		{
			if (!Assemblies.Any())
			{
				return;
			}

			foreach (var assembly in Assemblies)
			{
				var typeConfigurationTypeInfo = typeof(TypeConfiguration).GetTypeInfo();
				var types = assembly.ExportedTypes
					.Select(t => t.GetTypeInfo())
					.Where(t => !t.IsAbstract && typeConfigurationTypeInfo.IsAssignableFrom(t))
					.ToList();

				foreach (var type in types)
				{
					var baseTypeConfigurationType = FindGenericBaseTypeConfigurationType(type);
					if (baseTypeConfigurationType == null)
					{
						throw new InvalidOperationException("You should extend from the generic version of TypeConfiguration.");
					}

					var instance = (TypeConfiguration)Activator.CreateInstance(type.AsType());
					var elementType = GetElementTypeFromConcreteTypeConfiguration(baseTypeConfigurationType);
					var existing = TypeConfigurations.FirstOrDefault(tc => tc.Type == elementType);
					if (existing != null)
					{
						existing.Combine(instance);
					}
					else
					{
						TypeConfigurations.Add(instance);
					}
				}
			}
		}

		private Type GetElementTypeFromConcreteTypeConfiguration(Type type)
		{
			return type.GetGenericArguments()[0];
		}

		private Type FindGenericBaseTypeConfigurationType(Type type)
		{
			// Find TypeConfiguration<> in the base hierarchy.
			var baseType = type.BaseType;
			while (!baseType.IsConstructedGenericType || baseType.GetGenericTypeDefinition() != typeof(TypeConfiguration<>))
			{
				baseType = baseType.BaseType;
				if (baseType == null)
				{
					return null;
				}
			}

			return baseType;
		}

		private void BuildCore()
		{
			var builder = new TypeConfigurationBuilder(TypeConfigurations);
			foreach (var typeConfiguration in TypeConfigurations)
			{
				builder.Build(typeConfiguration, typeConfiguration.Type);
			}
		}
	}
}
