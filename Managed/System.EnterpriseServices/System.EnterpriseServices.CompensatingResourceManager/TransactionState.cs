namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Specifies the state of the current Compensating Resource Manager (CRM) transaction.</summary>
[Serializable]
public enum TransactionState
{
	/// <summary>The transaction is active.</summary>
	Active,
	/// <summary>The transaction is commited.</summary>
	Committed,
	/// <summary>The transaction is aborted.</summary>
	Aborted,
	/// <summary>The transaction is in-doubt.</summary>
	Indoubt
}
