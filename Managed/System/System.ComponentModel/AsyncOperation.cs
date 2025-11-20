using System.Threading;
using Unity;

namespace System.ComponentModel;

/// <summary>Tracks the lifetime of an asynchronous operation.</summary>
public sealed class AsyncOperation
{
	private readonly SynchronizationContext _syncContext;

	private readonly object _userSuppliedState;

	private bool _alreadyCompleted;

	/// <summary>Gets or sets an object used to uniquely identify an asynchronous operation.</summary>
	/// <returns>The state object passed to the asynchronous method invocation.</returns>
	public object UserSuppliedState => _userSuppliedState;

	/// <summary>Gets the <see cref="T:System.Threading.SynchronizationContext" /> object that was passed to the constructor.</summary>
	/// <returns>The <see cref="T:System.Threading.SynchronizationContext" /> object that was passed to the constructor.</returns>
	public SynchronizationContext SynchronizationContext => _syncContext;

	private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext)
	{
		_userSuppliedState = userSuppliedState;
		_syncContext = syncContext;
		_alreadyCompleted = false;
		_syncContext.OperationStarted();
	}

	~AsyncOperation()
	{
		if (!_alreadyCompleted && _syncContext != null)
		{
			_syncContext.OperationCompleted();
		}
	}

	/// <summary>Invokes a delegate on the thread or context appropriate for the application model.</summary>
	/// <param name="d">A <see cref="T:System.Threading.SendOrPostCallback" /> object that wraps the delegate to be called when the operation ends. </param>
	/// <param name="arg">An argument for the delegate contained in the <paramref name="d" /> parameter. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.ComponentModel.AsyncOperation.PostOperationCompleted(System.Threading.SendOrPostCallback,System.Object)" /> method has been called previously for this task. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public void Post(SendOrPostCallback d, object arg)
	{
		PostCore(d, arg, markCompleted: false);
	}

	/// <summary>Ends the lifetime of an asynchronous operation.</summary>
	/// <param name="d">A <see cref="T:System.Threading.SendOrPostCallback" /> object that wraps the delegate to be called when the operation ends. </param>
	/// <param name="arg">An argument for the delegate contained in the <paramref name="d" /> parameter. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.ComponentModel.AsyncOperation.OperationCompleted" /> has been called previously for this task. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="d" /> is null. </exception>
	public void PostOperationCompleted(SendOrPostCallback d, object arg)
	{
		PostCore(d, arg, markCompleted: true);
		OperationCompletedCore();
	}

	/// <summary>Ends the lifetime of an asynchronous operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.ComponentModel.AsyncOperation.OperationCompleted" /> has been called previously for this task. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void OperationCompleted()
	{
		VerifyNotCompleted();
		_alreadyCompleted = true;
		OperationCompletedCore();
	}

	private void PostCore(SendOrPostCallback d, object arg, bool markCompleted)
	{
		VerifyNotCompleted();
		VerifyDelegateNotNull(d);
		if (markCompleted)
		{
			_alreadyCompleted = true;
		}
		_syncContext.Post(d, arg);
	}

	private void OperationCompletedCore()
	{
		try
		{
			_syncContext.OperationCompleted();
		}
		finally
		{
			GC.SuppressFinalize(this);
		}
	}

	private void VerifyNotCompleted()
	{
		if (_alreadyCompleted)
		{
			throw new InvalidOperationException("This operation has already had OperationCompleted called on it and further calls are illegal.");
		}
	}

	private void VerifyDelegateNotNull(SendOrPostCallback d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d", "A non-null SendOrPostCallback must be supplied.");
		}
	}

	internal static AsyncOperation CreateOperation(object userSuppliedState, SynchronizationContext syncContext)
	{
		return new AsyncOperation(userSuppliedState, syncContext);
	}

	internal AsyncOperation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
