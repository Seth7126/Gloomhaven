namespace System.Data;

/// <summary>Provides data for the <see cref="E:System.Data.Common.DataAdapter.FillError" /> event of a <see cref="T:System.Data.Common.DbDataAdapter" />.</summary>
/// <filterpriority>2</filterpriority>
public class FillErrorEventArgs : EventArgs
{
	private bool _continueFlag;

	private DataTable _dataTable;

	private Exception _errors;

	private object[] _values;

	/// <summary>Gets or sets a value indicating whether to continue the fill operation despite the error.</summary>
	/// <returns>true if the fill operation should continue; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool Continue
	{
		get
		{
			return _continueFlag;
		}
		set
		{
			_continueFlag = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.DataTable" /> being updated when the error occurred.</summary>
	/// <returns>The <see cref="T:System.Data.DataTable" /> being updated.</returns>
	/// <filterpriority>1</filterpriority>
	public DataTable DataTable => _dataTable;

	/// <summary>Gets the errors being handled.</summary>
	/// <returns>The errors being handled.</returns>
	/// <filterpriority>1</filterpriority>
	public Exception Errors
	{
		get
		{
			return _errors;
		}
		set
		{
			_errors = value;
		}
	}

	/// <summary>Gets the values for the row being updated when the error occurred.</summary>
	/// <returns>The values for the row being updated.</returns>
	/// <filterpriority>1</filterpriority>
	public object[] Values
	{
		get
		{
			object[] array = new object[_values.Length];
			for (int i = 0; i < _values.Length; i++)
			{
				array[i] = _values[i];
			}
			return array;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.FillErrorEventArgs" /> class.</summary>
	/// <param name="dataTable">The <see cref="T:System.Data.DataTable" /> being updated. </param>
	/// <param name="values">The values for the row being updated. </param>
	public FillErrorEventArgs(DataTable dataTable, object[] values)
	{
		_dataTable = dataTable;
		_values = values;
		if (_values == null)
		{
			_values = Array.Empty<object>();
		}
	}
}
