namespace System.Data;

/// <summary>Provides data for the <see cref="E:System.Data.DataTable.ColumnChanging" /> event.</summary>
/// <filterpriority>2</filterpriority>
public class DataColumnChangeEventArgs : EventArgs
{
	private DataColumn _column;

	/// <summary>Gets the <see cref="T:System.Data.DataColumn" /> with a changing value.</summary>
	/// <returns>The <see cref="T:System.Data.DataColumn" /> with a changing value.</returns>
	/// <filterpriority>1</filterpriority>
	public DataColumn Column => _column;

	/// <summary>Gets the <see cref="T:System.Data.DataRow" /> of the column with a changing value.</summary>
	/// <returns>The <see cref="T:System.Data.DataRow" /> of the column with a changing value.</returns>
	/// <filterpriority>1</filterpriority>
	public DataRow Row { get; }

	/// <summary>Gets or sets the proposed new value for the column.</summary>
	/// <returns>The proposed value, of type <see cref="T:System.Object" />.</returns>
	/// <filterpriority>1</filterpriority>
	public object ProposedValue { get; set; }

	internal DataColumnChangeEventArgs(DataRow row)
	{
		Row = row;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataColumnChangeEventArgs" /> class.</summary>
	/// <param name="row">The <see cref="T:System.Data.DataRow" /> of the column with the changing value. </param>
	/// <param name="column">The <see cref="T:System.Data.DataColumn" /> with the changing value. </param>
	/// <param name="value">The new value. </param>
	public DataColumnChangeEventArgs(DataRow row, DataColumn column, object value)
	{
		Row = row;
		_column = column;
		ProposedValue = value;
	}

	internal void InitializeColumnChangeEvent(DataColumn column, object value)
	{
		_column = column;
		ProposedValue = value;
	}
}
