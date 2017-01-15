using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public class NestedTypeConfigurationWrapper
	{
		public NestedTypeConfigurationWrapper(TypeConfiguration typeConfiguration, TypeInfoWrapper typeInfoWrapper)
		{
			TypeConfiguration = typeConfiguration;
			TypeInfoWrapper = typeInfoWrapper;
		}

		public TypeConfiguration TypeConfiguration { get; private set; }

		internal TypeInfoWrapper TypeInfoWrapper { get; set; }
	}
}
