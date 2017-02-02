using System;

namespace MR.Augmenter
{
	public abstract class NestedTypeConfiguration
	{
		public TypeConfiguration TypeConfiguration { get; protected set; }

		public Action<object, IReadOnlyState, IState> AddState { get; protected set; }
	}

	public class NestedTypeConfiguration<T, TNested> : NestedTypeConfiguration
	{
		public void SetTypeConfiguration(Action<TypeConfiguration<TNested>> configure)
		{
			var tc = new TypeConfiguration<TNested>();
			configure(tc);
			TypeConfiguration = tc;
		}

		public void SetAddState(Action<T, IReadOnlyState, IState> addState)
		{
			AddState = (x, s1, s2) =>
			{
				var concrete = (T)x;
				addState(concrete, s1, s2);
			};
		}
	}
}
