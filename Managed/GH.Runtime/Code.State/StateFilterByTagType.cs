using System;
using Script.GUI.SMNavigation.States;

namespace Code.State;

public class StateFilterByTagType<TTag> : IStateFilter where TTag : Enum
{
	public bool IsValid(IState state)
	{
		return state is NavigationState<TTag>;
	}
}
