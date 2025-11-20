using System;
using System.Runtime.ConstrainedExecution;

namespace PlayEveryWare.EpicOnlineServices;

public abstract class GenericSafeHandle<HandleType> : CriticalFinalizerObject, IDisposable
{
	protected HandleType handleObject;

	private bool disposedValue;

	public GenericSafeHandle(HandleType handle)
	{
		handleObject = handle;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	}

	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Dispose(disposing: false);
	}

	protected abstract void ReleaseHandle();

	public abstract bool IsValid();

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			ReleaseHandle();
			disposedValue = true;
		}
	}

	~GenericSafeHandle()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
