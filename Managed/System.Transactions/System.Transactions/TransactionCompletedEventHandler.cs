namespace System.Transactions;

/// <summary>Represents the method that handles the <see cref="E:System.Transactions.Transaction.TransactionCompleted" /> event of a <see cref="T:System.Transactions.Transaction" /> class.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The <see cref="T:System.Transactions.TransactionEventArgs" /> that contains the event data.</param>
public delegate void TransactionCompletedEventHandler(object sender, TransactionEventArgs e);
