using System;
using Script.GUI.SMNavigation;

public class AnonymousOperation : INavigationOperation
{
	private readonly Action<NavigationStateMachine> _operation;

	public AnonymousOperation(Action<NavigationStateMachine> operation)
	{
		_operation = operation;
	}

	public void Execute(NavigationStateMachine stateMachine)
	{
		_operation?.Invoke(stateMachine);
	}
}
