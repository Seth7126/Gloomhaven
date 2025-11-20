using System.Threading;
using Unity;

namespace System.Transactions;

/// <summary>Facilitates communication between an enlisted transaction participant and the transaction manager during the Prepare phase of the transaction.</summary>
public class PreparingEnlistment : Enlistment
{
	private bool prepared;

	private Transaction tx;

	private IEnlistmentNotification enlisted;

	private WaitHandle waitHandle;

	private Exception ex;

	internal bool IsPrepared => prepared;

	internal WaitHandle WaitHandle => waitHandle;

	internal IEnlistmentNotification EnlistmentNotification => enlisted;

	internal Exception Exception
	{
		get
		{
			return ex;
		}
		set
		{
			ex = value;
		}
	}

	internal PreparingEnlistment(Transaction tx, IEnlistmentNotification enlisted)
	{
		this.tx = tx;
		this.enlisted = enlisted;
		waitHandle = new ManualResetEvent(initialState: false);
	}

	/// <summary>Indicates that the transaction should be rolled back.</summary>
	public void ForceRollback()
	{
		ForceRollback(null);
	}

	internal override void InternalOnDone()
	{
		Prepared();
	}

	/// <summary>Indicates that the transaction should be rolled back.</summary>
	/// <param name="e">An explanation of why a rollback is triggered.</param>
	[System.MonoTODO]
	public void ForceRollback(Exception e)
	{
		tx.Rollback(e, enlisted);
		((ManualResetEvent)waitHandle).Set();
	}

	/// <summary>Indicates that the transaction can be committed.</summary>
	[System.MonoTODO]
	public void Prepared()
	{
		prepared = true;
		((ManualResetEvent)waitHandle).Set();
	}

	/// <summary>Gets the recovery information of an enlistment.</summary>
	/// <returns>The recovery information of an enlistment.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt to get recovery information inside a volatile enlistment, which does not generate any recovery information.</exception>
	[System.MonoTODO]
	public byte[] RecoveryInformation()
	{
		throw new NotImplementedException();
	}

	internal PreparingEnlistment()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
