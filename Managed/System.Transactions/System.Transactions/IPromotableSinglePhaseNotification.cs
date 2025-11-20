namespace System.Transactions;

/// <summary>Describes an object that acts as a commit delegate for a non-distributed transaction internal to a resource manager.</summary>
public interface IPromotableSinglePhaseNotification : ITransactionPromoter
{
	/// <summary>Notifies a transaction participant that enlistment has completed successfully.</summary>
	/// <exception cref="T:System.Transactions.TransactionException">An attempt to enlist or serialize a transaction.</exception>
	void Initialize();

	/// <summary>Notifies an enlisted object that the transaction is being rolled back.</summary>
	/// <param name="singlePhaseEnlistment">A <see cref="T:System.Transactions.SinglePhaseEnlistment" /> object used to send a response to the transaction manager.</param>
	void Rollback(SinglePhaseEnlistment singlePhaseEnlistment);

	/// <summary>Notifies an enlisted object that the transaction is being committed.</summary>
	/// <param name="singlePhaseEnlistment">A <see cref="T:System.Transactions.SinglePhaseEnlistment" /> interface used to send a response to the transaction manager.</param>
	void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment);
}
