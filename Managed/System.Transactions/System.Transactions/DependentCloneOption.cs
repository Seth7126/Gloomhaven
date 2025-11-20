namespace System.Transactions;

/// <summary>Controls what kind of dependent transaction to create.</summary>
public enum DependentCloneOption
{
	/// <summary>The dependent transaction blocks the commit process of the transaction until the parent transaction times out, or <see cref="M:System.Transactions.DependentTransaction.Complete" /> is called. In this case, additional work can be done on the transaction and new enlistments can be created.</summary>
	BlockCommitUntilComplete,
	/// <summary>The dependent transaction automatically aborts the transaction if Commit is called on the parent transaction before <see cref="M:System.Transactions.DependentTransaction.Complete" /> is called.</summary>
	RollbackIfNotComplete
}
