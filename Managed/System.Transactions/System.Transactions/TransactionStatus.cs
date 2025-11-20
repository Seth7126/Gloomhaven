namespace System.Transactions;

/// <summary>Describes the current status of a distributed transaction.</summary>
public enum TransactionStatus
{
	/// <summary>The status of the transaction is unknown, because some participants must still be polled.</summary>
	Active,
	/// <summary>The transaction has been committed.</summary>
	Committed,
	/// <summary>The transaction has been rolled back.</summary>
	Aborted,
	/// <summary>The status of the transaction is unknown.</summary>
	InDoubt
}
