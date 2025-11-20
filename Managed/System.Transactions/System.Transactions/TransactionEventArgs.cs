namespace System.Transactions;

/// <summary>Provides data for the following transaction events: <see cref="E:System.Transactions.TransactionManager.DistributedTransactionStarted" />, <see cref="E:System.Transactions.Transaction.TransactionCompleted" />.</summary>
public class TransactionEventArgs : EventArgs
{
	private Transaction transaction;

	/// <summary>Gets the transaction for which event status is provided.</summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" /> for which event status is provided.</returns>
	public Transaction Transaction => transaction;

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionEventArgs" /> class. </summary>
	public TransactionEventArgs()
	{
	}

	internal TransactionEventArgs(Transaction transaction)
		: this()
	{
		this.transaction = transaction;
	}
}
