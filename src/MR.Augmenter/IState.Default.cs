using System.Collections.Generic;

namespace MR.Augmenter
{
	public class State : GracefulDictionary, IState, IReadOnlyState
	{
		IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

		IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;
	}
}
