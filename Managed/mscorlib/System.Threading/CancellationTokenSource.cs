using System.Collections.Generic;

namespace System.Threading;

/// <summary>Signals to a <see cref="T:System.Threading.CancellationToken" /> that it should be canceled.</summary>
public class CancellationTokenSource : IDisposable
{
	private sealed class Linked1CancellationTokenSource : CancellationTokenSource
	{
		private readonly CancellationTokenRegistration _reg1;

		internal Linked1CancellationTokenSource(CancellationToken token1)
		{
			_reg1 = token1.InternalRegisterWithoutEC(LinkedNCancellationTokenSource.s_linkedTokenCancelDelegate, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_reg1.Dispose();
				base.Dispose(disposing);
			}
		}
	}

	private sealed class Linked2CancellationTokenSource : CancellationTokenSource
	{
		private readonly CancellationTokenRegistration _reg1;

		private readonly CancellationTokenRegistration _reg2;

		internal Linked2CancellationTokenSource(CancellationToken token1, CancellationToken token2)
		{
			_reg1 = token1.InternalRegisterWithoutEC(LinkedNCancellationTokenSource.s_linkedTokenCancelDelegate, this);
			_reg2 = token2.InternalRegisterWithoutEC(LinkedNCancellationTokenSource.s_linkedTokenCancelDelegate, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_reg1.Dispose();
				_reg2.Dispose();
				base.Dispose(disposing);
			}
		}
	}

	private sealed class LinkedNCancellationTokenSource : CancellationTokenSource
	{
		internal static readonly Action<object> s_linkedTokenCancelDelegate = delegate(object s)
		{
			((CancellationTokenSource)s).NotifyCancellation(throwOnFirstException: false);
		};

		private CancellationTokenRegistration[] _linkingRegistrations;

