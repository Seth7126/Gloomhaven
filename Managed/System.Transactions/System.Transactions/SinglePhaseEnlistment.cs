namespace System.Transactions;

/// <summary>Provides a set of callbacks that facilitate communication between a participant enlisted for Single Phase Commit and the transaction manager when the <see cref="M:System.Transactions.ISinglePhaseNotification.SinglePhaseCommit(System.Transactions.SinglePhaseEnlistment)" /> notification is received.</summary>
public class SinglePhaseEnlistment : Enlistment
{
	private Transaction tx;

	private object abortingEnlisted;

	internal SinglePhaseEnlistment()
	{
	}

	internal SinglePhaseEnlistment(Transaction tx, object abortingEnlisted)
	{
		this.tx = tx;
		this.abortingEnlisted = abortingEnlisted;
	}

	/// <summary>Represents a callback that is used to indicate to the transaction manager that the transaction should be rolled back.</summary>
	public void Aborted()
	{
		Aborted(null);
	}

	/// <summary>Represents a callback that is used to indicate to the transaction manager that the transaction should be rolled back, and provides an explanation.</summary>
	/// <param name="e">An explanation of why a rollback is initiated.</param>
	public void Aborted(Exception e)
	{
		if (tx != null)
		{
			tx.Rollback(e, abortingEnlisted);
		}
	}

	/// <summary>Represents a callback that is used to indicate to the transaction manager that the SinglePhaseCommit was successful.</summary>
	[System.MonoTODO]
	public void Committed()
	{
	}

	/// <summary>Represents a callback that is used to indicate to the transaction manager that the status of the transaction is in doubt.</summary>
	[System.MonoTODO("Not implemented")]
	public void InDoubt()
	{
		throw new NotImplementedException();
	}

	/// <summary>Represents a callback that is used to indicate to the transaction manager that the status of the transaction is in doubt, and provides an explanation.</summary>
	/// <param name="e">An explanation of why the transaction is in doubt.</param>
	[System.MonoTODO("Not implemented")]
	public void InDoubt(Exception e)
	{
		throw new NotImplementedException();
	}
}
