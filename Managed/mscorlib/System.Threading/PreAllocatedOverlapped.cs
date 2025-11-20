namespace System.Threading;

public sealed class PreAllocatedOverlapped : IDisposable, IDeferredDisposable
{
	internal unsafe readonly Win32ThreadPoolNativeOverlapped* _overlapped;

	private DeferredDisposableLifetime<PreAllocatedOverlapped> _lifetime;

	static PreAllocatedOverlapped()
	{
		if (!Environment.IsRunningOnWindows)
		{
			throw new PlatformNotSupportedException();
		}
	}

	[CLSCompliant(false)]
	public unsafe PreAllocatedOverlapped(IOCompletionCallback callback, object state, object pinData)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		_overlapped = Win32ThreadPoolNativeOverlapped.Allocate(callback, state, pinData, this);
	}

	internal bool AddRef()
	{
		return _lifetime.AddRef(this);
	}

	internal void Release()
	{
		_lifetime.Release(this);
	}

	public void Dispose()
	{
		_lifetime.Dispose(this);
		GC.SuppressFinalize(this);
	}

	~PreAllocatedOverlapped()
	{
		if (!Environment.HasShutdownStarted)
		{
			Dispose();
		}
	}

	unsafe void IDeferredDisposable.OnFinalRelease(bool disposed)
	{
		if (_overlapped != null)
		{
			if (disposed)
			{
				Win32ThreadPoolNativeOverlapped.Free(_overlapped);
			}
			else
			{
				*Win32ThreadPoolNativeOverlapped.ToNativeOverlapped(_overlapped) = default(NativeOverlapped);
			}
		}
	}
}
