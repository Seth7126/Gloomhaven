using Internal.Runtime.Augments;

namespace System.Threading;

internal struct ThreadPoolCallbackWrapper
{
	private RuntimeThread _currentThread;

	public static ThreadPoolCallbackWrapper Enter()
	{
		return new ThreadPoolCallbackWrapper
		{
			_currentThread = RuntimeThread.InitializeThreadPoolThread()
		};
	}

	public void Exit(bool resetThread = true)
	{
		if (resetThread)
		{
			_currentThread.ResetThreadPoolThread();
		}
	}
}
