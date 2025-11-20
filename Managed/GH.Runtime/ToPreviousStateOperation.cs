using Script.GUI.SMNavigation;

public class ToPreviousStateOperation : INavigationOperation
{
	public void Execute(NavigationStateMachine stateMachine)
	{
		stateMachine.ToPreviousState();
	}
}
