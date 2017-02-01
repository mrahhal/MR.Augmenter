using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MR.Augmenter.Internal;

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

		private Task<object> _nullResultTask = Task.FromResult((object)null);
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

		public virtual Task<object> AugmentAsync<T>(
			T obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<IDictionary<string, object>> addState = null)
		{
			return AugmentCommonAsync(obj, configure, addState);
		}

		public Task<object> AugmentAsync<T>(
			IEnumerable<T> obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<IDictionary<string, object>> addState = null)
		{
			return AugmentCommonAsync(obj, configure, addState);
		}

		// This method exists in order to make calls to AugmentAsync with an array select
		// the right generic method (the enumerable one).
		public Task<object> AugmentAsync<T>(
			T[] obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<IDictionary<string, object>> addState = null)
		{
			return AugmentAsync((IEnumerable<T>)obj, configure, addState);
		}

		private Task<object> AugmentCommonAsync<T>(
			object obj,
			Action<TypeConfiguration<T>> configure,
			Action<IDictionary<string, object>> addState)
		{
			if (obj == null)
			{
				return _nullResultTask;
			}

			var type = obj.GetType();

			if (configure == null)
			{
				return AugmentInternal(
					obj, type,
					addState,
					null,
					null);
			}
			else
			{
				return AugmentInternal(
					obj, type,
					addState,
					(context, state) =>
					{
						var c = state as Action<TypeConfiguration<T>>;
						var ephemeralTypeConfigration = new TypeConfiguration<T>();
						c(ephemeralTypeConfigration);
						context.EphemeralTypeConfiguration = ephemeralTypeConfigration;
					},
					configure);
			}
		}

		private async Task<object> AugmentInternal(
			object obj, Type type,
			Action<IDictionary<string, object>> addState,
			Action<AugmentationContext, object> configure,
			object configureState)
		{
			var tiw = TypeInfoResolver.ResolveTypeInfo(type);
			if (tiw.IsPrimitive)
			{
				return obj;
			}

			var alwaysBuild = tiw.IsWrapper || configure != null;
			var typeConfiguration = ResolveTypeConfiguration(tiw.Type, alwaysBuild);

			if (typeConfiguration == null && configure == null)
			{
				// No configuration
				return obj;
			}

			var state = await CreateDictionaryAndAddStateAsync(addState);
			var context = new AugmentationContext(obj, typeConfiguration, state);

			if (tiw.IsArray)
			{
				var asEnumerable = obj as IEnumerable;
				var list = new List<object>();
				foreach (var item in asEnumerable)
				{
					// We'll reuse the context.
					context.Object = item;
					list.Add(AugmentOne(obj, configure, configureState, tiw, context));
				}
				return list;
			}
			else
			{
				return AugmentOne(obj, configure, configureState, tiw, context);
			}
		}

		private object AugmentOne(
			object obj,
			Action<AugmentationContext, object> configure,
			object configureState,
			TypeInfoWrapper tiw,
			AugmentationContext context)
		{
			if (tiw.IsWrapper)
			{
				context.Object = ((AugmenterWrapper)obj).Object;
				context.EphemeralTypeConfiguration = ((AugmenterWrapper)obj).TypeConfiguration;
			}

			configure?.Invoke(context, configureState);

			return AugmentCore(context);
		}

		private async Task<IReadOnlyDictionary<string, object>> CreateDictionaryAndAddStateAsync(
			Action<IDictionary<string, object>> addState)
		{
			var dictionary = new GracefulDictionary();

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

		private TypeConfiguration ResolveTypeConfiguration(Type type, bool alwaysBuild)
		{
			return _cache.GetOrAdd(type, t =>
			{
				var typeConfiguration = Configuration.TypeConfigurations
					.Where(c => type == c.Type)
					.FirstOrDefault();

				if (typeConfiguration == null)
				{
					typeConfiguration = _builder.Build(null, type, alwaysBuild);
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
