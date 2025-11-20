namespace System.Transactions;

/// <summary>Describes a resource object that supports single phase commit optimization to participate in a transaction.</summary>
public interface ISinglePhaseNotification : IEnlistmentNotification
{
	/// <summary>Represents the resource manager's implementation of the callback for the single phase commit optimization.  </summary>
	/// <param name="singlePhaseEnlistment">A <see cref="T:System.Transactions.SinglePhaseEnlistment" />  used to send a response to the transaction manager.</param>
	void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment);
}
