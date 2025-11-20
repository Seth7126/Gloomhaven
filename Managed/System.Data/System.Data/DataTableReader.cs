using System.Collections;
using System.Data.Common;
using System.Globalization;

namespace System.Data;

/// <summary>The <see cref="T:System.Data.DataTableReader" /> obtains the contents of one or more <see cref="T:System.Data.DataTable" /> objects in the form of one or more read-only, forward-only result sets. </summary>
/// <filterpriority>1</filterpriority>
public sealed class DataTableReader : DbDataReader
{
	private readonly DataTable[] _tables;

	private bool _isOpen = true;

	private DataTable _schemaTable;

	private int _tableCounter = -1;

	private int _rowCounter = -1;

	private DataTable _currentDataTable;

	private DataRow _currentDataRow;

	private bool _hasRows = true;

	private bool _reachEORows;

	private bool _currentRowRemoved;

	private bool _schemaIsChanged;

	private bool _started;

	private bool _readerIsInvalid;

	private DataTableReaderListener _listener;

	private bool _tableCleared;

	private bool ReaderIsInvalid
	{
		get
		{
			return _readerIsInvalid;
		}
		set
		{
			if (_readerIsInvalid != value)
			{
				_readerIsInvalid = value;
				if (_readerIsInvalid && _listener != null)
				{
					_listener.CleanUp();
				}
			}
		}
	}

	private bool IsSchemaChanged
	{
		get
		{
			return _schemaIsChanged;
		}
		set
		{
			if (value && _schemaIsChanged != value)
			{
				_schemaIsChanged = value;
				if (_listener != null)
				{
					_listener.CleanUp();
				}
			}
		}
	}

	internal DataTable CurrentDataTable => _currentDataTable;

