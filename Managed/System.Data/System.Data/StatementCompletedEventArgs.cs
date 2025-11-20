namespace System.Data;

/// <summary>Provides additional information for the <see cref="E:System.Data.SqlClient.SqlCommand.StatementCompleted" /> event.</summary>
/// <filterpriority>2</filterpriority>
public sealed class StatementCompletedEventArgs : EventArgs
{
	/// <summary>Indicates the number of rows affected by the statement that caused the <see cref="E:System.Data.SqlClient.SqlCommand.StatementCompleted" /> event to occur.</summary>
	/// <returns>The number of rows affected.</returns>
	/// <filterpriority>1</filterpriority>
	public int RecordCount { get; }

	/// <summary>Creates a new instance of the <see cref="T:System.Data.StatementCompletedEventArgs" /> class.</summary>
	/// <param name="recordCount">Indicates the number of rows affected by the statement that caused the <see cref="E:System.Data.SqlClient.SqlCommand.StatementCompleted" />  event to occur.</param>
	public StatementCompletedEventArgs(int recordCount)
	{
		RecordCount = recordCount;
	}
}
