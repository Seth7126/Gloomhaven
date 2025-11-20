using System;

namespace Script.Misc;

public class DisposeCallback : IDisposable
{
	private Action _callback;

	public DisposeCallback(Action callback)
	{
		_callback = callback;
	}

	public void Dispose()
	{
		_callback?.Invoke();
		_callback = null;
	}
}