	/// <summary>The depth of nesting for the current row of the <see cref="T:System.Data.DataTableReader" />.</summary>
	/// <returns>The depth of nesting for the current row; always zero.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int Depth
	{
		get
		{
			ValidateOpen("Depth");
			ValidateReader();
			return 0;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.DataTableReader" /> is closed.</summary>
	/// <returns>Returns true if the <see cref="T:System.Data.DataTableReader" /> is closed; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool IsClosed => !_isOpen;

	/// <summary>Gets the number of rows inserted, changed, or deleted by execution of the SQL statement.</summary>
	/// <returns>The <see cref="T:System.Data.DataTableReader" /> does not support this property and always returns 0.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int RecordsAffected
	{
		get
		{
			ValidateReader();
			return 0;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.DataTableReader" /> contains one or more rows.</summary>
	/// <returns>true if the <see cref="T:System.Data.DataTableReader" /> contains one or more rows; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to retrieve information about a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool HasRows
	{
		get
		{
			ValidateOpen("HasRows");
			ValidateReader();
			return _hasRows;
		}
	}

	/// <summary>Gets the value of the specified column in its native format given the column ordinal.</summary>
	/// <returns>The value of the specified column in its native format.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[int ordinal]
	{
		get
		{
			ValidateOpen("Item");
			ValidateReader();
			if (_currentDataRow == null || _currentDataRow.RowState == DataRowState.Deleted)
			{
				ReaderIsInvalid = true;
				throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
			}
			try
			{
				return _currentDataRow[ordinal];
			}
			catch (IndexOutOfRangeException e)
			{
				ExceptionBuilder.TraceExceptionWithoutRethrow(e);
				throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
			}
		}
	}

	/// <summary>Gets the value of the specified column in its native format given the column name.</summary>
	/// <returns>The value of the specified column in its native format.</returns>
	/// <param name="name">The name of the column. </param>
	/// <exception cref="T:System.ArgumentException">The name specified is not a valid column name. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[string name]
	{
		get
		{
			ValidateOpen("Item");
			ValidateReader();
			if (_currentDataRow == null || _currentDataRow.RowState == DataRowState.Deleted)
			{
				ReaderIsInvalid = true;
				throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
			}
			return _currentDataRow[name];
		}
	}

	/// <summary>Returns the number of columns in the current row.</summary>
	/// <returns>When not positioned in a valid result set, 0; otherwise the number of columns in the current row. </returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to retrieve the field count in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int FieldCount
	{
		get
		{
			ValidateOpen("FieldCount");
			ValidateReader();
			return _currentDataTable.Columns.Count;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataTableReader" /> class by using data from the supplied <see cref="T:System.Data.DataTable" />.</summary>
	/// <param name="dataTable">The <see cref="T:System.Data.DataTable" /> from which the new <see cref="T:System.Data.DataTableReader" /> obtains its result set. </param>
	public DataTableReader(DataTable dataTable)
	{
		if (dataTable == null)
		{
			throw ExceptionBuilder.ArgumentNull("DataTable");
		}
		_tables = new DataTable[1] { dataTable };
		Init();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataTableReader" /> class using the supplied array of <see cref="T:System.Data.DataTable" /> objects.</summary>
	/// <param name="dataTables">The array of <see cref="T:System.Data.DataTable" /> objects that supplies the results for the new <see cref="T:System.Data.DataTableReader" /> object. </param>
	public DataTableReader(DataTable[] dataTables)
	{
		if (dataTables == null)
		{
			throw ExceptionBuilder.ArgumentNull("DataTable");
		}
		if (dataTables.Length == 0)
		{
			throw ExceptionBuilder.DataTableReaderArgumentIsEmpty();
		}
		_tables = new DataTable[dataTables.Length];
		for (int i = 0; i < dataTables.Length; i++)
		{
			if (dataTables[i] == null)
			{
				throw ExceptionBuilder.ArgumentNull("DataTable");
			}
			_tables[i] = dataTables[i];
		}
		Init();
	}

	private void Init()
	{
		_tableCounter = 0;
		_reachEORows = false;
		_schemaIsChanged = false;
		_currentDataTable = _tables[_tableCounter];
		_hasRows = _currentDataTable.Rows.Count > 0;
		ReaderIsInvalid = false;
		_listener = new DataTableReaderListener(this);
	}

	/// <summary>Closes the current <see cref="T:System.Data.DataTableReader" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override void Close()
	{
		if (_isOpen)
		{
			if (_listener != null)
			{
				_listener.CleanUp();
			}
			_listener = null;
			_schemaTable = null;
			_isOpen = false;
		}
	}

	/// <summary>Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.DataTableReader" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that describes the column metadata.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.DataTableReader" /> is closed. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override DataTable GetSchemaTable()
	{
		ValidateOpen("GetSchemaTable");
		ValidateReader();
		if (_schemaTable == null)
		{
			_schemaTable = GetSchemaTableFromDataTable(_currentDataTable);
		}
		return _schemaTable;
	}

	/// <summary>Advances the <see cref="T:System.Data.DataTableReader" /> to the next result set, if any.</summary>
	/// <returns>true if there was another result set; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to navigate within a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool NextResult()
	{
		ValidateOpen("NextResult");
		if (_tableCounter == _tables.Length - 1)
		{
			return false;
		}
		_currentDataTable = _tables[++_tableCounter];
		if (_listener != null)
		{
			_listener.UpdataTable(_currentDataTable);
		}
		_schemaTable = null;
		_rowCounter = -1;
		_currentRowRemoved = false;
		_reachEORows = false;
		_schemaIsChanged = false;
		_started = false;
		ReaderIsInvalid = false;
		_tableCleared = false;
		_hasRows = _currentDataTable.Rows.Count > 0;
		return true;
	}

	/// <summary>Advances the <see cref="T:System.Data.DataTableReader" /> to the next record.</summary>
	/// <returns>true if there was another row to read; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool Read()
	{
		if (!_started)
		{
			_started = true;
		}
		ValidateOpen("Read");
		ValidateReader();
		if (_reachEORows)
		{
			return false;
		}
		if (_rowCounter >= _currentDataTable.Rows.Count - 1)
		{
			_reachEORows = true;
			if (_listener != null)
			{
				_listener.CleanUp();
			}
			return false;
		}
		_rowCounter++;
		ValidateRow(_rowCounter);
		_currentDataRow = _currentDataTable.Rows[_rowCounter];
		while (_currentDataRow.RowState == DataRowState.Deleted)
		{
			_rowCounter++;
			if (_rowCounter == _currentDataTable.Rows.Count)
			{
				_reachEORows = true;
				if (_listener != null)
				{
					_listener.CleanUp();
				}
				return false;
			}
			ValidateRow(_rowCounter);
			_currentDataRow = _currentDataTable.Rows[_rowCounter];
		}
		if (_currentRowRemoved)
		{
			_currentRowRemoved = false;
		}
		return true;
	}

	/// <summary>Gets the type of the specified column in provider-specific format.</summary>
	/// <returns>The <see cref="T:System.Type" /> that is the data type of the object.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	public override Type GetProviderSpecificFieldType(int ordinal)
	{
		ValidateOpen("GetProviderSpecificFieldType");
		ValidateReader();
		return GetFieldType(ordinal);
	}

	/// <summary>Gets the value of the specified column in provider-specific format.</summary>
	/// <returns>The value of the specified column in provider-specific format.</returns>
	/// <param name="ordinal">The zero-based number of the column whose value is retrieved. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /></exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object GetProviderSpecificValue(int ordinal)
	{
		ValidateOpen("GetProviderSpecificValue");
		ValidateReader();
		return GetValue(ordinal);
	}

	/// <summary>Fills the supplied array with provider-specific type information for all the columns in the <see cref="T:System.Data.DataTableReader" />.</summary>
	/// <returns>The number of column values copied into the array.</returns>
	/// <param name="values">An array of objects to be filled in with type information for the columns in the <see cref="T:System.Data.DataTableReader" />. </param>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetProviderSpecificValues(object[] values)
	{
		ValidateOpen("GetProviderSpecificValues");
		ValidateReader();
		return GetValues(values);
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.Boolean" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a Boolean. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool GetBoolean(int ordinal)
	{
		ValidateState("GetBoolean");
		ValidateReader();
		try
		{
			return (bool)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a byte.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a byte. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override byte GetByte(int ordinal)
	{
		ValidateState("GetByte");
		ValidateReader();
		try
		{
			return (byte)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Reads a stream of bytes starting at the specified column offset into the buffer as an array starting at the specified buffer offset.</summary>
	/// <returns>The actual number of bytes read.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <param name="dataIndex">The index within the field from which to start the read operation. </param>
	/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
	/// <param name="bufferIndex">The index within the buffer at which to start placing the data. </param>
	/// <param name="length">The maximum length to copy into the buffer. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a byte array. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		ValidateState("GetBytes");
		ValidateReader();
		byte[] array;
		try
		{
			array = (byte[])_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
		if (buffer == null)
		{
			return array.Length;
		}
		int num = (int)dataIndex;
		int num2 = Math.Min(array.Length - num, length);
		if (num < 0)
		{
			throw ADP.InvalidSourceBufferIndex(array.Length, num, "dataIndex");
		}
		if (bufferIndex < 0 || (bufferIndex > 0 && bufferIndex >= buffer.Length))
		{
			throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
		}
		if (0 < num2)
		{
			Array.Copy(array, dataIndex, buffer, bufferIndex, num2);
		}
		else
		{
			if (length < 0)
			{
				throw ADP.InvalidDataLength(length);
			}
			num2 = 0;
		}
		return num2;
	}

	/// <summary>Gets the value of the specified column as a character.</summary>
	/// <returns>The value of the column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified field does not contain a character. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override char GetChar(int ordinal)
	{
		ValidateState("GetChar");
		ValidateReader();
		try
		{
			return (char)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Returns the value of the specified column as a character array.</summary>
	/// <returns>The actual number of characters read.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <param name="dataIndex">The index within the field from which to start the read operation. </param>
	/// <param name="buffer">The buffer into which to read the stream of chars. </param>
	/// <param name="bufferIndex">The index within the buffer at which to start placing the data. </param>
	/// <param name="length">The maximum length to copy into the buffer. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a character array. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		ValidateState("GetChars");
		ValidateReader();
		char[] array;
		try
		{
			array = (char[])_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
		if (buffer == null)
		{
			return array.Length;
		}
		int num = (int)dataIndex;
		int num2 = Math.Min(array.Length - num, length);
		if (num < 0)
		{
			throw ADP.InvalidSourceBufferIndex(array.Length, num, "dataIndex");
		}
		if (bufferIndex < 0 || (bufferIndex > 0 && bufferIndex >= buffer.Length))
		{
			throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
		}
		if (0 < num2)
		{
			Array.Copy(array, dataIndex, buffer, bufferIndex, num2);
		}
		else
		{
			if (length < 0)
			{
				throw ADP.InvalidDataLength(length);
			}
			num2 = 0;
		}
		return num2;
	}

	/// <summary>Gets a string representing the data type of the specified column.</summary>
	/// <returns>A string representing the column's data type.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetDataTypeName(int ordinal)
	{
		ValidateOpen("GetDataTypeName");
		ValidateReader();
		return GetFieldType(ordinal).Name;
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a DateTime value. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DateTime GetDateTime(int ordinal)
	{
		ValidateState("GetDateTime");
		ValidateReader();
		try
		{
			return (DateTime)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a Decimal value. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override decimal GetDecimal(int ordinal)
	{
		ValidateState("GetDecimal");
		ValidateReader();
		try
		{
			return (decimal)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the column as a double-precision floating point number.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed DataTableReader.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a double-precision floating point number. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override double GetDouble(int ordinal)
	{
		ValidateState("GetDouble");
		ValidateReader();
		try
		{
			return (double)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the <see cref="T:System.Type" /> that is the data type of the object.</summary>
	/// <returns>The <see cref="T:System.Type" /> that is the data type of the object.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override Type GetFieldType(int ordinal)
	{
		ValidateOpen("GetFieldType");
		ValidateReader();
		try
		{
			return _currentDataTable.Columns[ordinal].DataType;
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a single-precision floating point number.</summary>
	/// <returns>The value of the column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a single-precision floating point number. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override float GetFloat(int ordinal)
	{
		ValidateState("GetFloat");
		ValidateReader();
		try
		{
			return (float)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a globally-unique identifier (GUID).</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a GUID. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override Guid GetGuid(int ordinal)
	{
		ValidateState("GetGuid");
		ValidateReader();
		try
		{
			return (Guid)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a 16-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a 16-bit signed integer. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override short GetInt16(int ordinal)
	{
		ValidateState("GetInt16");
		ValidateReader();
		try
		{
			return (short)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a 32-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a 32-bit signed integer value. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetInt32(int ordinal)
	{
		ValidateState("GetInt32");
		ValidateReader();
		try
		{
			return (int)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a 64-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a 64-bit signed integer value. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetInt64(int ordinal)
	{
		ValidateState("GetInt64");
		ValidateReader();
		try
		{
			return (long)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.String" />.</summary>
	/// <returns>The name of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetName(int ordinal)
	{
		ValidateOpen("GetName");
		ValidateReader();
		try
		{
			return _currentDataTable.Columns[ordinal].ColumnName;
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the column ordinal, given the name of the column.</summary>
	/// <returns>The zero-based column ordinal.</returns>
	/// <param name="name">The name of the column. </param>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.ArgumentException">The name specified is not a valid column name. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetOrdinal(string name)
	{
		ValidateOpen("GetOrdinal");
		ValidateReader();
		DataColumn dataColumn = _currentDataTable.Columns[name];
		if (dataColumn != null)
		{
			return dataColumn.Ordinal;
		}
		throw ExceptionBuilder.ColumnNotInTheTable(name, _currentDataTable.TableName);
	}

	/// <summary>Gets the value of the specified column as a string.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The specified column does not contain a string. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetString(int ordinal)
	{
		ValidateState("GetString");
		ValidateReader();
		try
		{
			return (string)_currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Gets the value of the specified column in its native format.</summary>
	/// <returns>The value of the specified column. This method returns DBNull for null columns.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access columns in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object GetValue(int ordinal)
	{
		ValidateState("GetValue");
		ValidateReader();
		try
		{
			return _currentDataRow[ordinal];
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Populates an array of objects with the column values of the current row.</summary>
	/// <returns>The number of column values copied into the array.</returns>
	/// <param name="values">An array of <see cref="T:System.Object" /> into which to copy the column values from the <see cref="T:System.Data.DataTableReader" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1. </exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetValues(object[] values)
	{
		ValidateState("GetValues");
		ValidateReader();
		if (values == null)
		{
			throw ExceptionBuilder.ArgumentNull("values");
		}
		Array.Copy(_currentDataRow.ItemArray, values, (_currentDataRow.ItemArray.Length > values.Length) ? values.Length : _currentDataRow.ItemArray.Length);
		if (_currentDataRow.ItemArray.Length <= values.Length)
		{
			return _currentDataRow.ItemArray.Length;
		}
		return values.Length;
	}

	/// <summary>Gets a value that indicates whether the column contains non-existent or missing values.</summary>
	/// <returns>true if the specified column value is equivalent to <see cref="T:System.DBNull" />; otherwise, false.</returns>
	/// <param name="ordinal">The zero-based column ordinal </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index passed was outside the range of 0 to <see cref="P:System.Data.DataTableReader.FieldCount" /> - 1.</exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">An attempt was made to retrieve data from a deleted row.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" /> .</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool IsDBNull(int ordinal)
	{
		ValidateState("IsDBNull");
		ValidateReader();
		try
		{
			return _currentDataRow.IsNull(ordinal);
		}
		catch (IndexOutOfRangeException e)
		{
			ExceptionBuilder.TraceExceptionWithoutRethrow(e);
			throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
		}
	}

	/// <summary>Returns an enumerator that can be used to iterate through the item collection. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that represents the item collection.</returns>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to read or access a column in a closed <see cref="T:System.Data.DataTableReader" />.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override IEnumerator GetEnumerator()
	{
		ValidateOpen("GetEnumerator");
		return new DbEnumerator((IDataReader)this);
	}

	internal static DataTable GetSchemaTableFromDataTable(DataTable table)
	{
		if (table == null)
		{
			throw ExceptionBuilder.ArgumentNull("DataTable");
		}
		DataTable dataTable = new DataTable("SchemaTable");
		dataTable.Locale = CultureInfo.InvariantCulture;
		DataColumn column = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
		DataColumn column2 = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
		DataColumn dataColumn = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
		DataColumn column3 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
		DataColumn column4 = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
		DataColumn column5 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
		DataColumn column6 = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
		DataColumn dataColumn2 = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
		DataColumn column7 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
		DataColumn dataColumn3 = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
		DataColumn dataColumn4 = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
		DataColumn column8 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
		DataColumn dataColumn5 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
		DataColumn dataColumn6 = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
		DataColumn column9 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
		DataColumn dataColumn7 = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
		DataColumn dataColumn8 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
		DataColumn column10 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
		DataColumn dataColumn9 = new DataColumn(SchemaTableOptionalColumn.AutoIncrementSeed, typeof(long));
		DataColumn dataColumn10 = new DataColumn(SchemaTableOptionalColumn.AutoIncrementStep, typeof(long));
		DataColumn column11 = new DataColumn(SchemaTableOptionalColumn.DefaultValue, typeof(object));
		DataColumn column12 = new DataColumn(SchemaTableOptionalColumn.Expression, typeof(string));
		DataColumn column13 = new DataColumn(SchemaTableOptionalColumn.ColumnMapping, typeof(MappingType));
		DataColumn dataColumn11 = new DataColumn(SchemaTableOptionalColumn.BaseTableNamespace, typeof(string));
		DataColumn column14 = new DataColumn(SchemaTableOptionalColumn.BaseColumnNamespace, typeof(string));
		dataColumn.DefaultValue = -1;
		if (table.DataSet != null)
		{
			dataColumn7.DefaultValue = table.DataSet.DataSetName;
		}
		dataColumn8.DefaultValue = table.TableName;
		dataColumn11.DefaultValue = table.Namespace;
		dataColumn4.DefaultValue = false;
		dataColumn2.DefaultValue = false;
		dataColumn3.DefaultValue = false;
		dataColumn5.DefaultValue = false;
		dataColumn6.DefaultValue = false;
		dataColumn9.DefaultValue = 0;
		dataColumn10.DefaultValue = 1;
		dataTable.Columns.Add(column);
		dataTable.Columns.Add(column2);
		dataTable.Columns.Add(dataColumn);
		dataTable.Columns.Add(column3);
		dataTable.Columns.Add(column4);
		dataTable.Columns.Add(column5);
		dataTable.Columns.Add(column6);
		dataTable.Columns.Add(dataColumn2);
		dataTable.Columns.Add(column7);
		dataTable.Columns.Add(dataColumn3);
		dataTable.Columns.Add(dataColumn4);
		dataTable.Columns.Add(column8);
		dataTable.Columns.Add(dataColumn5);
		dataTable.Columns.Add(dataColumn6);
		dataTable.Columns.Add(dataColumn7);
		dataTable.Columns.Add(column9);
		dataTable.Columns.Add(dataColumn8);
		dataTable.Columns.Add(column10);
		dataTable.Columns.Add(dataColumn9);
		dataTable.Columns.Add(dataColumn10);
		dataTable.Columns.Add(column11);
		dataTable.Columns.Add(column12);
		dataTable.Columns.Add(column13);
		dataTable.Columns.Add(dataColumn11);
		dataTable.Columns.Add(column14);
		foreach (DataColumn column15 in table.Columns)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow[column] = column15.ColumnName;
			dataRow[column2] = column15.Ordinal;
			dataRow[column5] = column15.DataType;
			if (column15.DataType == typeof(string))
			{
				dataRow[dataColumn] = column15.MaxLength;
			}
			dataRow[column7] = column15.AllowDBNull;
			dataRow[dataColumn3] = column15.ReadOnly;
			dataRow[column8] = column15.Unique;
			if (column15.AutoIncrement)
			{
				dataRow[dataColumn6] = true;
				dataRow[dataColumn9] = column15.AutoIncrementSeed;
				dataRow[dataColumn10] = column15.AutoIncrementStep;
			}
			if (column15.DefaultValue != DBNull.Value)
			{
				dataRow[column11] = column15.DefaultValue;
			}
			if (column15.Expression.Length != 0)
			{
				bool flag = false;
				DataColumn[] dependency = column15.DataExpression.GetDependency();
				for (int i = 0; i < dependency.Length; i++)
				{
					if (dependency[i].Table != table)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					dataRow[column12] = column15.Expression;
				}
			}
			dataRow[column13] = column15.ColumnMapping;
			dataRow[column10] = column15.ColumnName;
			dataRow[column14] = column15.Namespace;
			dataTable.Rows.Add(dataRow);
		}
		DataColumn[] primaryKey = table.PrimaryKey;
		foreach (DataColumn dataColumn13 in primaryKey)
		{
			dataTable.Rows[dataColumn13.Ordinal][dataColumn5] = true;
		}
		dataTable.AcceptChanges();
		return dataTable;
	}

	private void ValidateOpen(string caller)
	{
		if (!_isOpen)
		{
			throw ADP.DataReaderClosed(caller);
		}
	}

	private void ValidateReader()
	{
		if (ReaderIsInvalid)
		{
			throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
		}
		if (IsSchemaChanged)
		{
			throw ExceptionBuilder.DataTableReaderSchemaIsInvalid(_currentDataTable.TableName);
		}
	}

	private void ValidateState(string caller)
	{
		ValidateOpen(caller);
		if (_tableCleared)
		{
			throw ExceptionBuilder.EmptyDataTableReader(_currentDataTable.TableName);
		}
		if (_currentDataRow == null || _currentDataTable == null)
		{
			ReaderIsInvalid = true;
			throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
		}
		if (_currentDataRow.RowState == DataRowState.Deleted || _currentDataRow.RowState == DataRowState.Detached || _currentRowRemoved)
		{
			throw ExceptionBuilder.InvalidCurrentRowInDataTableReader();
		}
		if (0 > _rowCounter || _currentDataTable.Rows.Count <= _rowCounter)
		{
			ReaderIsInvalid = true;
			throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
		}
	}

	private void ValidateRow(int rowPosition)
	{
		if (ReaderIsInvalid)
		{
			throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
		}
		if (0 > rowPosition || _currentDataTable.Rows.Count <= rowPosition)
		{
			ReaderIsInvalid = true;
			throw ExceptionBuilder.InvalidDataTableReader(_currentDataTable.TableName);
		}
	}

	internal void SchemaChanged()
	{
		IsSchemaChanged = true;
	}

	internal void DataTableCleared()
	{
		if (_started)
		{
			_rowCounter = -1;
			if (!_reachEORows)
			{
				_currentRowRemoved = true;
			}
		}
	}

	internal void DataChanged(DataRowChangeEventArgs args)
	{
		if (!_started || (_rowCounter == -1 && !_tableCleared))
		{
			return;
		}
		switch (args.Action)
		{
		case DataRowAction.Add:
			ValidateRow(_rowCounter + 1);
			if (_currentDataRow == _currentDataTable.Rows[_rowCounter + 1])
			{
				_rowCounter++;
			}
			break;
		case DataRowAction.Delete:
		case DataRowAction.Rollback:
		case DataRowAction.Commit:
			if (args.Row.RowState != DataRowState.Detached)
			{
				break;
			}
			if (args.Row != _currentDataRow)
			{
				if (_rowCounter != 0)
				{
					ValidateRow(_rowCounter - 1);
					if (_currentDataRow == _currentDataTable.Rows[_rowCounter - 1])
					{
						_rowCounter--;
					}
				}
			}
			else
			{
				_currentRowRemoved = true;
				if (_rowCounter > 0)
				{
					_rowCounter--;
					_currentDataRow = _currentDataTable.Rows[_rowCounter];
				}
				else
				{
					_rowCounter = -1;
					_currentDataRow = null;
				}
			}
			break;
		}
	}
}
