using System;
using Microsoft.Extensions.DependencyInjection;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public static class AugmenterServiceCollectionExtensions
	{
		/// <summary>
		/// Adds augmenter to services.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configure">Can be null.</param>
		public static IAugmenterBuilder AddAugmenter(
			this IServiceCollection services,
			Action<AugmenterConfiguration> configure)
		{
			services.AddScoped<IAugmenter, Augmenter>();

			var configuration = new AugmenterConfiguration();
			configure?.Invoke(configuration);
			configuration.Build();
			services.AddSingleton(configuration);

			return new AugmenterBuilder(services);
		}
	}
}
