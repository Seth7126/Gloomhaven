namespace System.Transactions;

/// <summary>Specifies how distributed transactions interact with COM+ transactions.</summary>
public enum EnterpriseServicesInteropOption
{
	/// <summary>There is no synchronization between <see cref="P:System.EnterpriseServices.ContextUtil.Transaction" /> and <see cref="P:System.Transactions.Transaction.Current" />.  </summary>
	None,
	/// <summary>Search for an existing COM+ context and synchronize with it if one exists.</summary>
	Automatic,
	/// <summary>The <see cref="N:System.EnterpriseServices" /> context (which can be retrieved by calling the static method <see cref="P:System.EnterpriseServices.ContextUtil.Transaction" /> of the <see cref="T:System.EnterpriseServices.ContextUtil" /> class) and the <see cref="N:System.Transactions" /> ambient transaction (which can be retrieved by calling the static method <see cref="P:System.Transactions.Transaction.Current" /> of the <see cref="T:System.Transactions.Transaction" /> class) are always synchronized. This introduces a performance penalty because new <see cref="N:System.EnterpriseServices" /> contexts may need to be created.</summary>
	Full
}
