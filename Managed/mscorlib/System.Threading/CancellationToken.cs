using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading;

/// <summary>Propagates notification that operations should be canceled.</summary>
[DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}")]
public readonly struct CancellationToken
{
	private readonly CancellationTokenSource _source;

	private static readonly Action<object> s_actionToActionObjShunt = delegate(object obj)
	{
		((Action)obj)();
	};

	/// <summary>Returns an empty CancellationToken value.</summary>
	/// <returns>Returns an empty CancellationToken value.</returns>
	public static CancellationToken None => default(CancellationToken);

	/// <summary>Gets whether cancellation has been requested for this token.</summary>
	/// <returns>true if cancellation has been requested for this token; otherwise false.</returns>
	public bool IsCancellationRequested
	{
		get
		{
			if (_source != null)
			{
				return _source.IsCancellationRequested;
			}
			return false;
		}
	}

	/// <summary>Gets whether this token is capable of being in the canceled state.</summary>
	/// <returns>true if this token is capable of being in the canceled state; otherwise false.</returns>
	public bool CanBeCanceled => _source != null;

	/// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that is signaled when the token is canceled.</summary>
	/// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is signaled when the token is canceled.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	public WaitHandle WaitHandle => (_source ?? CancellationTokenSource.s_neverCanceledSource).WaitHandle;

	internal CancellationToken(CancellationTokenSource source)
	{
		_source = source;
	}

	/// <summary>Initializes the <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <param name="canceled">The canceled state for the token.</param>
	public CancellationToken(bool canceled)
		: this(canceled ? CancellationTokenSource.s_canceledSource : null)
	{
	}

	/// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
	/// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callback" /> is null.</exception>
	public CancellationTokenRegistration Register(Action callback)
	{
		return Register(s_actionToActionObjShunt, callback ?? throw new ArgumentNullException("callback"), useSynchronizationContext: false, useExecutionContext: true);
	}

	/// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
	/// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
	/// <param name="useSynchronizationContext">A Boolean value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callback" /> is null.</exception>
	public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
	{
		return Register(s_actionToActionObjShunt, callback ?? throw new ArgumentNullException("callback"), useSynchronizationContext, useExecutionContext: true);
	}

	/// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
	/// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
	/// <param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callback" /> is null.</exception>
	public CancellationTokenRegistration Register(Action<object> callback, object state)
	{
		return Register(callback, state, useSynchronizationContext: false, useExecutionContext: true);
	}

	/// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
	/// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
	/// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
	/// <param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
	/// <param name="useSynchronizationContext">A Boolean value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="callback" /> is null.</exception>
	public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext)
	{
		return Register(callback, state, useSynchronizationContext, useExecutionContext: true);
	}

	internal CancellationTokenRegistration InternalRegisterWithoutEC(Action<object> callback, object state)
	{
		return Register(callback, state, useSynchronizationContext: false, useExecutionContext: false);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext, bool useExecutionContext)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		return _source?.InternalRegister(callback, state, useSynchronizationContext ? SynchronizationContext.Current : null, useExecutionContext ? ExecutionContext.Capture() : null) ?? default(CancellationTokenRegistration);
	}

	/// <summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified token.</summary>
	/// <returns>True if the instances are equal; otherwise, false. Two tokens are equal if they are associated with the same <see cref="T:System.Threading.CancellationTokenSource" /> or if they were both constructed from public CancellationToken constructors and their <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> values are equal.</returns>
	/// <param name="other">The other <see cref="T:System.Threading.CancellationToken" /> to which to compare this instance.</param>
	public bool Equals(CancellationToken other)
	{
		return _source == other._source;
	}

	/// <summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified <see cref="T:System.Object" />.</summary>
	/// <returns>True if <paramref name="other" /> is a <see cref="T:System.Threading.CancellationToken" /> and if the two instances are equal; otherwise, false. Two tokens are equal if they are associated with the same <see cref="T:System.Threading.CancellationTokenSource" /> or if they were both constructed from public CancellationToken constructors and their <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> values are equal.</returns>
	/// <param name="other">The other object to which to compare this instance.</param>
	/// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	public override bool Equals(object other)
	{
		if (other is CancellationToken)
		{
			return Equals((CancellationToken)other);
		}
		return false;
	}

	/// <summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationToken" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Threading.CancellationToken" /> instance.</returns>
	public override int GetHashCode()
	{
		return (_source ?? CancellationTokenSource.s_neverCanceledSource).GetHashCode();
	}

	/// <summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are equal.</summary>
	/// <returns>True if the instances are equal; otherwise, false.</returns>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	public static bool operator ==(CancellationToken left, CancellationToken right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are not equal.</summary>
	/// <returns>True if the instances are not equal; otherwise, false.</returns>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	/// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	public static bool operator !=(CancellationToken left, CancellationToken right)
	{
		return !left.Equals(right);
	}

	/// <summary>Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</summary>
	/// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
	public void ThrowIfCancellationRequested()
	{
		if (IsCancellationRequested)
		{
			ThrowOperationCanceledException();
		}
	}

	private void ThrowOperationCanceledException()
	{
		throw new OperationCanceledException("The operation was canceled.", this);
	}
}
