using System;

namespace MR.Augmenter
{
	public class AugmenterWrapper
	{
		public AugmenterWrapper(object obj)
		{
			Object = obj;
		}

		public object Object { get; }

		internal TypeConfiguration TypeConfiguration { get; private set; }

		public void SetConfiguration<T>(Action<TypeConfiguration<T>> configure)
		{
			var typeConfiguration = new TypeConfiguration<T>();
			configure(typeConfiguration);
			TypeConfiguration = typeConfiguration;
		}
	}
}
