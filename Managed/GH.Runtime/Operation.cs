using System;

internal class Operation : IQueuedOperation
{
	private Action<Action> _operation;

	private Action _callback;

	public Operation(Action<Action> operation, Action callback)
	{
		_operation = operation;
		_callback = callback;
	}

	public void Execute()
	{
		_operation(OnOperationDone);
	}

	private void OnOperationDone()
	{
		_callback();
	}
}
