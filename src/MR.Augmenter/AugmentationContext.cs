using System;
using System.Collections.Generic;

namespace MR.Augmenter
{
	public class AugmentationContext
	{
		public AugmentationContext()
		{
		}

		public AugmentationContext(object obj, TypeConfiguration typeConfiguration, IReadOnlyDictionary<string, object> state)
		{
			Object = obj;
			Type = obj.GetType();
			TypeConfiguration = typeConfiguration;
			State = state;
		}

		public object Object { get; }

		public Type Type { get; }

		public TypeConfiguration TypeConfiguration { get; }

		public TypeConfiguration EphemeralTypeConfiguration { get; set; }

		public IReadOnlyDictionary<string, object> State { get; }
	}
}
