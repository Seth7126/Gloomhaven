namespace System.Transactions;

/// <summary>Represents a transaction that is not a root transaction, but can be escalated to be managed by the MSDTC.</summary>
public interface ISimpleTransactionSuperior : ITransactionPromoter
{
	/// <summary>Notifies an enlisted object that the transaction is being rolled back.</summary>
	void Rollback();
}
