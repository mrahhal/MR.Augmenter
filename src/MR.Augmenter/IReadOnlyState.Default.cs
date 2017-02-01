using System.Collections.ObjectModel;

namespace MR.Augmenter
{
	public class ReadOnlyState : ReadOnlyDictionary<string, object>, IReadOnlyState
	{
		public ReadOnlyState(IState state) : base(state)
		{
		}
	}
}
