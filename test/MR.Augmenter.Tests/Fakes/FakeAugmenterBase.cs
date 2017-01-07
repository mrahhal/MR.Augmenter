using System.Collections.Generic;

namespace MR.Augmenter
{
	public class FakeAugmenterBase : AugmenterBase
	{
		public FakeAugmenterBase(
				AugmenterConfiguration configuration)
				: base(configuration)
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
