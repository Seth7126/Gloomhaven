using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Indicates the transaction status.</summary>
[Serializable]
[ComVisible(false)]
public enum TransactionStatus
{
	/// <summary>The transaction has committed.</summary>
	Commited,
	/// <summary>The transaction has neither committed nor aborted.</summary>
	LocallyOk,
	/// <summary>No transactions are being used through <see cref="M:System.EnterpriseServices.ServiceDomain.Enter(System.EnterpriseServices.ServiceConfig)" />.</summary>
	NoTransaction,
	/// <summary>The transaction is in the process of aborting.</summary>
	Aborting,
	/// <summary>The transaction is aborted.</summary>
	Aborted
}
