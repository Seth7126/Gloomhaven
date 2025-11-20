using System;
using System.Collections.Concurrent;

public class SaveQueue
{
	private ConcurrentQueue<IQueuedOperation> _writeQueue = new ConcurrentQueue<IQueuedOperation>();

	private IQueuedOperation _executingOperation;

	public bool IsAnyOperationExecuting => _executingOperation != null;

	public void EnqueueWriteOperation(Action<byte[], Action> saveAction, byte[] data, Action onDone = null)
	{
		SaveOperation item = new SaveOperation(saveAction, data, delegate
		{
			OnOperationDone(onDone);
		});
		_writeQueue.Enqueue(item);
		ProcessQueue();
	}

	public void EnqueueOperation(Action<Action> saveAction, Action onDone = null)
	{
		Operation item = new Operation(saveAction, delegate
		{
			OnOperationDone(onDone);
		});
		_writeQueue.Enqueue(item);
		ProcessQueue();
	}

	public void AbortAllOperations()
	{
		_writeQueue.Clear();
		_executingOperation = null;
	}

	private void OnOperationDone(Action OnDone)
	{
		OnDone?.Invoke();
		_executingOperation = null;
		ProcessQueue();
	}

	private void ProcessQueue()
	{
		if (!IsAnyOperationExecuting)
		{
			TryExecuteNext();
		}
	}

	private void TryExecuteNext()
	{
		if (_writeQueue.TryDequeue(out var result))
		{
			_executingOperation = result;
			result.Execute();
		}
	}
}
