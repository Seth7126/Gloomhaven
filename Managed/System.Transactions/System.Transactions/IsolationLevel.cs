namespace System.Transactions;

/// <summary>Specifies the isolation level of a transaction.</summary>
public enum IsolationLevel
{
	/// <summary>Volatile data can be read but not modified, and no new data can be added during the transaction.</summary>
	Serializable,
	/// <summary>Volatile data can be read but not modified during the transaction. New data can be added during the transaction.</summary>
	RepeatableRead,
	/// <summary>Volatile data cannot be read during the transaction, but can be modified.</summary>
	ReadCommitted,
	/// <summary>Volatile data can be read and modified during the transaction.</summary>
	ReadUncommitted,
	/// <summary>Volatile data can be read. Before a transaction modifies data, it verifies if another transaction has changed the data after it was initially read. If the data has been updated, an error is raised. This allows a transaction to get to the previously committed value of the data.</summary>
	Snapshot,
	/// <summary>The pending changes from more highly isolated transactions cannot be overwritten.</summary>
	Chaos,
	/// <summary>A different isolation level than the one specified is being used, but the level cannot be determined. An exception is thrown if this value is set.</summary>
	Unspecified
}
