namespace System.Transactions;

/// <summary>Represents a non-rooted transaction that can be delegated. This class cannot be inherited.</summary>
[Serializable]
public sealed class SubordinateTransaction : Transaction
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.SubordinateTransaction" /> class. </summary>
	/// <param name="isoLevel">The isolation level of the transaction</param>
	/// <param name="superior">A <see cref="T:System.Transactions.ISimpleTransactionSuperior" /></param>
	public SubordinateTransaction(IsolationLevel isoLevel, ISimpleTransactionSuperior superior)
		: base(isoLevel)
	{
		throw new NotImplementedException();
	}
}
