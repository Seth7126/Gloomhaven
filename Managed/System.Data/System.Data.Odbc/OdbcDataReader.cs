using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity;

namespace System.Data.Odbc;

/// <summary>Provides a way of reading a forward-only stream of data rows from a data source. This class cannot be inherited. </summary>
/// <filterpriority>1</filterpriority>
public sealed class OdbcDataReader : DbDataReader
{
	private enum HasRowsStatus
	{
		DontKnow,
		HasRows,
		HasNoRows
	}

	internal sealed class QualifiedTableName
	{
		private string _catalogName;

		private string _schemaName;

		private string _tableName;

		private string _quotedTableName;

		private string _quoteChar;

		internal string Catalog => _catalogName;

		internal string Schema => _schemaName;

		internal string Table
		{
			get
			{
				return _tableName;
			}
			set
			{
				_quotedTableName = value;
				_tableName = UnQuote(value);
			}
		}

		internal string QuotedTable => _quotedTableName;

		internal string GetTable(bool flag)
		{
			if (!flag)
			{
				return Table;
			}
			return QuotedTable;
		}

		internal QualifiedTableName(string quoteChar)
		{
			_quoteChar = quoteChar;
		}

		internal QualifiedTableName(string quoteChar, string qualifiedname)
		{
			_quoteChar = quoteChar;
			string[] array = ParseProcedureName(qualifiedname, quoteChar, quoteChar);
			_catalogName = UnQuote(array[1]);
			_schemaName = UnQuote(array[2]);
			_quotedTableName = array[3];
			_tableName = UnQuote(array[3]);
		}

		private string UnQuote(string str)
		{
			if (str != null && str.Length > 0)
			{
				char c = _quoteChar[0];
				if (str[0] == c && str.Length > 1 && str[str.Length - 1] == c)
				{
					str = str.Substring(1, str.Length - 2);
				}
			}
			return str;
		}

		internal static string[] ParseProcedureName(string name, string quotePrefix, string quoteSuffix)
		{
			string[] array = new string[4];
			if (!string.IsNullOrEmpty(name))
			{
				bool flag = !string.IsNullOrEmpty(quotePrefix) && !string.IsNullOrEmpty(quoteSuffix);
				int i = 0;
				int j;
				for (j = 0; j < array.Length; j++)
				{
					if (i >= name.Length)
					{
						break;
					}
					int num = i;
					if (flag && name.IndexOf(quotePrefix, i, quotePrefix.Length, StringComparison.Ordinal) == i)
					{
						for (i += quotePrefix.Length; i < name.Length; i += quoteSuffix.Length)
						{
							i = name.IndexOf(quoteSuffix, i, StringComparison.Ordinal);
							if (i < 0)
							{
								i = name.Length;
								break;
							}
							i += quoteSuffix.Length;
							if (i >= name.Length || name.IndexOf(quoteSuffix, i, quoteSuffix.Length, StringComparison.Ordinal) != i)
							{
								break;
							}
						}
					}
					if (i < name.Length)
					{
						i = name.IndexOf(".", i, StringComparison.Ordinal);
						if (i < 0 || j == array.Length - 1)
						{
							i = name.Length;
						}
					}
					array[j] = name.Substring(num, i - num);
					i += ".".Length;
				}
				int num2 = array.Length - 1;
				while (0 <= num2)
				{
					array[num2] = ((0 < j) ? array[--j] : null);
					num2--;
				}
			}
			return array;
		}
	}

	private sealed class MetaData
	{
		internal int ordinal;

		internal TypeMap typemap;

		internal SQLLEN size;

		internal byte precision;

		internal byte scale;

		internal bool isAutoIncrement;

		internal bool isUnique;

		internal bool isReadOnly;

		internal bool isNullable;

		internal bool isRowVersion;

		internal bool isLong;

		internal bool isKeyColumn;

		internal string baseSchemaName;

		internal string baseCatalogName;

		internal string baseTableName;

		internal string baseColumnName;
	}

	private OdbcCommand _command;

	private int _recordAffected;

	private FieldNameLookup _fieldNameLookup;

	private DbCache _dataCache;

	private HasRowsStatus _hasRows;

	private bool _isClosed;

	private bool _isRead;

	private bool _isValidResult;

	private bool _noMoreResults;

	private bool _noMoreRows;

	private bool _skipReadOnce;

	private int _hiddenColumns;

	private CommandBehavior _commandBehavior;

	private int _row;

	private int _column;

	private long _sequentialBytesRead;

	private static int s_objectTypeCount;

	internal readonly int ObjectID;

	private MetaData[] _metadata;

	private DataTable _schemaTable;

	private string _cmdText;

	private CMDWrapper _cmdWrapper;

