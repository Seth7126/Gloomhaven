using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Pipes;

internal abstract class PipeCompletionSource<TResult> : TaskCompletionSource<TResult>
{
	private const int NoResult = 0;

	private const int ResultSuccess = 1;

	private const int ResultError = 2;

	private const int RegisteringCancellation = 4;

	private const int CompletedCallback = 8;

	private readonly ThreadPoolBoundHandle _threadPoolBinding;

	private CancellationTokenRegistration _cancellationRegistration;

	private int _errorCode;

	private unsafe NativeOverlapped* _overlapped;

	private MemoryHandle _pinnedMemory;

	private int _state;

	internal unsafe NativeOverlapped* Overlapped => _overlapped;

	protected unsafe PipeCompletionSource(ThreadPoolBoundHandle handle, ReadOnlyMemory<byte> bufferToPin)
		: base(TaskCreationOptions.RunContinuationsAsynchronously)
	{
		_threadPoolBinding = handle;
		_state = 0;
		_pinnedMemory = bufferToPin.Pin();
		_overlapped = _threadPoolBinding.AllocateNativeOverlapped(delegate(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
		{
			((PipeCompletionSource<TResult>)ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped)).AsyncCallback(errorCode, numBytes);
		}, this, null);
	}

	internal unsafe void RegisterForCancellation(CancellationToken cancellationToken)
	{
		if (!cancellationToken.CanBeCanceled || Overlapped == null)
		{
			return;
		}
		int num = Interlocked.CompareExchange(ref _state, 4, 0);
		switch (num)
		{
		case 0:
			_cancellationRegistration = cancellationToken.Register(delegate(object thisRef)
			{
				((PipeCompletionSource<TResult>)thisRef).Cancel();
			}, this);
			num = Interlocked.Exchange(ref _state, 0);
			break;
		default:
			num = Interlocked.Exchange(ref _state, 0);
			break;
		case 8:
			break;
		}
		if ((num & 3) != 0)
		{
			CompleteCallback(num);
		}
	}

	internal unsafe void ReleaseResources()
	{
		_cancellationRegistration.Dispose();
		if (_overlapped != null)
		{
			_threadPoolBinding.FreeNativeOverlapped(Overlapped);
			_overlapped = null;
		}
		_pinnedMemory.Dispose();
	}

	internal abstract void SetCompletedSynchronously();

	protected virtual void AsyncCallback(uint errorCode, uint numBytes)
	{
		int num;
		if (errorCode == 0)
		{
			num = 1;
		}
		else
		{
			num = 2;
			_errorCode = (int)errorCode;
		}
		if (Interlocked.Exchange(ref _state, num) == 0 && Interlocked.Exchange(ref _state, 8) != 0)
		{
			CompleteCallback(num);
		}
	}

	protected abstract void HandleError(int errorCode);

	private unsafe void Cancel()
	{
		SafeHandle handle = _threadPoolBinding.Handle;
		NativeOverlapped* overlapped = Overlapped;
		if (!handle.IsInvalid && !global::Interop.Kernel32.CancelIoEx(handle, overlapped))
		{
			Marshal.GetLastWin32Error();
		}
	}

	protected virtual void HandleUnexpectedCancellation()
	{
		TrySetCanceled();
	}

	private void CompleteCallback(int resultState)
	{
		CancellationToken token = _cancellationRegistration.Token;
		ReleaseResources();
		if (resultState == 2)
		{
			if (_errorCode == 995)
			{
				if (token.CanBeCanceled && !token.IsCancellationRequested)
				{
					HandleUnexpectedCancellation();
				}
				else
				{
					TrySetCanceled(token);
				}
			}
			else
			{
				HandleError(_errorCode);
			}
		}
		else
		{
			SetCompletedSynchronously();
		}
	}
}
