namespace System.Data;

/// <summary>Specifies the action to take with regard to the current and remaining rows during an <see cref="M:System.Data.Common.DbDataAdapter.Update(System.Data.DataSet)" />.</summary>
/// <filterpriority>2</filterpriority>
public enum UpdateStatus
{
	/// <summary>The <see cref="T:System.Data.Common.DataAdapter" /> is to continue proccessing rows.</summary>
	Continue,
	/// <summary>The event handler reports that the update should be treated as an error.</summary>
	ErrorsOccurred,
	/// <summary>The current row is not to be updated.</summary>
	SkipCurrentRow,
	/// <summary>The current row and all remaining rows are not to be updated.</summary>
	SkipAllRemainingRows
}
