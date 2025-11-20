using System;

internal class SaveOperation : IQueuedOperation
{
	private Action<byte[], Action> _saveOperation;

	private Action _callback;

	private byte[] _data;

	public SaveOperation(Action<byte[], Action> saveOperation, byte[] data, Action callback)
	{
		_data = data;
		_saveOperation = saveOperation;
		_callback = callback;
	}

	public void Execute()
	{
		_saveOperation(_data, OnOperationDone);
	}

	private void OnOperationDone()
	{
		_callback();
	}
}
