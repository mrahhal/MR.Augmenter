using System.Collections.Generic;

namespace MR.Augmenter
{
	public class State : GracefulDictionary, IState, IReadOnlyState
	{
		public State()
		{
		}

		public State(IReadOnlyState state1, State state2)
		{
			foreach (var pair in state1)
			{
				this[pair.Key] = pair.Value;
			}
			foreach (var pair in state2)
			{
				this[pair.Key] = pair.Value;
			}
		}

		IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

		IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;
	}
}
