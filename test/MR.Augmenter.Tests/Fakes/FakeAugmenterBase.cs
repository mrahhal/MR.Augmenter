using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace MR.Augmenter
{
	public class FakeAugmenterBase : AugmenterBase
	{
		public FakeAugmenterBase(
			IOptions<AugmenterConfiguration> configuration,
			IServiceProvider services)
			: base(configuration, services)
		{
		}

		public List<AugmentationContext> Contexts { get; } = new List<AugmentationContext>();

		protected override object AugmentCore(AugmentationContext context)
		{
			return AugmentCorePublic(context);
		}

		public virtual object AugmentCorePublic(AugmentationContext context)
		{
			Contexts.Add(context);
			return context.Object;
		}
	}
}