	private CNativeBuffer Buffer
	{
		get
		{
			CNativeBuffer dataReaderBuf = _cmdWrapper._dataReaderBuf;
			if (dataReaderBuf == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return dataReaderBuf;
		}
	}

	private OdbcConnection Connection
	{
		get
		{
			if (_cmdWrapper != null)
			{
				return _cmdWrapper.Connection;
			}
			return null;
		}
	}

	internal OdbcCommand Command
	{
		get
		{
			return _command;
		}
		set
		{
			_command = value;
		}
	}

	private OdbcStatementHandle StatementHandle => _cmdWrapper.StatementHandle;

	private OdbcStatementHandle KeyInfoStatementHandle => _cmdWrapper.KeyInfoStatement;

	internal bool IsCancelingCommand
	{
		get
		{
			if (_command != null)
			{
				return _command.Canceling;
			}
			return false;
		}
	}

	internal bool IsNonCancelingCommand
	{
		get
		{
			if (_command != null)
			{
				return !_command.Canceling;
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates the depth of nesting for the current row.</summary>
	/// <returns>The depth of nesting for the current row.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int Depth
	{
		get
		{
			if (IsClosed)
			{
				throw ADP.DataReaderClosed("Depth");
			}
			return 0;
		}
	}

	/// <summary>Gets the number of columns in the current row.</summary>
	/// <returns>When not positioned in a valid record set, 0; otherwise the number of columns in the current record. The default is -1.</returns>
	/// <exception cref="T:System.NotSupportedException">There is no current connection to a data source. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int FieldCount
	{
		get
		{
			if (IsClosed)
			{
				throw ADP.DataReaderClosed("FieldCount");
			}
			if (_noMoreResults)
			{
				return 0;
			}
			if (_dataCache == null)
			{
				short cColsAffected;
				ODBC32.RetCode retCode = FieldCountNoThrow(out cColsAffected);
				if (retCode != ODBC32.RetCode.SUCCESS)
				{
					Connection.HandleError(StatementHandle, retCode);
				}
			}
			if (_dataCache == null)
			{
				return 0;
			}
			return _dataCache._count;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Odbc.OdbcDataReader" /> contains one or more rows.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcDataReader" /> contains one or more rows; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool HasRows
	{
		get
		{
			if (IsClosed)
			{
				throw ADP.DataReaderClosed("HasRows");
			}
			if (_hasRows == HasRowsStatus.DontKnow)
			{
				Read();
				_skipReadOnce = true;
			}
			return _hasRows == HasRowsStatus.HasRows;
		}
	}

	/// <summary>Indicates whether the <see cref="T:System.Data.Odbc.OdbcDataReader" /> is closed.</summary>
	/// <returns>true if the <see cref="T:System.Data.Odbc.OdbcDataReader" /> is closed; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	public override bool IsClosed => _isClosed;

	/// <summary>Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.</summary>
	/// <returns>The number of rows changed, inserted, or deleted. -1 for SELECT statements; 0 if no rows were affected, or the statement failed.</returns>
	/// <filterpriority>1</filterpriority>
	public override int RecordsAffected => _recordAffected;

	/// <summary>Gets the value of the specified column in its native format given the column ordinal.</summary>
	/// <returns>The value of the specified column in its native format.</returns>
	/// <param name="i">The column ordinal. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[int i] => GetValue(i);

	/// <summary>Gets the value of the specified column in its native format given the column name.</summary>
	/// <returns>The value of the specified column in its native format.</returns>
	/// <param name="value">The column name. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object this[string value] => GetValue(GetOrdinal(value));

	internal OdbcDataReader(OdbcCommand command, CMDWrapper cmdWrapper, CommandBehavior commandbehavior)
	{
		_recordAffected = -1;
		_row = -1;
		_column = -1;
		ObjectID = Interlocked.Increment(ref s_objectTypeCount);
		base._002Ector();
		_command = command;
		_commandBehavior = commandbehavior;
		_cmdText = command.CommandText;
		_cmdWrapper = cmdWrapper;
	}

	internal bool IsBehavior(CommandBehavior behavior)
	{
		return IsCommandBehavior(behavior);
	}

	internal ODBC32.RetCode FieldCountNoThrow(out short cColsAffected)
	{
		if (IsCancelingCommand)
		{
			cColsAffected = 0;
			return ODBC32.RetCode.ERROR;
		}
		ODBC32.RetCode retCode = StatementHandle.NumberOfResultColumns(out cColsAffected);
		if (retCode == ODBC32.RetCode.SUCCESS)
		{
			_hiddenColumns = 0;
			if (IsCommandBehavior(CommandBehavior.KeyInfo) && !Connection.ProviderInfo.NoSqlSoptSSNoBrowseTable && !Connection.ProviderInfo.NoSqlSoptSSHiddenColumns)
			{
				for (int i = 0; i < cColsAffected; i++)
				{
					if (GetColAttribute(i, (ODBC32.SQL_DESC)1211, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.IGNORE).ToInt64() == 1)
					{
						_hiddenColumns = cColsAffected - i;
						cColsAffected = (short)i;
						break;
					}
				}
			}
			_dataCache = new DbCache(this, cColsAffected);
		}
		else
		{
			cColsAffected = 0;
		}
		return retCode;
	}

	private SQLLEN GetRowCount()
	{
		if (!IsClosed)
		{
			SQLLEN rowCount;
			ODBC32.RetCode retCode = StatementHandle.RowCount(out rowCount);
			if (retCode == ODBC32.RetCode.SUCCESS || ODBC32.RetCode.SUCCESS_WITH_INFO == retCode)
			{
				return rowCount;
			}
		}
		return -1;
	}

	internal int CalculateRecordsAffected(int cRowsAffected)
	{
		if (0 <= cRowsAffected)
		{
			if (-1 == _recordAffected)
			{
				_recordAffected = cRowsAffected;
			}
			else
			{
				_recordAffected += cRowsAffected;
			}
		}
		return _recordAffected;
	}

	/// <summary>Closes the <see cref="T:System.Data.Odbc.OdbcDataReader" /> object.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Close()
	{
		Close(disposing: false);
	}

	private void Close(bool disposing)
	{
		Exception ex = null;
		CMDWrapper cmdWrapper = _cmdWrapper;
		if (cmdWrapper != null && cmdWrapper.StatementHandle != null)
		{
			if (IsNonCancelingCommand)
			{
				NextResult(disposing, !disposing);
				if (_command != null)
				{
					if (_command.HasParameters)
					{
						_command.Parameters.GetOutputValues(_cmdWrapper);
					}
					cmdWrapper.FreeStatementHandle(ODBC32.STMT.CLOSE);
					_command.CloseFromDataReader();
				}
			}
			cmdWrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.CLOSE);
		}
		if (_command != null)
		{
			_command.CloseFromDataReader();
			if (IsCommandBehavior(CommandBehavior.CloseConnection))
			{
				_command.Parameters.RebindCollection = true;
				Connection.Close();
			}
		}
		else
		{
			cmdWrapper?.Dispose();
		}
		_command = null;
		_isClosed = true;
		_dataCache = null;
		_metadata = null;
		_schemaTable = null;
		_isRead = false;
		_hasRows = HasRowsStatus.DontKnow;
		_isValidResult = false;
		_noMoreResults = true;
		_noMoreRows = true;
		_fieldNameLookup = null;
		SetCurrentRowColumnInfo(-1, 0);
		if (ex != null && !disposing)
		{
			throw ex;
		}
		_cmdWrapper = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close(disposing: true);
		}
	}

	/// <summary>Gets the name of the source data type.</summary>
	/// <returns>The name of the source data type.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetDataTypeName(int i)
	{
		if (_dataCache != null)
		{
			DbSchemaInfo schema = _dataCache.GetSchema(i);
			if (schema._typename == null)
			{
				schema._typename = GetColAttributeStr(i, ODBC32.SQL_DESC.TYPE_NAME, ODBC32.SQL_COLUMN.TYPE_NAME, ODBC32.HANDLER.THROW);
			}
			return schema._typename;
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the rows in the data reader.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the rows in the data reader.</returns>
	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator((IDataReader)this, IsCommandBehavior(CommandBehavior.CloseConnection));
	}

	/// <summary>Gets the <see cref="T:System.Type" /> that is the data type of the object.</summary>
	/// <returns>The <see cref="T:System.Type" /> that is the data type of the object.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override Type GetFieldType(int i)
	{
		if (_dataCache != null)
		{
			DbSchemaInfo schema = _dataCache.GetSchema(i);
			if (schema._type == null)
			{
				schema._type = GetSqlType(i)._type;
			}
			return schema._type;
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the name of the specified column.</summary>
	/// <returns>A string that is the name of the specified column.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetName(int i)
	{
		if (_dataCache != null)
		{
			DbSchemaInfo schema = _dataCache.GetSchema(i);
			if (schema._name == null)
			{
				schema._name = GetColAttributeStr(i, ODBC32.SQL_DESC.NAME, ODBC32.SQL_COLUMN.NAME, ODBC32.HANDLER.THROW);
				if (schema._name == null)
				{
					schema._name = "";
				}
			}
			return schema._name;
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the column ordinal, given the name of the column.</summary>
	/// <returns>The zero-based column ordinal.</returns>
	/// <param name="value">The name of the column. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetOrdinal(string value)
	{
		if (_fieldNameLookup == null)
		{
			if (_dataCache == null)
			{
				throw ADP.DataReaderNoData();
			}
			_fieldNameLookup = new FieldNameLookup(this, -1);
		}
		return _fieldNameLookup.GetOrdinal(value);
	}

	private int IndexOf(string value)
	{
		if (_fieldNameLookup == null)
		{
			if (_dataCache == null)
			{
				throw ADP.DataReaderNoData();
			}
			_fieldNameLookup = new FieldNameLookup(this, -1);
		}
		return _fieldNameLookup.IndexOf(value);
	}

	private bool IsCommandBehavior(CommandBehavior condition)
	{
		return condition == (condition & _commandBehavior);
	}

	internal object GetValue(int i, TypeMap typemap)
	{
		switch (typemap._sql_type)
		{
		case ODBC32.SQL_TYPE.WLONGVARCHAR:
		case ODBC32.SQL_TYPE.WVARCHAR:
		case ODBC32.SQL_TYPE.WCHAR:
		case ODBC32.SQL_TYPE.LONGVARCHAR:
		case ODBC32.SQL_TYPE.CHAR:
		case ODBC32.SQL_TYPE.VARCHAR:
			return internalGetString(i);
		case ODBC32.SQL_TYPE.NUMERIC:
		case ODBC32.SQL_TYPE.DECIMAL:
			return internalGetDecimal(i);
		case ODBC32.SQL_TYPE.SMALLINT:
			return internalGetInt16(i);
		case ODBC32.SQL_TYPE.INTEGER:
			return internalGetInt32(i);
		case ODBC32.SQL_TYPE.REAL:
			return internalGetFloat(i);
		case ODBC32.SQL_TYPE.FLOAT:
		case ODBC32.SQL_TYPE.DOUBLE:
			return internalGetDouble(i);
		case ODBC32.SQL_TYPE.BIT:
			return internalGetBoolean(i);
		case ODBC32.SQL_TYPE.TINYINT:
			return internalGetByte(i);
		case ODBC32.SQL_TYPE.BIGINT:
			return internalGetInt64(i);
		case ODBC32.SQL_TYPE.LONGVARBINARY:
		case ODBC32.SQL_TYPE.VARBINARY:
		case ODBC32.SQL_TYPE.BINARY:
			return internalGetBytes(i);
		case ODBC32.SQL_TYPE.TYPE_DATE:
			return internalGetDate(i);
		case ODBC32.SQL_TYPE.TYPE_TIME:
			return internalGetTime(i);
		case ODBC32.SQL_TYPE.TYPE_TIMESTAMP:
			return internalGetDateTime(i);
		case ODBC32.SQL_TYPE.GUID:
			return internalGetGuid(i);
		case ODBC32.SQL_TYPE.SS_VARIANT:
			if (_isRead)
			{
				if (_dataCache.AccessIndex(i) == null && QueryFieldInfo(i, ODBC32.SQL_C.BINARY, out var _))
				{
					ODBC32.SQL_TYPE sqltype = (ODBC32.SQL_TYPE)(int)GetColAttribute(i, (ODBC32.SQL_DESC)1216, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.THROW);
					return GetValue(i, TypeMap.FromSqlType(sqltype));
				}
				return _dataCache[i];
			}
			throw ADP.DataReaderNoData();
		default:
			return internalGetBytes(i);
		}
	}

	/// <summary>Gets the value of the column at the specified ordinal in its native format.</summary>
	/// <returns>The value to return.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override object GetValue(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null)
			{
				_dataCache[i] = GetValue(i, GetSqlType(i));
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Populates an array of objects with the column values of the current row.</summary>
	/// <returns>The number of instances of <see cref="T:System.Object" /> in the array.</returns>
	/// <param name="values">An array of type <see cref="T:System.Object" /> into which to copy the attribute columns. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetValues(object[] values)
	{
		if (_isRead)
		{
			int num = Math.Min(values.Length, FieldCount);
			for (int i = 0; i < num; i++)
			{
				values[i] = GetValue(i);
			}
			return num;
		}
		throw ADP.DataReaderNoData();
	}

	private TypeMap GetSqlType(int i)
	{
		DbSchemaInfo schema = _dataCache.GetSchema(i);
		TypeMap typeMap;
		if (!schema._dbtype.HasValue)
		{
			schema._dbtype = (ODBC32.SQL_TYPE)(int)GetColAttribute(i, ODBC32.SQL_DESC.CONCISE_TYPE, ODBC32.SQL_COLUMN.TYPE, ODBC32.HANDLER.THROW);
			typeMap = TypeMap.FromSqlType(schema._dbtype.Value);
			if (typeMap._signType)
			{
				bool unsigned = GetColAttribute(i, ODBC32.SQL_DESC.UNSIGNED, ODBC32.SQL_COLUMN.UNSIGNED, ODBC32.HANDLER.THROW).ToInt64() != 0;
				typeMap = TypeMap.UpgradeSignedType(typeMap, unsigned);
				schema._dbtype = typeMap._sql_type;
			}
		}
		else
		{
			typeMap = TypeMap.FromSqlType(schema._dbtype.Value);
		}
		Connection.SetSupportedType(schema._dbtype.Value);
		return typeMap;
	}

	/// <summary>Gets a value that indicates whether the column contains nonexistent or missing values.</summary>
	/// <returns>true if the specified column value is equivalent to <see cref="T:System.DBNull" />; otherwise false.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool IsDBNull(int i)
	{
		if (!IsCommandBehavior(CommandBehavior.SequentialAccess))
		{
			return Convert.IsDBNull(GetValue(i));
		}
		object obj = _dataCache[i];
		if (obj != null)
		{
			return Convert.IsDBNull(obj);
		}
		TypeMap sqlType = GetSqlType(i);
		if (sqlType._bufferSize > 0)
		{
			return Convert.IsDBNull(GetValue(i));
		}
		int cbLengthOrIndicator;
		return !QueryFieldInfo(i, sqlType._sql_c, out cbLengthOrIndicator);
	}

	/// <summary>Gets the value of the specified column as a byte.</summary>
	/// <returns>The value of the specified column as a byte.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override byte GetByte(int i)
	{
		return (byte)internalGetByte(i);
	}

	private object internalGetByte(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.UTINYINT))
			{
				_dataCache[i] = Buffer.ReadByte(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a character.</summary>
	/// <returns>The value of the specified column as a character.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override char GetChar(int i)
	{
		return (char)internalGetChar(i);
	}

	private object internalGetChar(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.WCHAR))
			{
				_dataCache[i] = Buffer.ReadChar(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a 16-bit signed integer.</summary>
	/// <returns>The value of the specified column as a 16-bit signed integer.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override short GetInt16(int i)
	{
		return (short)internalGetInt16(i);
	}

	private object internalGetInt16(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.SSHORT))
			{
				_dataCache[i] = Buffer.ReadInt16(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a 32-bit signed integer.</summary>
	/// <returns>The value of the specified column as a 32-bit signed integer.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override int GetInt32(int i)
	{
		return (int)internalGetInt32(i);
	}

	private object internalGetInt32(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.SLONG))
			{
				_dataCache[i] = Buffer.ReadInt32(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a 64-bit signed integer.</summary>
	/// <returns>The value of the specified column as a 64-bit signed integer.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetInt64(int i)
	{
		return (long)internalGetInt64(i);
	}

	private object internalGetInt64(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.WCHAR))
			{
				string s = (string)Buffer.MarshalToManaged(0, ODBC32.SQL_C.WCHAR, -3);
				_dataCache[i] = long.Parse(s, CultureInfo.InvariantCulture);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a Boolean.</summary>
	/// <returns>A Boolean that is the value of the column.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override bool GetBoolean(int i)
	{
		return (bool)internalGetBoolean(i);
	}

	private object internalGetBoolean(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.BIT))
			{
				_dataCache[i] = Buffer.MarshalToManaged(0, ODBC32.SQL_C.BIT, -1);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a single-precision floating-point number.</summary>
	/// <returns>The value of the specified column as a single-precision floating-point number.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override float GetFloat(int i)
	{
		return (float)internalGetFloat(i);
	}

	private object internalGetFloat(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.REAL))
			{
				_dataCache[i] = Buffer.ReadSingle(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public DateTime GetDate(int i)
	{
		return (DateTime)internalGetDate(i);
	}

	private object internalGetDate(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.TYPE_DATE))
			{
				_dataCache[i] = Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_DATE, -1);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DateTime GetDateTime(int i)
	{
		return (DateTime)internalGetDateTime(i);
	}

	private object internalGetDateTime(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.TYPE_TIMESTAMP))
			{
				_dataCache[i] = Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_TIMESTAMP, -1);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.Decimal" /> object.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override decimal GetDecimal(int i)
	{
		return (decimal)internalGetDecimal(i);
	}

	private object internalGetDecimal(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.WCHAR))
			{
				string text = null;
				try
				{
					text = (string)Buffer.MarshalToManaged(0, ODBC32.SQL_C.WCHAR, -3);
					_dataCache[i] = decimal.Parse(text, CultureInfo.InvariantCulture);
				}
				catch (OverflowException ex)
				{
					_dataCache[i] = text;
					throw ex;
				}
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a double-precision floating-point number.</summary>
	/// <returns>The value of the specified column as a double-precision floating-point number.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override double GetDouble(int i)
	{
		return (double)internalGetDouble(i);
	}

	private object internalGetDouble(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.DOUBLE))
			{
				_dataCache[i] = Buffer.ReadDouble(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a globally unique identifier (GUID).</summary>
	/// <returns>The value of the specified column as a GUID.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override Guid GetGuid(int i)
	{
		return (Guid)internalGetGuid(i);
	}

	private object internalGetGuid(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.GUID))
			{
				_dataCache[i] = Buffer.ReadGuid(0);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.String" />.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.String" />.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override string GetString(int i)
	{
		return (string)internalGetString(i);
	}

	private object internalGetString(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null)
			{
				CNativeBuffer buffer = Buffer;
				int num = buffer.Length - 4;
				if (GetData(i, ODBC32.SQL_C.WCHAR, buffer.Length - 2, out var cbLengthOrIndicator))
				{
					if (cbLengthOrIndicator <= num && -4 != cbLengthOrIndicator)
					{
						string text = buffer.PtrToStringUni(0, Math.Min(cbLengthOrIndicator, num) / 2);
						_dataCache[i] = text;
						return text;
					}
					char[] array = new char[num / 2];
					StringBuilder stringBuilder = new StringBuilder(((cbLengthOrIndicator == -4) ? num : cbLengthOrIndicator) / 2);
					int num2 = num;
					int num3 = ((-4 == cbLengthOrIndicator) ? (-1) : (cbLengthOrIndicator - num2));
					bool data;
					do
					{
						int num4 = num2 / 2;
						buffer.ReadChars(0, array, 0, num4);
						stringBuilder.Append(array, 0, num4);
						if (num3 == 0)
						{
							break;
						}
						data = GetData(i, ODBC32.SQL_C.WCHAR, buffer.Length - 2, out cbLengthOrIndicator);
						if (-4 != cbLengthOrIndicator)
						{
							num2 = Math.Min(cbLengthOrIndicator, num);
							num3 = ((0 < num3) ? (num3 - num2) : 0);
						}
					}
					while (data);
					_dataCache[i] = stringBuilder.ToString();
				}
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.TimeSpan" /> object.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.TimeSpan" /> object.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public TimeSpan GetTime(int i)
	{
		return (TimeSpan)internalGetTime(i);
	}

	private object internalGetTime(int i)
	{
		if (_isRead)
		{
			if (_dataCache.AccessIndex(i) == null && GetData(i, ODBC32.SQL_C.TYPE_TIME))
			{
				_dataCache[i] = Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_TIME, -1);
			}
			return _dataCache[i];
		}
		throw ADP.DataReaderNoData();
	}

	private void SetCurrentRowColumnInfo(int row, int column)
	{
		if (_row != row || _column != column)
		{
			_row = row;
			_column = column;
			_sequentialBytesRead = 0L;
		}
	}

	/// <summary>Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the particular buffer offset.</summary>
	/// <returns>The actual number of bytes read.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <param name="dataIndex">The index within the field where the read operation is to start. </param>
	/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
	/// <param name="bufferIndex">The index within the <paramref name="buffer" /> where the write operation is to start. </param>
	/// <param name="length">The number of bytes to read. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
	{
		return GetBytesOrChars(i, dataIndex, buffer, isCharsBuffer: false, bufferIndex, length);
	}

	/// <summary>Reads a stream of characters from the specified column offset into the buffer as an array, starting at the particular buffer offset.</summary>
	/// <returns>The actual number of characters read.</returns>
	/// <param name="i">The zero-based column ordinal. </param>
	/// <param name="dataIndex">The index within the row where the read operation is to start. </param>
	/// <param name="buffer">The buffer into which to copy data. </param>
	/// <param name="bufferIndex">The index within the <paramref name="buffer" /> where the write operation is to start. </param>
	/// <param name="length">The number of characters to read. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
	{
		return GetBytesOrChars(i, dataIndex, buffer, isCharsBuffer: true, bufferIndex, length);
	}

	private long GetBytesOrChars(int i, long dataIndex, Array buffer, bool isCharsBuffer, int bufferIndex, int length)
	{
		if (IsClosed)
		{
			throw ADP.DataReaderNoData();
		}
		if (!_isRead)
		{
			throw ADP.DataReaderNoData();
		}
		if (dataIndex < 0)
		{
			throw ADP.ArgumentOutOfRange("dataIndex");
		}
		if (bufferIndex < 0)
		{
			throw ADP.ArgumentOutOfRange("bufferIndex");
		}
		if (length < 0)
		{
			throw ADP.ArgumentOutOfRange("length");
		}
		string method = (isCharsBuffer ? "GetChars" : "GetBytes");
		SetCurrentRowColumnInfo(_row, i);
		object obj = null;
		obj = ((!isCharsBuffer) ? ((object)(byte[])_dataCache[i]) : ((object)(string)_dataCache[i]));
		if (!IsCommandBehavior(CommandBehavior.SequentialAccess) || obj != null)
		{
			if (int.MaxValue < dataIndex)
			{
				throw ADP.ArgumentOutOfRange("dataIndex");
			}
			if (obj == null)
			{
				obj = ((!isCharsBuffer) ? ((object)(byte[])internalGetBytes(i)) : ((object)(string)internalGetString(i)));
			}
			int num = (isCharsBuffer ? ((string)obj).Length : ((byte[])obj).Length);
			if (buffer == null)
			{
				return num;
			}
			if (length == 0)
			{
				return 0L;
			}
			if (dataIndex >= num)
			{
				return 0L;
			}
			int val = Math.Min(num - (int)dataIndex, length);
			val = Math.Min(val, buffer.Length - bufferIndex);
			if (val <= 0)
			{
				return 0L;
			}
			if (isCharsBuffer)
			{
				((string)obj).CopyTo((int)dataIndex, (char[])buffer, bufferIndex, val);
			}
			else
			{
				Array.Copy((byte[])obj, (int)dataIndex, (byte[])buffer, bufferIndex, val);
			}
			return val;
		}
		if (buffer == null)
		{
			ODBC32.SQL_C sqlctype = (isCharsBuffer ? ODBC32.SQL_C.WCHAR : ODBC32.SQL_C.BINARY);
			if (!QueryFieldInfo(i, sqlctype, out var cbLengthOrIndicator))
			{
				if (isCharsBuffer)
				{
					throw ADP.InvalidCast();
				}
				return -1L;
			}
			if (isCharsBuffer)
			{
				return cbLengthOrIndicator / 2;
			}
			return cbLengthOrIndicator;
		}
		if ((isCharsBuffer && dataIndex < _sequentialBytesRead / 2) || (!isCharsBuffer && dataIndex < _sequentialBytesRead))
		{
			throw ADP.NonSeqByteAccess(dataIndex, _sequentialBytesRead, method);
		}
		dataIndex = ((!isCharsBuffer) ? (dataIndex - _sequentialBytesRead) : (dataIndex - _sequentialBytesRead / 2));
		if (dataIndex > 0 && readBytesOrCharsSequentialAccess(i, null, isCharsBuffer, 0, dataIndex) < dataIndex)
		{
			return 0L;
		}
		length = Math.Min(length, buffer.Length - bufferIndex);
		if (length <= 0)
		{
			if (isCharsBuffer && !QueryFieldInfo(i, ODBC32.SQL_C.WCHAR, out var _))
			{
				throw ADP.InvalidCast();
			}
			return 0L;
		}
		return readBytesOrCharsSequentialAccess(i, buffer, isCharsBuffer, bufferIndex, length);
	}

	private int readBytesOrCharsSequentialAccess(int i, Array buffer, bool isCharsBuffer, int bufferIndex, long bytesOrCharsLength)
	{
		int num = 0;
		long num2 = (isCharsBuffer ? checked(bytesOrCharsLength * 2) : bytesOrCharsLength);
		CNativeBuffer buffer2 = Buffer;
		while (num2 > 0)
		{
			int num3;
			bool data;
			int cbLengthOrIndicator;
			if (isCharsBuffer)
			{
				num3 = (int)Math.Min(num2, buffer2.Length - 4);
				data = GetData(i, ODBC32.SQL_C.WCHAR, num3 + 2, out cbLengthOrIndicator);
			}
			else
			{
				num3 = (int)Math.Min(num2, buffer2.Length - 2);
				data = GetData(i, ODBC32.SQL_C.BINARY, num3, out cbLengthOrIndicator);
			}
			if (!data)
			{
				throw ADP.InvalidCast();
			}
			bool flag = false;
			if (cbLengthOrIndicator == 0)
			{
				break;
			}
			int num4;
			if (-4 == cbLengthOrIndicator)
			{
				num4 = num3;
			}
			else if (cbLengthOrIndicator > num3)
			{
				num4 = num3;
			}
			else
			{
				num4 = cbLengthOrIndicator;
				flag = true;
			}
			_sequentialBytesRead += num4;
			if (isCharsBuffer)
			{
				int num5 = num4 / 2;
				if (buffer != null)
				{
					buffer2.ReadChars(0, (char[])buffer, bufferIndex, num5);
					bufferIndex += num5;
				}
				num += num5;
			}
			else
			{
				if (buffer != null)
				{
					buffer2.ReadBytes(0, (byte[])buffer, bufferIndex, num4);
					bufferIndex += num4;
				}
				num += num4;
			}
			num2 -= num4;
			if (flag)
			{
				break;
			}
		}
		return num;
	}

	private object internalGetBytes(int i)
	{
		if (_dataCache.AccessIndex(i) == null)
		{
			int num = Buffer.Length - 4;
			int num2 = 0;
			if (GetData(i, ODBC32.SQL_C.BINARY, num, out var cbLengthOrIndicator))
			{
				CNativeBuffer buffer = Buffer;
				byte[] array;
				if (-4 != cbLengthOrIndicator)
				{
					array = new byte[cbLengthOrIndicator];
					Buffer.ReadBytes(0, array, num2, Math.Min(cbLengthOrIndicator, num));
					while (cbLengthOrIndicator > num)
					{
						GetData(i, ODBC32.SQL_C.BINARY, num, out cbLengthOrIndicator);
						num2 += num;
						buffer.ReadBytes(0, array, num2, Math.Min(cbLengthOrIndicator, num));
					}
				}
				else
				{
					List<byte[]> list = new List<byte[]>();
					int num3 = 0;
					do
					{
						int num4 = ((-4 != cbLengthOrIndicator) ? cbLengthOrIndicator : num);
						array = new byte[num4];
						num3 += num4;
						buffer.ReadBytes(0, array, 0, num4);
						list.Add(array);
					}
					while (-4 == cbLengthOrIndicator && GetData(i, ODBC32.SQL_C.BINARY, num, out cbLengthOrIndicator));
					array = new byte[num3];
					foreach (byte[] item in list)
					{
						item.CopyTo(array, num2);
						num2 += item.Length;
					}
				}
				_dataCache[i] = array;
			}
		}
		return _dataCache[i];
	}

	private SQLLEN GetColAttribute(int iColumn, ODBC32.SQL_DESC v3FieldId, ODBC32.SQL_COLUMN v2FieldId, ODBC32.HANDLER handler)
	{
		short stringLength = 0;
		if (Connection == null || _cmdWrapper.Canceling)
		{
			return -1;
		}
		OdbcStatementHandle statementHandle = StatementHandle;
		ODBC32.RetCode retCode;
		SQLLEN numericAttribute;
		if (Connection.IsV3Driver)
		{
			retCode = statementHandle.ColumnAttribute(iColumn + 1, (short)v3FieldId, Buffer, out stringLength, out numericAttribute);
		}
		else
		{
			if (v2FieldId == (ODBC32.SQL_COLUMN)(-1))
			{
				return 0;
			}
			retCode = statementHandle.ColumnAttribute(iColumn + 1, (short)v2FieldId, Buffer, out stringLength, out numericAttribute);
		}
		if (retCode != ODBC32.RetCode.SUCCESS)
		{
			if (retCode == ODBC32.RetCode.ERROR && "HY091" == Command.GetDiagSqlState())
			{
				Connection.FlagUnsupportedColAttr(v3FieldId, v2FieldId);
			}
			if (handler == ODBC32.HANDLER.THROW)
			{
				Connection.HandleError(statementHandle, retCode);
			}
			return -1;
		}
		return numericAttribute;
	}

	private string GetColAttributeStr(int i, ODBC32.SQL_DESC v3FieldId, ODBC32.SQL_COLUMN v2FieldId, ODBC32.HANDLER handler)
	{
		short stringLength = 0;
		CNativeBuffer buffer = Buffer;
		buffer.WriteInt16(0, 0);
		OdbcStatementHandle statementHandle = StatementHandle;
		if (Connection == null || _cmdWrapper.Canceling || statementHandle == null)
		{
			return "";
		}
		ODBC32.RetCode retCode;
		SQLLEN numericAttribute;
		if (Connection.IsV3Driver)
		{
			retCode = statementHandle.ColumnAttribute(i + 1, (short)v3FieldId, buffer, out stringLength, out numericAttribute);
		}
		else
		{
			if (v2FieldId == (ODBC32.SQL_COLUMN)(-1))
			{
				return null;
			}
			retCode = statementHandle.ColumnAttribute(i + 1, (short)v2FieldId, buffer, out stringLength, out numericAttribute);
		}
		if (retCode != ODBC32.RetCode.SUCCESS || stringLength == 0)
		{
			if (retCode == ODBC32.RetCode.ERROR && "HY091" == Command.GetDiagSqlState())
			{
				Connection.FlagUnsupportedColAttr(v3FieldId, v2FieldId);
			}
			if (handler == ODBC32.HANDLER.THROW)
			{
				Connection.HandleError(statementHandle, retCode);
			}
			return null;
		}
		return buffer.PtrToStringUni(0, stringLength / 2);
	}

	private string GetDescFieldStr(int i, ODBC32.SQL_DESC attribute, ODBC32.HANDLER handler)
	{
		int numericAttribute = 0;
		if (Connection == null || _cmdWrapper.Canceling)
		{
			return "";
		}
		if (!Connection.IsV3Driver)
		{
			return null;
		}
		CNativeBuffer buffer = Buffer;
		using (OdbcDescriptorHandle odbcDescriptorHandle = new OdbcDescriptorHandle(StatementHandle, ODBC32.SQL_ATTR.APP_PARAM_DESC))
		{
			ODBC32.RetCode descriptionField = odbcDescriptorHandle.GetDescriptionField(i + 1, attribute, buffer, out numericAttribute);
			if (descriptionField != ODBC32.RetCode.SUCCESS || numericAttribute == 0)
			{
				if (descriptionField == ODBC32.RetCode.ERROR && "HY091" == Command.GetDiagSqlState())
				{
					Connection.FlagUnsupportedColAttr(attribute, ODBC32.SQL_COLUMN.COUNT);
				}
				if (handler == ODBC32.HANDLER.THROW)
				{
					Connection.HandleError(StatementHandle, descriptionField);
				}
				return null;
			}
		}
		return buffer.PtrToStringUni(0, numericAttribute / 2);
	}

	private bool QueryFieldInfo(int i, ODBC32.SQL_C sqlctype, out int cbLengthOrIndicator)
	{
		int cb = 0;
		if (sqlctype == ODBC32.SQL_C.WCHAR)
		{
			cb = 2;
		}
		return GetData(i, sqlctype, cb, out cbLengthOrIndicator);
	}

	private bool GetData(int i, ODBC32.SQL_C sqlctype)
	{
		int cbLengthOrIndicator;
		return GetData(i, sqlctype, Buffer.Length - 4, out cbLengthOrIndicator);
	}

	private bool GetData(int i, ODBC32.SQL_C sqlctype, int cb, out int cbLengthOrIndicator)
	{
		IntPtr cbActual = IntPtr.Zero;
		if (IsCancelingCommand)
		{
			throw ADP.DataReaderNoData();
		}
		CNativeBuffer buffer = Buffer;
		ODBC32.RetCode data = StatementHandle.GetData(i + 1, sqlctype, buffer, cb, out cbActual);
		switch (data)
		{
		case ODBC32.RetCode.SUCCESS_WITH_INFO:
			if ((int)cbActual != -4)
			{
			}
			break;
		case ODBC32.RetCode.NO_DATA:
			if (sqlctype != ODBC32.SQL_C.WCHAR && sqlctype != ODBC32.SQL_C.BINARY)
			{
				Connection.HandleError(StatementHandle, data);
			}
			if (cbActual == (IntPtr)(-4))
			{
				cbActual = (IntPtr)0;
			}
			break;
		default:
			Connection.HandleError(StatementHandle, data);
			break;
		case ODBC32.RetCode.SUCCESS:
			break;
		}
		SetCurrentRowColumnInfo(_row, i);
		if (cbActual == (IntPtr)(-1))
		{
			_dataCache[i] = DBNull.Value;
			cbLengthOrIndicator = 0;
			return false;
		}
		cbLengthOrIndicator = (int)cbActual;
		return true;
	}

	/// <summary>Advances the <see cref="T:System.Data.Odbc.OdbcDataReader" /> to the next record.</summary>
	/// <returns>true if there are more rows; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool Read()
	{
		if (IsClosed)
		{
			throw ADP.DataReaderClosed("Read");
		}
		if (IsCancelingCommand)
		{
			_isRead = false;
			return false;
		}
		if (_skipReadOnce)
		{
			_skipReadOnce = false;
			return _isRead;
		}
		if (_noMoreRows || _noMoreResults || IsCommandBehavior(CommandBehavior.SchemaOnly))
		{
			return false;
		}
		if (!_isValidResult)
		{
			return false;
		}
		ODBC32.RetCode retCode = StatementHandle.Fetch();
		switch (retCode)
		{
		case ODBC32.RetCode.SUCCESS_WITH_INFO:
			Connection.HandleErrorNoThrow(StatementHandle, retCode);
			_hasRows = HasRowsStatus.HasRows;
			_isRead = true;
			break;
		case ODBC32.RetCode.SUCCESS:
			_hasRows = HasRowsStatus.HasRows;
			_isRead = true;
			break;
		case ODBC32.RetCode.NO_DATA:
			_isRead = false;
			if (_hasRows == HasRowsStatus.DontKnow)
			{
				_hasRows = HasRowsStatus.HasNoRows;
			}
			break;
		default:
			Connection.HandleError(StatementHandle, retCode);
			break;
		}
		_dataCache.FlushValues();
		if (IsCommandBehavior(CommandBehavior.SingleRow))
		{
			_noMoreRows = true;
			SetCurrentRowColumnInfo(-1, 0);
		}
		else
		{
			SetCurrentRowColumnInfo(_row + 1, 0);
		}
		return _isRead;
	}

	internal void FirstResult()
	{
		SQLLEN rowCount = GetRowCount();
		CalculateRecordsAffected(rowCount);
		if (FieldCountNoThrow(out var cColsAffected) == ODBC32.RetCode.SUCCESS && cColsAffected == 0)
		{
			NextResult();
		}
		else
		{
			_isValidResult = true;
		}
	}

	/// <summary>Advances the <see cref="T:System.Data.Odbc.OdbcDataReader" /> to the next result when reading the results of batch SQL statements.</summary>
	/// <returns>true if there are more result sets; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool NextResult()
	{
		return NextResult(disposing: false, allresults: false);
	}

	private bool NextResult(bool disposing, bool allresults)
	{
		ODBC32.RetCode retcode = ODBC32.RetCode.SUCCESS;
		bool flag = false;
		bool flag2 = IsCommandBehavior(CommandBehavior.SingleResult);
		if (IsClosed)
		{
			throw ADP.DataReaderClosed("NextResult");
		}
		_fieldNameLookup = null;
		if (IsCancelingCommand || _noMoreResults)
		{
			return false;
		}
		_isRead = false;
		_hasRows = HasRowsStatus.DontKnow;
		_fieldNameLookup = null;
		_metadata = null;
		_schemaTable = null;
		int num = 0;
		OdbcErrorCollection odbcErrorCollection = null;
		ODBC32.RetCode retCode;
		bool flag3;
		do
		{
			_isValidResult = false;
			retCode = StatementHandle.MoreResults();
			flag3 = retCode == ODBC32.RetCode.SUCCESS || retCode == ODBC32.RetCode.SUCCESS_WITH_INFO;
			if (retCode == ODBC32.RetCode.SUCCESS_WITH_INFO)
			{
				Connection.HandleErrorNoThrow(StatementHandle, retCode);
			}
			else if (!disposing && retCode != ODBC32.RetCode.NO_DATA && retCode != ODBC32.RetCode.SUCCESS)
			{
				if (odbcErrorCollection == null)
				{
					retcode = retCode;
					odbcErrorCollection = new OdbcErrorCollection();
				}
				ODBC32.GetDiagErrors(odbcErrorCollection, null, StatementHandle, retCode);
				num++;
			}
			if (!disposing && flag3)
			{
				num = 0;
				SQLLEN rowCount = GetRowCount();
				CalculateRecordsAffected(rowCount);
				if (!flag2)
				{
					FieldCountNoThrow(out var cColsAffected);
					flag = (_isValidResult = cColsAffected != 0);
				}
			}
		}
		while ((!flag2 && flag3 && !flag) || (ODBC32.RetCode.NO_DATA != retCode && allresults && num < 2000) || (flag2 && flag3));
		if (retCode == ODBC32.RetCode.NO_DATA)
		{
			_dataCache = null;
			_noMoreResults = true;
		}
		if (odbcErrorCollection != null)
		{
			odbcErrorCollection.SetSource(Connection.Driver);
			OdbcException ex = OdbcException.CreateException(odbcErrorCollection, retcode);
			Connection.ConnectionIsAlive(ex);
			throw ex;
		}
		return flag3;
	}

	private void BuildMetaDataInfo()
	{
		int fieldCount = FieldCount;
		MetaData[] array = new MetaData[fieldCount];
		bool flag = IsCommandBehavior(CommandBehavior.KeyInfo);
		List<string> list = ((!flag) ? null : new List<string>());
		for (int i = 0; i < fieldCount; i++)
		{
			array[i] = new MetaData();
			array[i].ordinal = i;
			TypeMap typeMap = TypeMap.FromSqlType((ODBC32.SQL_TYPE)(int)GetColAttribute(i, ODBC32.SQL_DESC.CONCISE_TYPE, ODBC32.SQL_COLUMN.TYPE, ODBC32.HANDLER.THROW));
			if (typeMap._signType)
			{
				bool unsigned = GetColAttribute(i, ODBC32.SQL_DESC.UNSIGNED, ODBC32.SQL_COLUMN.UNSIGNED, ODBC32.HANDLER.THROW).ToInt64() != 0;
				typeMap = TypeMap.UpgradeSignedType(typeMap, unsigned);
			}
			array[i].typemap = typeMap;
			array[i].size = GetColAttribute(i, ODBC32.SQL_DESC.OCTET_LENGTH, ODBC32.SQL_COLUMN.LENGTH, ODBC32.HANDLER.IGNORE);
			ODBC32.SQL_TYPE sql_type = array[i].typemap._sql_type;
			if ((uint)(sql_type - -10) <= 2u)
			{
				MetaData obj = array[i];
				obj.size = (int)obj.size / 2;
			}
			array[i].precision = (byte)(int)GetColAttribute(i, (ODBC32.SQL_DESC)4, ODBC32.SQL_COLUMN.PRECISION, ODBC32.HANDLER.IGNORE);
			array[i].scale = (byte)(int)GetColAttribute(i, (ODBC32.SQL_DESC)5, ODBC32.SQL_COLUMN.SCALE, ODBC32.HANDLER.IGNORE);
			array[i].isAutoIncrement = (int)GetColAttribute(i, ODBC32.SQL_DESC.AUTO_UNIQUE_VALUE, ODBC32.SQL_COLUMN.AUTO_INCREMENT, ODBC32.HANDLER.IGNORE) == 1;
			array[i].isReadOnly = (int)GetColAttribute(i, ODBC32.SQL_DESC.UPDATABLE, ODBC32.SQL_COLUMN.UPDATABLE, ODBC32.HANDLER.IGNORE) == 0;
			ODBC32.SQL_NULLABILITY sQL_NULLABILITY = (ODBC32.SQL_NULLABILITY)(int)GetColAttribute(i, ODBC32.SQL_DESC.NULLABLE, ODBC32.SQL_COLUMN.NULLABLE, ODBC32.HANDLER.IGNORE);
			array[i].isNullable = sQL_NULLABILITY == ODBC32.SQL_NULLABILITY.NULLABLE;
			sql_type = array[i].typemap._sql_type;
			if (sql_type == ODBC32.SQL_TYPE.WLONGVARCHAR || sql_type == ODBC32.SQL_TYPE.LONGVARBINARY || sql_type == ODBC32.SQL_TYPE.LONGVARCHAR)
			{
				array[i].isLong = true;
			}
			else
			{
				array[i].isLong = false;
			}
			if (IsCommandBehavior(CommandBehavior.KeyInfo))
			{
				if (!Connection.ProviderInfo.NoSqlCASSColumnKey)
				{
					bool flag2 = (int)GetColAttribute(i, (ODBC32.SQL_DESC)1212, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.IGNORE) == 1;
					if (flag2)
					{
						array[i].isKeyColumn = flag2;
						array[i].isUnique = true;
						flag = false;
					}
				}
				array[i].baseSchemaName = GetColAttributeStr(i, ODBC32.SQL_DESC.SCHEMA_NAME, ODBC32.SQL_COLUMN.OWNER_NAME, ODBC32.HANDLER.IGNORE);
				array[i].baseCatalogName = GetColAttributeStr(i, ODBC32.SQL_DESC.CATALOG_NAME, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.IGNORE);
				array[i].baseTableName = GetColAttributeStr(i, ODBC32.SQL_DESC.BASE_TABLE_NAME, ODBC32.SQL_COLUMN.TABLE_NAME, ODBC32.HANDLER.IGNORE);
				array[i].baseColumnName = GetColAttributeStr(i, ODBC32.SQL_DESC.BASE_COLUMN_NAME, ODBC32.SQL_COLUMN.NAME, ODBC32.HANDLER.IGNORE);
				if (Connection.IsV3Driver)
				{
					if (array[i].baseTableName == null || array[i].baseTableName.Length == 0)
					{
						array[i].baseTableName = GetDescFieldStr(i, ODBC32.SQL_DESC.BASE_TABLE_NAME, ODBC32.HANDLER.IGNORE);
					}
					if (array[i].baseColumnName == null || array[i].baseColumnName.Length == 0)
					{
						array[i].baseColumnName = GetDescFieldStr(i, ODBC32.SQL_DESC.BASE_COLUMN_NAME, ODBC32.HANDLER.IGNORE);
					}
				}
				if (array[i].baseTableName != null && !list.Contains(array[i].baseTableName))
				{
					list.Add(array[i].baseTableName);
				}
			}
			if ((array[i].isKeyColumn || array[i].isAutoIncrement) && sQL_NULLABILITY == ODBC32.SQL_NULLABILITY.UNKNOWN)
			{
				array[i].isNullable = false;
			}
		}
		if (!Connection.ProviderInfo.NoSqlCASSColumnKey)
		{
			for (int j = fieldCount; j < fieldCount + _hiddenColumns; j++)
			{
				if ((int)GetColAttribute(j, (ODBC32.SQL_DESC)1212, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.IGNORE) == 1 && (int)GetColAttribute(j, (ODBC32.SQL_DESC)1211, (ODBC32.SQL_COLUMN)(-1), ODBC32.HANDLER.IGNORE) == 1)
				{
					for (int k = 0; k < fieldCount; k++)
					{
						array[k].isKeyColumn = false;
						array[k].isUnique = false;
					}
				}
			}
		}
		_metadata = array;
		if (!IsCommandBehavior(CommandBehavior.KeyInfo))
		{
			return;
		}
		if (list != null && list.Count > 0)
		{
			List<string>.Enumerator enumerator = list.GetEnumerator();
			QualifiedTableName qualifiedTableName = new QualifiedTableName(Connection.QuoteChar("GetSchemaTable"));
			while (enumerator.MoveNext())
			{
				qualifiedTableName.Table = enumerator.Current;
				if (RetrieveKeyInfo(flag, qualifiedTableName, quoted: false) <= 0)
				{
					RetrieveKeyInfo(flag, qualifiedTableName, quoted: true);
				}
			}
			return;
		}
		QualifiedTableName qualifiedTableName2 = new QualifiedTableName(Connection.QuoteChar("GetSchemaTable"), GetTableNameFromCommandText());
		if (!string.IsNullOrEmpty(qualifiedTableName2.Table))
		{
			SetBaseTableNames(qualifiedTableName2);
			if (RetrieveKeyInfo(flag, qualifiedTableName2, quoted: false) <= 0)
			{
				RetrieveKeyInfo(flag, qualifiedTableName2, quoted: true);
			}
		}
	}

	private DataTable NewSchemaTable()
	{
		DataTable dataTable = new DataTable("SchemaTable");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.MinimumCapacity = FieldCount;
		DataColumnCollection columns = dataTable.Columns;
		columns.Add(new DataColumn("ColumnName", typeof(string)));
		columns.Add(new DataColumn("ColumnOrdinal", typeof(int)));
		columns.Add(new DataColumn("ColumnSize", typeof(int)));
		columns.Add(new DataColumn("NumericPrecision", typeof(short)));
		columns.Add(new DataColumn("NumericScale", typeof(short)));
		columns.Add(new DataColumn("DataType", typeof(object)));
		columns.Add(new DataColumn("ProviderType", typeof(int)));
		columns.Add(new DataColumn("IsLong", typeof(bool)));
		columns.Add(new DataColumn("AllowDBNull", typeof(bool)));
		columns.Add(new DataColumn("IsReadOnly", typeof(bool)));
		columns.Add(new DataColumn("IsRowVersion", typeof(bool)));
		columns.Add(new DataColumn("IsUnique", typeof(bool)));
		columns.Add(new DataColumn("IsKey", typeof(bool)));
		columns.Add(new DataColumn("IsAutoIncrement", typeof(bool)));
		columns.Add(new DataColumn("BaseSchemaName", typeof(string)));
		columns.Add(new DataColumn("BaseCatalogName", typeof(string)));
		columns.Add(new DataColumn("BaseTableName", typeof(string)));
		columns.Add(new DataColumn("BaseColumnName", typeof(string)));
		foreach (DataColumn item in columns)
		{
			item.ReadOnly = true;
		}
		return dataTable;
	}

	/// <summary>Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.Odbc.OdbcDataReader" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that describes the column metadata.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.Odbc.OdbcDataReader" /> is closed. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override DataTable GetSchemaTable()
	{
		if (IsClosed)
		{
			throw ADP.DataReaderClosed("GetSchemaTable");
		}
		if (_noMoreResults)
		{
			return null;
		}
		if (_schemaTable != null)
		{
			return _schemaTable;
		}
		DataTable dataTable = NewSchemaTable();
		if (FieldCount == 0)
		{
			return dataTable;
		}
		if (_metadata == null)
		{
			BuildMetaDataInfo();
		}
		DataColumn column = dataTable.Columns["ColumnName"];
		DataColumn column2 = dataTable.Columns["ColumnOrdinal"];
		DataColumn column3 = dataTable.Columns["ColumnSize"];
		DataColumn column4 = dataTable.Columns["NumericPrecision"];
		DataColumn column5 = dataTable.Columns["NumericScale"];
		DataColumn column6 = dataTable.Columns["DataType"];
		DataColumn column7 = dataTable.Columns["ProviderType"];
		DataColumn column8 = dataTable.Columns["IsLong"];
		DataColumn column9 = dataTable.Columns["AllowDBNull"];
		DataColumn column10 = dataTable.Columns["IsReadOnly"];
		DataColumn column11 = dataTable.Columns["IsRowVersion"];
		DataColumn column12 = dataTable.Columns["IsUnique"];
		DataColumn column13 = dataTable.Columns["IsKey"];
		DataColumn column14 = dataTable.Columns["IsAutoIncrement"];
		DataColumn column15 = dataTable.Columns["BaseSchemaName"];
		DataColumn column16 = dataTable.Columns["BaseCatalogName"];
		DataColumn column17 = dataTable.Columns["BaseTableName"];
		DataColumn column18 = dataTable.Columns["BaseColumnName"];
		int fieldCount = FieldCount;
		for (int i = 0; i < fieldCount; i++)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow[column] = GetName(i);
			dataRow[column2] = i;
			dataRow[column3] = (int)Math.Min(Math.Max(-2147483648L, _metadata[i].size.ToInt64()), 2147483647L);
			dataRow[column4] = (short)_metadata[i].precision;
			dataRow[column5] = (short)_metadata[i].scale;
			dataRow[column6] = _metadata[i].typemap._type;
			dataRow[column7] = _metadata[i].typemap._odbcType;
			dataRow[column8] = _metadata[i].isLong;
			dataRow[column9] = _metadata[i].isNullable;
			dataRow[column10] = _metadata[i].isReadOnly;
			dataRow[column11] = _metadata[i].isRowVersion;
			dataRow[column12] = _metadata[i].isUnique;
			dataRow[column13] = _metadata[i].isKeyColumn;
			dataRow[column14] = _metadata[i].isAutoIncrement;
			dataRow[column15] = _metadata[i].baseSchemaName;
			dataRow[column16] = _metadata[i].baseCatalogName;
			dataRow[column17] = _metadata[i].baseTableName;
			dataRow[column18] = _metadata[i].baseColumnName;
			dataTable.Rows.Add(dataRow);
			dataRow.AcceptChanges();
		}
		_schemaTable = dataTable;
		return dataTable;
	}

	internal int RetrieveKeyInfo(bool needkeyinfo, QualifiedTableName qualifiedTableName, bool quoted)
	{
		int num = 0;
		IntPtr zero = IntPtr.Zero;
		if (IsClosed || _cmdWrapper == null)
		{
			return 0;
		}
		_cmdWrapper.CreateKeyInfoStatementHandle();
		CNativeBuffer buffer = Buffer;
		bool success = false;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			buffer.DangerousAddRef(ref success);
			ODBC32.RetCode retCode;
			if (needkeyinfo)
			{
				if (!Connection.ProviderInfo.NoSqlPrimaryKeys)
				{
					retCode = KeyInfoStatementHandle.PrimaryKeys(qualifiedTableName.Catalog, qualifiedTableName.Schema, qualifiedTableName.GetTable(quoted));
					if (retCode == ODBC32.RetCode.SUCCESS || retCode == ODBC32.RetCode.SUCCESS_WITH_INFO)
					{
						bool flag = false;
						buffer.WriteInt16(0, 0);
						retCode = KeyInfoStatementHandle.BindColumn2(4, ODBC32.SQL_C.WCHAR, buffer.PtrOffset(0, 256), (IntPtr)256, buffer.PtrOffset(256, IntPtr.Size).Handle);
						while ((retCode = KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
						{
							zero = buffer.ReadIntPtr(256);
							string text = buffer.PtrToStringUni(0, (int)zero / 2);
							int ordinalFromBaseColName = GetOrdinalFromBaseColName(text);
							if (ordinalFromBaseColName != -1)
							{
								num++;
								_metadata[ordinalFromBaseColName].isKeyColumn = true;
								_metadata[ordinalFromBaseColName].isUnique = true;
								_metadata[ordinalFromBaseColName].isNullable = false;
								_metadata[ordinalFromBaseColName].baseTableName = qualifiedTableName.Table;
								if (_metadata[ordinalFromBaseColName].baseColumnName == null)
								{
									_metadata[ordinalFromBaseColName].baseColumnName = text;
								}
								continue;
							}
							flag = true;
							break;
						}
						if (flag)
						{
							MetaData[] metadata = _metadata;
							for (int i = 0; i < metadata.Length; i++)
							{
								metadata[i].isKeyColumn = false;
							}
						}
						retCode = KeyInfoStatementHandle.BindColumn3(4, ODBC32.SQL_C.WCHAR, buffer.DangerousGetHandle());
					}
					else if ("IM001" == Command.GetDiagSqlState())
					{
						Connection.ProviderInfo.NoSqlPrimaryKeys = true;
					}
				}
				if (num == 0)
				{
					KeyInfoStatementHandle.MoreResults();
					num += RetrieveKeyInfoFromStatistics(qualifiedTableName, quoted);
				}
				KeyInfoStatementHandle.MoreResults();
			}
			retCode = KeyInfoStatementHandle.SpecialColumns(qualifiedTableName.GetTable(quoted));
			if (retCode == ODBC32.RetCode.SUCCESS || retCode == ODBC32.RetCode.SUCCESS_WITH_INFO)
			{
				zero = IntPtr.Zero;
				buffer.WriteInt16(0, 0);
				retCode = KeyInfoStatementHandle.BindColumn2(2, ODBC32.SQL_C.WCHAR, buffer.PtrOffset(0, 256), (IntPtr)256, buffer.PtrOffset(256, IntPtr.Size).Handle);
				while ((retCode = KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
				{
					zero = buffer.ReadIntPtr(256);
					string text = buffer.PtrToStringUni(0, (int)zero / 2);
					int ordinalFromBaseColName = GetOrdinalFromBaseColName(text);
					if (ordinalFromBaseColName != -1)
					{
						_metadata[ordinalFromBaseColName].isRowVersion = true;
						if (_metadata[ordinalFromBaseColName].baseColumnName == null)
						{
							_metadata[ordinalFromBaseColName].baseColumnName = text;
						}
					}
				}
				retCode = KeyInfoStatementHandle.BindColumn3(2, ODBC32.SQL_C.WCHAR, buffer.DangerousGetHandle());
				retCode = KeyInfoStatementHandle.MoreResults();
			}
		}
		finally
		{
			if (success)
			{
				buffer.DangerousRelease();
			}
		}
		return num;
	}

	private int RetrieveKeyInfoFromStatistics(QualifiedTableName qualifiedTableName, bool quoted)
	{
		string text = string.Empty;
		string empty = string.Empty;
		string currentindexname = string.Empty;
		int[] array = new int[16];
		int[] array2 = new int[16];
		int num = 0;
		int num2 = 0;
		bool flag = false;
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		int num3 = 0;
		string tableName = string.Copy(qualifiedTableName.GetTable(quoted));
		if (KeyInfoStatementHandle.Statistics(tableName) != ODBC32.RetCode.SUCCESS)
		{
			return 0;
		}
		CNativeBuffer buffer = Buffer;
		bool success = false;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			buffer.DangerousAddRef(ref success);
			HandleRef buffer2 = buffer.PtrOffset(0, 256);
			HandleRef buffer3 = buffer.PtrOffset(256, 256);
			HandleRef buffer4 = buffer.PtrOffset(512, 4);
			IntPtr handle = buffer.PtrOffset(520, IntPtr.Size).Handle;
			IntPtr handle2 = buffer.PtrOffset(528, IntPtr.Size).Handle;
			IntPtr handle3 = buffer.PtrOffset(536, IntPtr.Size).Handle;
			buffer.WriteInt16(256, 0);
			ODBC32.RetCode retCode = KeyInfoStatementHandle.BindColumn2(6, ODBC32.SQL_C.WCHAR, buffer3, (IntPtr)256, handle2);
			retCode = KeyInfoStatementHandle.BindColumn2(8, ODBC32.SQL_C.SSHORT, buffer4, (IntPtr)4, handle3);
			buffer.WriteInt16(512, 0);
			retCode = KeyInfoStatementHandle.BindColumn2(9, ODBC32.SQL_C.WCHAR, buffer2, (IntPtr)256, handle);
			while ((retCode = KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
			{
				zero2 = buffer.ReadIntPtr(520);
				zero = buffer.ReadIntPtr(528);
				if (buffer.ReadInt16(256) == 0)
				{
					continue;
				}
				text = buffer.PtrToStringUni(0, (int)zero2 / 2);
				empty = buffer.PtrToStringUni(256, (int)zero / 2);
				int ordinal = buffer.ReadInt16(512);
				if (SameIndexColumn(currentindexname, empty, ordinal, num2))
				{
					if (!flag)
					{
						ordinal = GetOrdinalFromBaseColName(text, qualifiedTableName.Table);
						if (ordinal == -1)
						{
							flag = true;
						}
						else if (num2 < 16)
						{
							array[num2++] = ordinal;
						}
						else
						{
							flag = true;
						}
					}
					continue;
				}
				if (!flag && num2 != 0 && (num == 0 || num > num2))
				{
					num = num2;
					for (int i = 0; i < num2; i++)
					{
						array2[i] = array[i];
					}
				}
				num2 = 0;
				currentindexname = empty;
				flag = false;
				ordinal = GetOrdinalFromBaseColName(text, qualifiedTableName.Table);
				if (ordinal == -1)
				{
					flag = true;
				}
				else
				{
					array[num2++] = ordinal;
				}
			}
			if (!flag && num2 != 0 && (num == 0 || num > num2))
			{
				num = num2;
				for (int j = 0; j < num2; j++)
				{
					array2[j] = array[j];
				}
			}
			if (num != 0)
			{
				for (int k = 0; k < num; k++)
				{
					int num4 = array2[k];
					num3++;
					_metadata[num4].isKeyColumn = true;
					_metadata[num4].isNullable = false;
					_metadata[num4].isUnique = true;
					if (_metadata[num4].baseTableName == null)
					{
						_metadata[num4].baseTableName = qualifiedTableName.Table;
					}
					if (_metadata[num4].baseColumnName == null)
					{
						_metadata[num4].baseColumnName = text;
					}
				}
			}
			_cmdWrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.UNBIND);
			return num3;
		}
		finally
		{
			if (success)
			{
				buffer.DangerousRelease();
			}
		}
	}

	internal bool SameIndexColumn(string currentindexname, string indexname, int ordinal, int ncols)
	{
		if (string.IsNullOrEmpty(currentindexname))
		{
			return false;
		}
		if (currentindexname == indexname && ordinal == ncols + 1)
		{
			return true;
		}
		return false;
	}

	internal int GetOrdinalFromBaseColName(string columnname)
	{
		return GetOrdinalFromBaseColName(columnname, null);
	}

	internal int GetOrdinalFromBaseColName(string columnname, string tablename)
	{
		if (string.IsNullOrEmpty(columnname))
		{
			return -1;
		}
		if (_metadata != null)
		{
			int fieldCount = FieldCount;
			for (int i = 0; i < fieldCount; i++)
			{
				if (_metadata[i].baseColumnName != null && columnname == _metadata[i].baseColumnName)
				{
					if (string.IsNullOrEmpty(tablename))
					{
						return i;
					}
					if (tablename == _metadata[i].baseTableName)
					{
						return i;
					}
				}
			}
		}
		return IndexOf(columnname);
	}

	internal string GetTableNameFromCommandText()
	{
		if (_command == null)
		{
			return null;
		}
		string cmdText = _cmdText;
		if (string.IsNullOrEmpty(cmdText))
		{
			return null;
		}
		CStringTokenizer cStringTokenizer = new CStringTokenizer(cmdText, Connection.QuoteChar("GetSchemaTable")[0], Connection.EscapeChar("GetSchemaTable"));
		int num = (cStringTokenizer.StartsWith("select") ? cStringTokenizer.FindTokenIndex("from") : ((!cStringTokenizer.StartsWith("insert") && !cStringTokenizer.StartsWith("update") && !cStringTokenizer.StartsWith("delete")) ? (-1) : cStringTokenizer.CurrentPosition));
		if (num == -1)
		{
			return null;
		}
		string result = cStringTokenizer.NextToken();
		cmdText = cStringTokenizer.NextToken();
		if (cmdText.Length > 0 && cmdText[0] == ',')
		{
			return null;
		}
		if (cmdText.Length == 2 && (cmdText[0] == 'a' || cmdText[0] == 'A') && (cmdText[1] == 's' || cmdText[1] == 'S'))
		{
			cmdText = cStringTokenizer.NextToken();
			cmdText = cStringTokenizer.NextToken();
			if (cmdText.Length > 0 && cmdText[0] == ',')
			{
				return null;
			}
		}
		return result;
	}

	internal void SetBaseTableNames(QualifiedTableName qualifiedTableName)
	{
		int fieldCount = FieldCount;
		for (int i = 0; i < fieldCount; i++)
		{
			if (_metadata[i].baseTableName == null)
			{
				_metadata[i].baseTableName = qualifiedTableName.Table;
				_metadata[i].baseSchemaName = qualifiedTableName.Schema;
				_metadata[i].baseCatalogName = qualifiedTableName.Catalog;
			}
		}
	}

	internal OdbcDataReader()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
