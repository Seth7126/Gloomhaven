using Code.State;
using Script.GUI.SMNavigation;

public class ToNonMenuPreviousStateWithFilterOperation : INavigationOperation
{
	private readonly IStateFilter _stateFilter;

	public ToNonMenuPreviousStateWithFilterOperation(IStateFilter stateFilter)
	{
		_stateFilter = stateFilter;
	}

	public void Execute(NavigationStateMachine stateMachine)
	{
		stateMachine.ToNonMenuPreviousState(_stateFilter);
	}
}