		internal LinkedNCancellationTokenSource(params CancellationToken[] tokens)
		{
			_linkingRegistrations = new CancellationTokenRegistration[tokens.Length];
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i].CanBeCanceled)
				{
					_linkingRegistrations[i] = tokens[i].InternalRegisterWithoutEC(s_linkedTokenCancelDelegate, this);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
			{
				return;
			}
			CancellationTokenRegistration[] linkingRegistrations = _linkingRegistrations;
			if (linkingRegistrations != null)
			{
				_linkingRegistrations = null;
				for (int i = 0; i < linkingRegistrations.Length; i++)
				{
					linkingRegistrations[i].Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}

	internal static readonly CancellationTokenSource s_canceledSource = new CancellationTokenSource
	{
		_state = 3
	};

	internal static readonly CancellationTokenSource s_neverCanceledSource = new CancellationTokenSource
	{
		_state = 0
	};

	private static readonly int s_nLists = ((PlatformHelper.ProcessorCount > 24) ? 24 : PlatformHelper.ProcessorCount);

	private volatile ManualResetEvent _kernelEvent;

	private volatile SparselyPopulatedArray<CancellationCallbackInfo>[] _registeredCallbacksLists;

	private const int CannotBeCanceled = 0;

	private const int NotCanceledState = 1;

	private const int NotifyingState = 2;

	private const int NotifyingCompleteState = 3;

	private volatile int _state;

	private volatile int _threadIDExecutingCallbacks = -1;

	private bool _disposed;

	private volatile CancellationCallbackInfo _executingCallback;

	private volatile Timer _timer;

	private static readonly TimerCallback s_timerCallback = TimerCallbackLogic;

	/// <summary>Gets whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
	/// <returns>Whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
	public bool IsCancellationRequested => _state >= 2;

	internal bool IsCancellationCompleted => _state == 3;

	internal bool IsDisposed => _disposed;

	internal int ThreadIDExecutingCallbacks
	{
		get
		{
			return _threadIDExecutingCallbacks;
		}
		set
		{
			_threadIDExecutingCallbacks = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The token source has been disposed.</exception>
	public CancellationToken Token
	{
		get
		{
			ThrowIfDisposed();
			return new CancellationToken(this);
		}
	}

	internal bool CanBeCanceled => _state != 0;

	internal WaitHandle WaitHandle
	{
		get
		{
			ThrowIfDisposed();
			if (_kernelEvent != null)
			{
				return _kernelEvent;
			}
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			if (Interlocked.CompareExchange(ref _kernelEvent, manualResetEvent, null) != null)
			{
				manualResetEvent.Dispose();
			}
			if (IsCancellationRequested)
			{
				_kernelEvent.Set();
			}
			return _kernelEvent;
		}
	}

	internal CancellationCallbackInfo ExecutingCallback => _executingCallback;

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	public CancellationTokenSource()
	{
		_state = 1;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified time span.</summary>
	/// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="delay" />.<see cref="P:System.TimeSpan.TotalMilliseconds" /> is less than -1 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public CancellationTokenSource(TimeSpan delay)
	{
		long num = (long)delay.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("delay");
		}
		InitializeWithTimer((int)num);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class that will be canceled after the specified delay in milliseconds.</summary>
	/// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when <paramref name="millisecondsDelay" /> is less than -1.</exception>
	public CancellationTokenSource(int millisecondsDelay)
	{
		if (millisecondsDelay < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsDelay");
		}
		InitializeWithTimer(millisecondsDelay);
	}

	private void InitializeWithTimer(int millisecondsDelay)
	{
		_state = 1;
		_timer = new Timer(s_timerCallback, this, millisecondsDelay, -1);
	}

	/// <summary>Communicates a request for cancellation.</summary>
	/// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
	public void Cancel()
	{
		Cancel(throwOnFirstException: false);
	}

	/// <summary>Communicates a request for cancellation, and specifies whether remaining callbacks and cancelable operations should be processed.</summary>
	/// <param name="throwOnFirstException">true if exceptions should immediately propagate; otherwise, false.</param>
	/// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
	public void Cancel(bool throwOnFirstException)
	{
		ThrowIfDisposed();
		NotifyCancellation(throwOnFirstException);
	}

	/// <summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified time span.</summary>
	/// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when <paramref name="delay" /> is less than -1 or greater than Int32.MaxValue.</exception>
	public void CancelAfter(TimeSpan delay)
	{
		long num = (long)delay.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("delay");
		}
		CancelAfter((int)num);
	}

	/// <summary>Schedules a cancel operation on this <see cref="T:System.Threading.CancellationTokenSource" /> after the specified number of milliseconds.</summary>
	/// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception thrown when <paramref name="millisecondsDelay" /> is less than -1.</exception>
	public void CancelAfter(int millisecondsDelay)
	{
		ThrowIfDisposed();
		if (millisecondsDelay < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsDelay");
		}
		if (IsCancellationRequested)
		{
			return;
		}
		if (_timer == null)
		{
			Timer timer = new Timer(s_timerCallback, this, -1, -1);
			if (Interlocked.CompareExchange(ref _timer, timer, null) != null)
			{
				timer.Dispose();
			}
		}
		try
		{
			_timer.Change(millisecondsDelay, -1);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private static void TimerCallbackLogic(object obj)
	{
		CancellationTokenSource cancellationTokenSource = (CancellationTokenSource)obj;
		if (cancellationTokenSource.IsDisposed)
		{
			return;
		}
		try
		{
			cancellationTokenSource.Cancel();
		}
		catch (ObjectDisposedException)
		{
			if (!cancellationTokenSource.IsDisposed)
			{
				throw;
			}
		}
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
	/// <exception cref="T:System.ObjectDisposedException">A linked <see cref="T:System.Threading.CancellationTokenSource" /> has already been disposed.</exception>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Threading.CancellationTokenSource" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || _disposed)
		{
			return;
		}
		_timer?.Dispose();
		_registeredCallbacksLists = null;
		if (_kernelEvent != null)
		{
			ManualResetEvent manualResetEvent = Interlocked.Exchange(ref _kernelEvent, null);
			if (manualResetEvent != null && _state != 2)
			{
				manualResetEvent.Dispose();
			}
		}
		_disposed = true;
	}

	internal void ThrowIfDisposed()
	{
		if (_disposed)
		{
			ThrowObjectDisposedException();
		}
	}

	private static void ThrowObjectDisposedException()
	{
		throw new ObjectDisposedException(null, "The CancellationTokenSource has been disposed.");
	}

	internal CancellationTokenRegistration InternalRegister(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext executionContext)
	{
		if (!IsCancellationRequested)
		{
			if (_disposed)
			{
				return default(CancellationTokenRegistration);
			}
			int num = Environment.CurrentManagedThreadId % s_nLists;
			CancellationCallbackInfo cancellationCallbackInfo = ((targetSyncContext != null) ? new CancellationCallbackInfo.WithSyncContext(callback, stateForCallback, executionContext, this, targetSyncContext) : new CancellationCallbackInfo(callback, stateForCallback, executionContext, this));
			SparselyPopulatedArray<CancellationCallbackInfo>[] array = _registeredCallbacksLists;
			if (array == null)
			{
				SparselyPopulatedArray<CancellationCallbackInfo>[] array2 = new SparselyPopulatedArray<CancellationCallbackInfo>[s_nLists];
				array = Interlocked.CompareExchange(ref _registeredCallbacksLists, array2, null);
				if (array == null)
				{
					array = array2;
				}
			}
			SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read(ref array[num]);
			if (sparselyPopulatedArray == null)
			{
				SparselyPopulatedArray<CancellationCallbackInfo> value = new SparselyPopulatedArray<CancellationCallbackInfo>(4);
				Interlocked.CompareExchange(ref array[num], value, null);
				sparselyPopulatedArray = array[num];
			}
			SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo = sparselyPopulatedArray.Add(cancellationCallbackInfo);
			CancellationTokenRegistration result = new CancellationTokenRegistration(cancellationCallbackInfo, registrationInfo);
			if (!IsCancellationRequested)
			{
				return result;
			}
			if (!result.Unregister())
			{
				return result;
			}
		}
		callback(stateForCallback);
		return default(CancellationTokenRegistration);
	}

	private void NotifyCancellation(bool throwOnFirstException)
	{
		if (!IsCancellationRequested && Interlocked.CompareExchange(ref _state, 2, 1) == 1)
		{
			_timer?.Dispose();
			ThreadIDExecutingCallbacks = Environment.CurrentManagedThreadId;
			_kernelEvent?.Set();
			ExecuteCallbackHandlers(throwOnFirstException);
		}
	}

	private void ExecuteCallbackHandlers(bool throwOnFirstException)
	{
		LowLevelListWithIList<Exception> lowLevelListWithIList = null;
		SparselyPopulatedArray<CancellationCallbackInfo>[] registeredCallbacksLists = _registeredCallbacksLists;
		if (registeredCallbacksLists == null)
		{
			Interlocked.Exchange(ref _state, 3);
			return;
		}
		try
		{
			for (int i = 0; i < registeredCallbacksLists.Length; i++)
			{
				SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read(ref registeredCallbacksLists[i]);
				if (sparselyPopulatedArray == null)
				{
					continue;
				}
				for (SparselyPopulatedArrayFragment<CancellationCallbackInfo> sparselyPopulatedArrayFragment = sparselyPopulatedArray.Tail; sparselyPopulatedArrayFragment != null; sparselyPopulatedArrayFragment = sparselyPopulatedArrayFragment.Prev)
				{
					for (int num = sparselyPopulatedArrayFragment.Length - 1; num >= 0; num--)
					{
						_executingCallback = sparselyPopulatedArrayFragment[num];
						if (_executingCallback != null)
						{
							CancellationCallbackCoreWorkArguments cancellationCallbackCoreWorkArguments = new CancellationCallbackCoreWorkArguments(sparselyPopulatedArrayFragment, num);
							try
							{
								if (_executingCallback is CancellationCallbackInfo.WithSyncContext withSyncContext)
								{
									withSyncContext.TargetSyncContext.Send(CancellationCallbackCoreWork_OnSyncContext, cancellationCallbackCoreWorkArguments);
									ThreadIDExecutingCallbacks = Environment.CurrentManagedThreadId;
								}
								else
								{
									CancellationCallbackCoreWork(cancellationCallbackCoreWorkArguments);
								}
							}
							catch (Exception item)
							{
								if (throwOnFirstException)
								{
									throw;
								}
								if (lowLevelListWithIList == null)
								{
									lowLevelListWithIList = new LowLevelListWithIList<Exception>();
								}
								lowLevelListWithIList.Add(item);
							}
						}
					}
				}
			}
		}
		finally
		{
			_state = 3;
			_executingCallback = null;
			Interlocked.MemoryBarrier();
		}
		if (lowLevelListWithIList == null)
		{
			return;
		}
		throw new AggregateException(lowLevelListWithIList);
	}

	private void CancellationCallbackCoreWork_OnSyncContext(object obj)
	{
		CancellationCallbackCoreWork((CancellationCallbackCoreWorkArguments)obj);
	}

	private void CancellationCallbackCoreWork(CancellationCallbackCoreWorkArguments args)
	{
		CancellationCallbackInfo cancellationCallbackInfo = args._currArrayFragment.SafeAtomicRemove(args._currArrayIndex, _executingCallback);
		if (cancellationCallbackInfo == _executingCallback)
		{
			cancellationCallbackInfo.CancellationTokenSource.ThreadIDExecutingCallbacks = Environment.CurrentManagedThreadId;
			cancellationCallbackInfo.ExecuteCallback();
		}
	}

	/// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
	/// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
	/// <param name="token1">The first cancellation token to observe.</param>
	/// <param name="token2">The second cancellation token to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
	public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
	{
		if (token1.CanBeCanceled)
		{
			if (!token2.CanBeCanceled)
			{
				return new Linked1CancellationTokenSource(token1);
			}
			return new Linked2CancellationTokenSource(token1, token2);
		}
		return CreateLinkedTokenSource(token2);
	}

	internal static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token)
	{
		if (!token.CanBeCanceled)
		{
			return new CancellationTokenSource();
		}
		return new Linked1CancellationTokenSource(token);
	}

	/// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens in the specified array are in the canceled state.</summary>
	/// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
	/// <param name="tokens">An array that contains the cancellation token instances to observe.</param>
	/// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="tokens" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="tokens" /> is empty.</exception>
	public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
	{
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens");
		}
		return tokens.Length switch
		{
			0 => throw new ArgumentException("No tokens were supplied."), 
			1 => CreateLinkedTokenSource(tokens[0]), 
			2 => CreateLinkedTokenSource(tokens[0], tokens[1]), 
			_ => new LinkedNCancellationTokenSource(tokens), 
		};
	}

	internal void WaitForCallbackToComplete(CancellationCallbackInfo callbackInfo)
	{
		SpinWait spinWait = default(SpinWait);
		while (ExecutingCallback == callbackInfo)
		{
			spinWait.SpinOnce();
		}
	}
}
