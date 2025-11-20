using System.Runtime.InteropServices;

namespace System.Threading;

internal struct Win32ThreadPoolNativeOverlapped
{
	private class ExecutionContextCallbackArgs
	{
		internal uint _errorCode;

		internal uint _bytesWritten;

		internal unsafe Win32ThreadPoolNativeOverlapped* _overlapped;

		internal OverlappedData _data;
	}

	internal class OverlappedData
	{
		internal GCHandle[] _pinnedData;

		internal IOCompletionCallback _callback;

		internal object _state;

		internal ExecutionContext _executionContext;

		internal ThreadPoolBoundHandle _boundHandle;

		internal PreAllocatedOverlapped _preAllocated;

		internal bool _completed;

		internal void Reset()
		{
			if (_pinnedData != null)
			{
				for (int i = 0; i < _pinnedData.Length; i++)
				{
					if (_pinnedData[i].IsAllocated && _pinnedData[i].Target != null)
					{
						_pinnedData[i].Target = null;
					}
				}
			}
			_callback = null;
			_state = null;
			_executionContext = null;
			_completed = false;
			_preAllocated = null;
		}
	}

	[ThreadStatic]
	private static ExecutionContextCallbackArgs t_executionContextCallbackArgs;

	private static ContextCallback s_executionContextCallback;

	private static OverlappedData[] s_dataArray;

	private static int s_dataCount;

	private static IntPtr s_freeList;

	private NativeOverlapped _overlapped;

	private IntPtr _nextFree;

	private int _dataIndex;

	internal OverlappedData Data => s_dataArray[_dataIndex];

	static Win32ThreadPoolNativeOverlapped()
	{
		if (!Environment.IsRunningOnWindows)
		{
			throw new PlatformNotSupportedException();
		}
	}

	internal unsafe static Win32ThreadPoolNativeOverlapped* Allocate(IOCompletionCallback callback, object state, object pinData, PreAllocatedOverlapped preAllocated)
	{
		Win32ThreadPoolNativeOverlapped* ptr = AllocateNew();
		try
		{
			ptr->SetData(callback, state, pinData, preAllocated);
			return ptr;
		}
		catch
		{
			Free(ptr);
			throw;
		}
	}

	private unsafe static Win32ThreadPoolNativeOverlapped* AllocateNew()
	{
		IntPtr intPtr;
		Win32ThreadPoolNativeOverlapped* ptr;
		while ((intPtr = Volatile.Read(ref s_freeList)) != IntPtr.Zero)
		{
			ptr = (Win32ThreadPoolNativeOverlapped*)(void*)intPtr;
			if (!(Interlocked.CompareExchange(ref s_freeList, ptr->_nextFree, intPtr) != intPtr))
			{
				ptr->_nextFree = IntPtr.Zero;
				return ptr;
			}
		}
		ptr = (Win32ThreadPoolNativeOverlapped*)(void*)Interop.MemAlloc((UIntPtr)(ulong)sizeof(Win32ThreadPoolNativeOverlapped));
		*ptr = default(Win32ThreadPoolNativeOverlapped);
		OverlappedData value = new OverlappedData();
		int num = Interlocked.Increment(ref s_dataCount) - 1;
		if (num < 0)
		{
			Environment.FailFast("Too many outstanding Win32ThreadPoolNativeOverlapped instances");
		}
		while (true)
		{
			OverlappedData[] array = Volatile.Read(ref s_dataArray);
			int num2 = ((array != null) ? array.Length : 0);
			if (num2 <= num)
			{
				int num3 = num2;
				if (num3 == 0)
				{
					num3 = 128;
				}
				while (num3 <= num)
				{
					num3 = num3 * 3 / 2;
				}
				OverlappedData[] array2 = array;
				Array.Resize(ref array2, num3);
				if (Interlocked.CompareExchange(ref s_dataArray, array2, array) != array)
				{
					continue;
				}
				array = array2;
			}
			if (s_dataArray[num] != null)
			{
				break;
			}
			Interlocked.Exchange(ref array[num], value);
		}
		ptr->_dataIndex = num;
		return ptr;
	}

