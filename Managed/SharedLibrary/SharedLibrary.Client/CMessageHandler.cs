#define ENABLE_LOGS
using System;
using System.Collections.Concurrent;
using System.Threading;
using SM.Utils;
using SharedLibrary.Logger;

namespace SharedLibrary.Client;

public class CMessageHandler
{
	public delegate void MessageHandlerCallback(object message, bool processImmediately = false);

	private volatile ConcurrentQueue<object> _messageQueue;

	private volatile BlockingCollection<object> _blockingCollection;

	private volatile ManualResetEventSlim _manualResetEventSlim;

	private volatile CancellationTokenSource _cancellationTokenSource;

	private bool _blockMessageProcessing;

	private bool _threadStarted;

	public bool IsInitialised { get; private set; }

	public MessageHandlerCallback MessageHandler { get; private set; }

	public Thread MessageThreadHandler { get; private set; }

	public int MessageQueueLength => _blockingCollection.Count;

	public virtual void Initialise(bool reinitialise = false)
	{
		if (!IsInitialised || reinitialise)
		{
			IsInitialised = true;
		}
		_blockMessageProcessing = false;
		Reset();
	}

	public void ToggleMessageProcessing(bool process)
	{
		if (process)
		{
			if (_blockMessageProcessing)
			{
				_blockMessageProcessing = false;
				if (_manualResetEventSlim != null)
				{
					_manualResetEventSlim.Set();
				}
			}
		}
		else if (!_blockMessageProcessing)
		{
			_blockMessageProcessing = true;
			if (_manualResetEventSlim != null)
			{
				_manualResetEventSlim.Reset();
			}
		}
	}

	protected virtual void ProcessMessage(object message)
	{
	}

	private void ResetQueue()
	{
		_messageQueue = new ConcurrentQueue<object>();
		_blockingCollection = new BlockingCollection<object>(_messageQueue);
		if (_manualResetEventSlim != null)
		{
			_manualResetEventSlim.Dispose();
		}
		_manualResetEventSlim = new ManualResetEventSlim(!_blockMessageProcessing);
	}

	public void ThreadMessageHandler()
	{
		CancellationToken token = _cancellationTokenSource.Token;
		try
		{
			while (true)
			{
				token.ThrowIfCancellationRequested();
				_manualResetEventSlim.Wait(token);
				object obj = null;
				token.ThrowIfCancellationRequested();
				obj = _blockingCollection.Take(token);
				token.ThrowIfCancellationRequested();
				_manualResetEventSlim.Wait(token);
				token.ThrowIfCancellationRequested();
				if (obj != null)
				{
					ProcessMessage(obj);
				}
			}
		}
		catch (OperationCanceledException)
		{
			LogUtils.Log("OperationCanceledException ThreadEventLogMessageHandler finished");
		}
		catch (ObjectDisposedException)
		{
			LogUtils.Log("ObjectDisposedException ThreadEventLogMessageHandler finished");
		}
	}

	public virtual void SetMessageHandler(MessageHandlerCallback messageHandler)
	{
		MessageHandler = messageHandler;
	}

	public virtual void Start()
	{
		try
		{
			_blockMessageProcessing = false;
			_cancellationTokenSource = new CancellationTokenSource();
			_threadStarted = true;
			MessageThreadHandler.Start();
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception while Starting Message Handler.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public virtual void Stop()
	{
		try
		{
			_cancellationTokenSource.Cancel();
			MessageThreadHandler.Join();
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception while Stopping Message Handler.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public virtual void Reset()
	{
		MessageHandler = null;
		if (_threadStarted && !_cancellationTokenSource.IsCancellationRequested)
		{
			_cancellationTokenSource.Cancel();
			MessageThreadHandler.Join();
		}
		ResetQueue();
		_threadStarted = false;
		MessageThreadHandler = new Thread(ThreadMessageHandler);
	}

	public void AddQueueMessage(object message, bool processImmediately)
	{
		if (processImmediately)
		{
			ProcessMessage(message);
		}
		else
		{
			_blockingCollection.Add(message);
		}
	}
}
