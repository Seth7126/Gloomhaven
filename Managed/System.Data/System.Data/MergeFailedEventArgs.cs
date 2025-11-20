namespace System.Data;

/// <summary>Occurs when a target and source DataRow have the same primary key value, and the <see cref="P:System.Data.DataSet.EnforceConstraints" /> property is set to true.</summary>
/// <filterpriority>2</filterpriority>
public class MergeFailedEventArgs : EventArgs
{
	/// <summary>Returns the <see cref="T:System.Data.DataTable" /> object.</summary>
	/// <returns>The <see cref="T:System.Data.DataTable" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public DataTable Table { get; }

	/// <summary>Returns a description of the merge conflict.</summary>
	/// <returns>A description of the merge conflict.</returns>
	/// <filterpriority>1</filterpriority>
	public string Conflict { get; }

	/// <summary>Initializes a new instance of a <see cref="T:System.Data.MergeFailedEventArgs" /> class with the <see cref="T:System.Data.DataTable" /> and a description of the merge conflict.</summary>
	/// <param name="table">The <see cref="T:System.Data.DataTable" /> object. </param>
	/// <param name="conflict">A description of the merge conflict. </param>
	public MergeFailedEventArgs(DataTable table, string conflict)
	{
		Table = table;
		Conflict = conflict;
	}
}
