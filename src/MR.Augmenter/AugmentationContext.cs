using System;
using System.Collections.Generic;

namespace MR.Augmenter
{
	public class AugmentationContext
	{
		public AugmentationContext()
		{
		}

		public AugmentationContext(object obj, List<TypeConfiguration> typeConfigurations, IReadOnlyDictionary<string, object> state)
		{
			Object = obj;
			Type = obj.GetType();
			TypeConfigurations = typeConfigurations;
			State = state;
		}

		public object Object { get; }

		public Type Type { get; }

		public List<TypeConfiguration> TypeConfigurations { get; }

		public IReadOnlyDictionary<string, object> State { get; }
	}
}
