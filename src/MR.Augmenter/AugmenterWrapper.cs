using System;

namespace MR.Augmenter
{
	public abstract class AugmenterWrapper
	{
		public AugmenterWrapper(object obj)
		{
			Object = obj;
		}

		public object Object { get; }

		public TypeConfiguration TypeConfiguration { get; protected set; }
	}

	public class AugmenterWrapper<T> : AugmenterWrapper
	{
		public AugmenterWrapper(object obj)
			: base(obj)
		{
		}

		public new T Object => (T)base.Object;

		public void SetConfiguration(Action<TypeConfiguration<T>> configure)
		{
			var typeConfiguration = new TypeConfiguration<T>();
			configure(typeConfiguration);
			TypeConfiguration = typeConfiguration;
		}

		public static AugmenterWrapper<T> Create(T obj) => new AugmenterWrapper<T>(obj);
	}
}
