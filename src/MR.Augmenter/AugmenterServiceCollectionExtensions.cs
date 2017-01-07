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
			services.AddSingleton<IAugmenter, JsonAugmenter>();

			var configuration = new AugmenterConfiguration();
			configure(configuration);
			services.AddSingleton(configuration);
		}
	}
}
