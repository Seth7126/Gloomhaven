using System;
using System.Linq;
using Script.GUI.SMNavigation.States;

namespace Code.State;

public class StateFilterByTag<TTag> : IStateFilter where TTag : Enum
{
	private TTag[] _tags;

	public StateFilterByTag(params TTag[] tags)
	{
		_tags = tags;
	}

	public bool IsValid(IState state)
	{
		if (state is NavigationState<TTag> navigationState && _tags.Contains(navigationState.StateTag))
		{
			return true;
		}
		return false;
	}
}
