namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IDesignerHost.TransactionClosed" /> and <see cref="E:System.ComponentModel.Design.IDesignerHost.TransactionClosing" /> events.</summary>
public class DesignerTransactionCloseEventArgs : EventArgs
{
	/// <summary>Indicates whether the designer called <see cref="M:System.ComponentModel.Design.DesignerTransaction.Commit" /> on the transaction.</summary>
	/// <returns>true if the designer called <see cref="M:System.ComponentModel.Design.DesignerTransaction.Commit" /> on the transaction; otherwise, false.</returns>
	public bool TransactionCommitted { get; }

	/// <summary>Gets a value indicating whether this is the last transaction to close.</summary>
	/// <returns>true, if this is the last transaction to close; otherwise, false. </returns>
	public bool LastTransaction { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerTransactionCloseEventArgs" /> class, using the specified value that indicates whether the designer called <see cref="M:System.ComponentModel.Design.DesignerTransaction.Commit" /> on the transaction.</summary>
	/// <param name="commit">A value indicating whether the transaction was committed.</param>
	[Obsolete("This constructor is obsolete. Use DesignerTransactionCloseEventArgs(bool, bool) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	public DesignerTransactionCloseEventArgs(bool commit)
		: this(commit, lastTransaction: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerTransactionCloseEventArgs" /> class. </summary>
	/// <param name="commit">A value indicating whether the transaction was committed.</param>
	/// <param name="lastTransaction">true if this is the last transaction to close; otherwise, false.</param>
	public DesignerTransactionCloseEventArgs(bool commit, bool lastTransaction)
	{
		TransactionCommitted = commit;
		LastTransaction = lastTransaction;
	}
}
