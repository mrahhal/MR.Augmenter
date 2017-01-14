using System;
using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter
{
	public static class AugmenterServiceCollectionExtensions
	{
		public static void AddAugmenter(
			this IServiceCollection services,
			Action<AugmenterConfiguration> configure)
		{
			services.AddScoped<IAugmenter, JsonAugmenter>();

			var configuration = new AugmenterConfiguration();
			configure(configuration);
			configuration.Build();
			services.AddSingleton(configuration);
		}
	}
}
