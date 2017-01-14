using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MR.Augmenter
{
	/// <summary>
	/// The base class of <see cref="IAugmenter"/>.
	/// </summary>
	public abstract class AugmenterBase : IAugmenter
	{
		private ConcurrentDictionary<Type, TypeConfiguration> _cache
			= new ConcurrentDictionary<Type, TypeConfiguration>();

		private IReadOnlyDictionary<string, object> _emptyDictionary =
			new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

		private TypeConfigurationBuilder _builder;

		public AugmenterBase(
			AugmenterConfiguration configuration,
			IServiceProvider services)
		{
			if (!configuration.Built)
			{
				throw new InvalidOperationException("AugmenterConfiguration should be built first using Build().");
			}

			Configuration = configuration;
			Services = services;
			_builder = new TypeConfigurationBuilder(configuration.TypeConfigurations);
		}

		public AugmenterConfiguration Configuration { get; }

		public IServiceProvider Services { get; }

		public virtual async Task<object> AugmentAsync<T>(
			T obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<Dictionary<string, object>> addState = null)
		{
			if (obj == null)
			{
				return null;
			}

			var type = obj.GetType();
			var typeConfiguration = ResolveTypeConfiguration(type);

			if (typeConfiguration == null && configure == null)
			{
				return obj;
			}

			var state = await CreateDictionaryAndAddStateAsync(addState);
			var context = new AugmentationContext(obj, typeConfiguration, state);

			if (configure != null)
			{
				var ephemeralTypeConfigration = new TypeConfiguration<T>();
				configure(ephemeralTypeConfigration);
				context.EphemeralTypeConfiguration = ephemeralTypeConfigration;
			}

			return AugmentCore(context);
		}

		private async Task<IReadOnlyDictionary<string, object>> CreateDictionaryAndAddStateAsync(
			Action<Dictionary<string, object>> addState)
		{
			var dictionary = new Dictionary<string, object>();

			if (Configuration.ConfigureGlobalState != null)
			{
				var task = Configuration.ConfigureGlobalState(dictionary, Services);
				if (task != null)
				{
					await task;
				}
			}

			addState?.Invoke(dictionary);

			return new ReadOnlyDictionary<string, object>(dictionary);
		}

		private TypeConfiguration ResolveTypeConfiguration(Type type)
		{
			return _cache.GetOrAdd(type, t =>
			{
				var typeConfiguration = Configuration.TypeConfigurations
					.Where(c => type == c.Type)
					.FirstOrDefault();

				if (typeConfiguration == null)
				{
					typeConfiguration = _builder.Build(null, type);
				}

				return typeConfiguration;
			});
		}

		protected abstract object AugmentCore(AugmentationContext context);

		protected bool ShouldIgnoreAugment(object value)
		{
			return value == AugmentationValue.Ignore;
		}
	}
}
