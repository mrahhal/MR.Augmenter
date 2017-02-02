using System;

namespace MR.Augmenter
{
	public abstract class AugmenterWrapper
	{
		protected AugmenterWrapper(object obj)
		{
			Object = obj;
		}

		public object Object { get; }

		public TypeConfiguration TypeConfiguration { get; protected set; }

		public Action<object, IState> AddState { get; protected set; }
	}

	public class AugmenterWrapper<T> : AugmenterWrapper
	{
		public AugmenterWrapper(object obj)
			: base(obj)
		{
		}

		public new T Object => (T)base.Object;

		[Obsolete("Use SetTypeConfiguration instead.")]
		public void SetConfiguration(Action<TypeConfiguration<T>> configure)
		{
			SetTypeConfiguration(configure);
		}

		public void SetTypeConfiguration(Action<TypeConfiguration<T>> configure)
		{
			var typeConfiguration = new TypeConfiguration<T>();
			configure(typeConfiguration);
			TypeConfiguration = typeConfiguration;
		}

		public void SetAddState(Action<T, IState> addState)
		{
			AddState = (x, s) =>
			{
				var concrete = (T)x;
				addState(concrete, s);
			};
		}

		public static AugmenterWrapper<T> Create(T obj) => new AugmenterWrapper<T>(obj);
	}
}
