using System;
using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter.Internal
{
	public class AugmenterBuilder : IAugmenterBuilder
	{
		public AugmenterBuilder(IServiceCollection services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
		}

		public IServiceCollection Services { get; }
	}
}
