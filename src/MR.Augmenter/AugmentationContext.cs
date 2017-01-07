using System;
using System.Collections.Generic;

namespace MR.Augmenter
{
	public class AugmentationContext
	{
		public AugmentationContext()
		{
		}

		public AugmentationContext(object obj, List<TypeConfiguration> typeConfigurations)
		{
			Object = obj;
			Type = obj.GetType();
			TypeConfigurations = typeConfigurations;
		}

		public object Object { get; }

		public Type Type { get; }

		public List<TypeConfiguration> TypeConfigurations { get; }
	}
}
