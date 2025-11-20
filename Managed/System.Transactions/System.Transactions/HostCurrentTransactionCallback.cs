namespace System.Transactions;

/// <summary>Provides a mechanism for the hosting environment to supply its own default notion of <see cref="P:System.Transactions.Transaction.Current" />.</summary>
/// <returns>A <see cref="T:System.Transactions.Transaction" /> object.</returns>
public delegate Transaction HostCurrentTransactionCallback();
