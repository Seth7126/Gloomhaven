#define DEBUG
using System;
using System.Threading;

namespace UdpKit.Async;

internal class Task
{
	private readonly Thread _thread;

	private readonly AutoResetEvent _event;

	private Action _action;

	private readonly object _locker = new object();

	private volatile bool _abort;

	public bool IsRunning => _action != null;

	public string Name => _thread.ManagedThreadId.ToString();

	public Task()
	{
		_abort = false;
		_event = new AutoResetEvent(initialState: false);
		_thread = new Thread(Runner)
		{
			IsBackground = true,
			Priority = ThreadPriority.AboveNormal
		};
		_thread.Start();
		UdpLog.Debug("Starting UdpKit Task with ID: {0}", _thread.ManagedThreadId);
	}

	public void Run(Action callback)
	{
		lock (_locker)
		{
			_action = callback;
			_event.Set();
		}
	}

	public void Abort()
	{
		UdpLog.Debug("Stopping UdpKit Task {0}", _thread.ManagedThreadId);
		_abort = true;
		_event.Set();
	}

	public void ForceAbort()
	{
		UdpLog.Debug("Forcing stop UdpKit Task {0}", _thread.ManagedThreadId);
		_thread.Abort();
	}

	private void Runner()
	{
		try
		{
			while (!_abort)
			{
				_event.WaitOne();
				lock (_locker)
				{
					if (_action != null)
					{
						_action();
						_action = null;
					}
				}
			}
		}
		catch (ThreadAbortException)
		{
			UdpLog.Debug("UdpKit Task Aborted {0}", Thread.CurrentThread.ManagedThreadId);
		}
		catch (ThreadInterruptedException)
		{
			UdpLog.Debug("UdpKit Task Interrupted {0}", Thread.CurrentThread.ManagedThreadId);
		}
	}
}
