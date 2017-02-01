using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MR.Augmenter
{
	public class AugmenterConfiguration
	{
		public bool Built { get; private set; }

		internal List<TypeConfiguration> TypeConfigurations { get; } = new List<TypeConfiguration>();

		public Func<IState, IServiceProvider, Task> ConfigureGlobalState { get; set; }

		public void Configure<T>(Action<TypeConfiguration<T>> configure)
		{
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
			var builder = new TypeConfigurationBuilder(TypeConfigurations);
			foreach (var typeConfiguration in TypeConfigurations)
			{
				builder.Build(typeConfiguration, typeConfiguration.Type);
			}
		}
	}
}