	private void SetData(IOCompletionCallback callback, object state, object pinData, PreAllocatedOverlapped preAllocated)
	{
		OverlappedData data = Data;
		data._callback = callback;
		data._state = state;
		data._executionContext = ExecutionContext.Capture();
		data._preAllocated = preAllocated;
		if (pinData == null)
		{
			return;
		}
		if (pinData is object[] array && array.GetType() == typeof(object[]))
		{
			if (data._pinnedData == null || data._pinnedData.Length < array.Length)
			{
				Array.Resize(ref data._pinnedData, array.Length);
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!data._pinnedData[i].IsAllocated)
				{
					data._pinnedData[i] = GCHandle.Alloc(array[i], GCHandleType.Pinned);
				}
				else
				{
					data._pinnedData[i].Target = array[i];
				}
			}
		}
		else
		{
			if (data._pinnedData == null)
			{
				data._pinnedData = new GCHandle[1];
			}
			if (!data._pinnedData[0].IsAllocated)
			{
				data._pinnedData[0] = GCHandle.Alloc(pinData, GCHandleType.Pinned);
			}
			else
			{
				data._pinnedData[0].Target = pinData;
			}
		}
	}

	internal unsafe static void Free(Win32ThreadPoolNativeOverlapped* overlapped)
	{
		overlapped->Data.Reset();
		overlapped->_overlapped = default(NativeOverlapped);
		IntPtr intPtr;
		do
		{
			intPtr = (overlapped->_nextFree = Volatile.Read(ref s_freeList));
		}
		while (!(Interlocked.CompareExchange(ref s_freeList, (IntPtr)overlapped, intPtr) == intPtr));
	}

	internal unsafe static NativeOverlapped* ToNativeOverlapped(Win32ThreadPoolNativeOverlapped* overlapped)
	{
		return (NativeOverlapped*)overlapped;
	}

	internal unsafe static Win32ThreadPoolNativeOverlapped* FromNativeOverlapped(NativeOverlapped* overlapped)
	{
		return (Win32ThreadPoolNativeOverlapped*)overlapped;
	}

	internal unsafe static void CompleteWithCallback(uint errorCode, uint bytesWritten, Win32ThreadPoolNativeOverlapped* overlapped)
	{
		OverlappedData data = overlapped->Data;
		data._completed = true;
		if (data._executionContext == null)
		{
			data._callback(errorCode, bytesWritten, ToNativeOverlapped(overlapped));
			return;
		}
		ContextCallback callback = OnExecutionContextCallback;
		ExecutionContextCallbackArgs executionContextCallbackArgs = t_executionContextCallbackArgs;
		if (executionContextCallbackArgs == null)
		{
			executionContextCallbackArgs = new ExecutionContextCallbackArgs();
		}
		t_executionContextCallbackArgs = null;
		executionContextCallbackArgs._errorCode = errorCode;
		executionContextCallbackArgs._bytesWritten = bytesWritten;
		executionContextCallbackArgs._overlapped = overlapped;
		executionContextCallbackArgs._data = data;
		ExecutionContext.Run(data._executionContext, callback, executionContextCallbackArgs);
	}

	private unsafe static void OnExecutionContextCallback(object state)
	{
		ExecutionContextCallbackArgs obj = (ExecutionContextCallbackArgs)state;
		uint errorCode = obj._errorCode;
		uint bytesWritten = obj._bytesWritten;
		Win32ThreadPoolNativeOverlapped* overlapped = obj._overlapped;
		OverlappedData data = obj._data;
		obj._data = null;
		t_executionContextCallbackArgs = obj;
		data._callback(errorCode, bytesWritten, ToNativeOverlapped(overlapped));
	}
}
