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
			Action<AugmenterConfiguration> configure = null)
		{
			services.AddOptions();

			services.AddScoped<IAugmenter, Augmenter>();

			if (configure != null)
			{
				services.Configure(configure);
			}

			services.PostConfigure<AugmenterConfiguration>(configuration => configuration.Build());

			return new AugmenterBuilder(services);
		}
	}
}
