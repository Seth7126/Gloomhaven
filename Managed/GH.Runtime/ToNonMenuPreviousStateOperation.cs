using Script.GUI.SMNavigation;

public class ToNonMenuPreviousStateOperation : INavigationOperation
{
	public void Execute(NavigationStateMachine stateMachine)
	{
		stateMachine.ToNonMenuPreviousState();
	}
}
