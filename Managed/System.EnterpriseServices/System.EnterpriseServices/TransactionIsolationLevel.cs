namespace System.EnterpriseServices;

/// <summary>Specifies the value of the <see cref="T:System.EnterpriseServices.TransactionAttribute" />.</summary>
[Serializable]
public enum TransactionIsolationLevel
{
	/// <summary>The isolation level for the component is obtained from the calling component's isolation level. If this is the root component, the isolation level used is <see cref="F:System.EnterpriseServices.TransactionIsolationLevel.Serializable" />.</summary>
	Any = 0,
	/// <summary>Shared locks are held while the data is being read to avoid reading modified data, but the data can be changed before the end of the transaction, resulting in non-repeatable reads or phantom data.</summary>
	ReadCommitted = 2,
	/// <summary>Shared locks are issued and no exclusive locks are honored.</summary>
	ReadUncommitted = 1,
	/// <summary>Locks are placed on all data that is used in a query, preventing other users from updating the data. Prevents non-repeatable reads, but phantom rows are still possible.</summary>
	RepeatableRead = 3,
	/// <summary>Prevents updating or inserting until the transaction is complete.</summary>
	Serializable = 4
}
