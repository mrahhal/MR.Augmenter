using System;
using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter.Internal
{
	public class AugmenterBuilder : IAugmenterBuilder
	{
		public AugmenterBuilder(IServiceCollection services)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}
