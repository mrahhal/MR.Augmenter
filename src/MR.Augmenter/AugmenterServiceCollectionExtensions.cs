using System;
using Microsoft.Extensions.DependencyInjection;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public static class AugmenterServiceCollectionExtensions
	{
		public static IAugmenterBuilder AddAugmenter(
			this IServiceCollection services,
			Action<AugmenterConfiguration> configure)
		{
			services.AddScoped<IAugmenter, Augmenter>();

			var configuration = new AugmenterConfiguration();
			configure(configuration);
			configuration.Build();
			services.AddSingleton(configuration);

			return new AugmenterBuilder(services);
		}
	}
}
