using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System.Threading;

public sealed class ThreadPoolBoundHandle : IDisposable, IDeferredDisposable
{
	private readonly SafeHandle _handle;

	private readonly SafeThreadPoolIOHandle _threadPoolHandle;

	private DeferredDisposableLifetime<ThreadPoolBoundHandle> _lifetime;

	public SafeHandle Handle => _handle;

	static ThreadPoolBoundHandle()
	{
		if (!Environment.IsRunningOnWindows)
		{
			throw new PlatformNotSupportedException();
		}
	}

	private ThreadPoolBoundHandle(SafeHandle handle, SafeThreadPoolIOHandle threadPoolHandle)
	{
		_threadPoolHandle = threadPoolHandle;
		_handle = handle;
	}

	public static ThreadPoolBoundHandle BindHandle(SafeHandle handle)
	{
		if (handle == null)
		{
			throw new ArgumentNullException("handle");
		}
		if (handle.IsClosed || handle.IsInvalid)
		{
			throw new ArgumentException("'handle' has been disposed or is an invalid handle.", "handle");
		}
		IntPtr pfnio = AddrofIntrinsics.AddrOf<Interop.NativeIoCompletionCallback>(OnNativeIOCompleted);
		SafeThreadPoolIOHandle safeThreadPoolIOHandle = Interop.mincore.CreateThreadpoolIo(handle, pfnio, IntPtr.Zero, IntPtr.Zero);
		if (safeThreadPoolIOHandle.IsInvalid)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			switch (lastWin32Error)
			{
			case 6:
				throw new ArgumentException("'handle' has been disposed or is an invalid handle.", "handle");
			case 87:
				throw new ArgumentException("'handle' has already been bound to the thread pool, or was not opened for asynchronous I/O.", "handle");
			default:
				throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
			}
		}
		return new ThreadPoolBoundHandle(handle, safeThreadPoolIOHandle);
	}

	[CLSCompliant(false)]
	public unsafe NativeOverlapped* AllocateNativeOverlapped(IOCompletionCallback callback, object state, object pinData)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		AddRef();
		try
		{
			Win32ThreadPoolNativeOverlapped* intPtr = Win32ThreadPoolNativeOverlapped.Allocate(callback, state, pinData, null);
			intPtr->Data._boundHandle = this;
			Interop.mincore.StartThreadpoolIo(_threadPoolHandle);
			return Win32ThreadPoolNativeOverlapped.ToNativeOverlapped(intPtr);
		}
		catch
		{
			Release();
			throw;
		}
	}

	[CLSCompliant(false)]
	public unsafe NativeOverlapped* AllocateNativeOverlapped(PreAllocatedOverlapped preAllocated)
	{
		if (preAllocated == null)
		{
			throw new ArgumentNullException("preAllocated");
		}
		bool flag = false;
		bool flag2 = false;
		try
		{
			flag = AddRef();
			flag2 = preAllocated.AddRef();
			Win32ThreadPoolNativeOverlapped.OverlappedData data = preAllocated._overlapped->Data;
			if (data._boundHandle != null)
			{
				throw new ArgumentException("'preAllocated' is already in use.", "preAllocated");
			}
			data._boundHandle = this;
			Interop.mincore.StartThreadpoolIo(_threadPoolHandle);
			return Win32ThreadPoolNativeOverlapped.ToNativeOverlapped(preAllocated._overlapped);
		}
		catch
		{
			if (flag2)
			{
				preAllocated.Release();
			}
			if (flag)
			{
				Release();
			}
			throw;
		}
	}

	[CLSCompliant(false)]
	public unsafe void FreeNativeOverlapped(NativeOverlapped* overlapped)
	{
		if (overlapped == null)
		{
			throw new ArgumentNullException("overlapped");
		}
		Win32ThreadPoolNativeOverlapped* overlapped2 = Win32ThreadPoolNativeOverlapped.FromNativeOverlapped(overlapped);
		Win32ThreadPoolNativeOverlapped.OverlappedData overlappedData = GetOverlappedData(overlapped2, this);
		if (!overlappedData._completed)
		{
			Interop.mincore.CancelThreadpoolIo(_threadPoolHandle);
			Release();
		}
		overlappedData._boundHandle = null;
		overlappedData._completed = false;
		if (overlappedData._preAllocated != null)
		{
			overlappedData._preAllocated.Release();
		}
		else
		{
			Win32ThreadPoolNativeOverlapped.Free(overlapped2);
		}
	}

	[CLSCompliant(false)]
	public unsafe static object GetNativeOverlappedState(NativeOverlapped* overlapped)
	{
		if (overlapped == null)
		{
			throw new ArgumentNullException("overlapped");
		}
		return GetOverlappedData(Win32ThreadPoolNativeOverlapped.FromNativeOverlapped(overlapped), null)._state;
	}

	private unsafe static Win32ThreadPoolNativeOverlapped.OverlappedData GetOverlappedData(Win32ThreadPoolNativeOverlapped* overlapped, ThreadPoolBoundHandle expectedBoundHandle)
	{
		Win32ThreadPoolNativeOverlapped.OverlappedData data = overlapped->Data;
		if (data._boundHandle == null)
		{
			throw new ArgumentException("'overlapped' has already been freed.", "overlapped");
		}
		if (expectedBoundHandle != null && data._boundHandle != expectedBoundHandle)
		{
			throw new ArgumentException("'overlapped' was not allocated by this ThreadPoolBoundHandle instance.", "overlapped");
		}
		return data;
	}

	[NativeCallable(CallingConvention = CallingConvention.StdCall)]
	private unsafe static void OnNativeIOCompleted(IntPtr instance, IntPtr context, IntPtr overlappedPtr, uint ioResult, UIntPtr numberOfBytesTransferred, IntPtr ioPtr)
	{
		ThreadPoolCallbackWrapper threadPoolCallbackWrapper = ThreadPoolCallbackWrapper.Enter();
		Win32ThreadPoolNativeOverlapped* ptr = (Win32ThreadPoolNativeOverlapped*)(void*)overlappedPtr;
		(ptr->Data._boundHandle ?? throw new InvalidOperationException("'overlapped' has already been freed.")).Release();
		Win32ThreadPoolNativeOverlapped.CompleteWithCallback(ioResult, (uint)numberOfBytesTransferred, ptr);
		threadPoolCallbackWrapper.Exit();
	}

	private bool AddRef()
	{
		return _lifetime.AddRef(this);
	}

	private void Release()
	{
		_lifetime.Release(this);
	}

	public void Dispose()
	{
		_lifetime.Dispose(this);
		GC.SuppressFinalize(this);
	}

	~ThreadPoolBoundHandle()
	{
		if (!Environment.IsRunningOnWindows)
		{
			throw new PlatformNotSupportedException();
		}
		if (!Environment.HasShutdownStarted)
		{
			Dispose();
		}
	}

	void IDeferredDisposable.OnFinalRelease(bool disposed)
	{
		if (disposed)
		{
			_threadPoolHandle.Dispose();
		}
	}

	internal ThreadPoolBoundHandle()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
