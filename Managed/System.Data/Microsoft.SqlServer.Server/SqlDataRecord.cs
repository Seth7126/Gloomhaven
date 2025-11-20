using System;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Server;

/// <summary>Represents a single row of data and its metadata. This class cannot be inherited.</summary>
public class SqlDataRecord : IDataRecord
{
	private SmiRecordBuffer _recordBuffer;

	private SmiExtendedMetaData[] _columnSmiMetaData;

	private SmiEventSink_Default _eventSink;

	private SqlMetaData[] _columnMetaData;

	private FieldNameLookup _fieldNameLookup;

	private bool _usesStringStorageForXml;

	private static readonly SmiMetaData s_maxNVarCharForXml = new SmiMetaData(SqlDbType.NVarChar, -1L, SmiMetaData.DefaultNVarChar_NoCollation.Precision, SmiMetaData.DefaultNVarChar_NoCollation.Scale, SmiMetaData.DefaultNVarChar.LocaleId, SmiMetaData.DefaultNVarChar.CompareOptions, null);

	/// <summary>Gets the number of columns in the data row. This property is read-only.</summary>
	/// <returns>The number of columns in the data row as an integer.</returns>
	public virtual int FieldCount
	{
		get
		{
			EnsureSubclassOverride();
			return _columnMetaData.Length;
		}
	}

	/// <summary>Gets the common language runtime (CLR) type value for the column specified by the column <paramref name="ordinal" /> argument.</summary>
	/// <returns>The CLR type value of the column specified by the <paramref name="ordinal" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual object this[int ordinal]
	{
		get
		{
			EnsureSubclassOverride();
			return GetValue(ordinal);
		}
	}

	/// <summary>Gets the common language runtime (CLR) type value for the column specified by the column <paramref name="name" /> argument.</summary>
	/// <returns>The CLR type value of the column specified by the <paramref name="name" />.</returns>
	/// <param name="name">The name of the column.</param>
	public virtual object this[string name]
	{
		get
		{
			EnsureSubclassOverride();
			return GetValue(GetOrdinal(name));
		}
	}

	internal SmiRecordBuffer RecordBuffer => _recordBuffer;

	/// <summary>Returns the name of the column specified by the ordinal argument.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the column name.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />). </exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual string GetName(int ordinal)
	{
		EnsureSubclassOverride();
		return GetSqlMetaData(ordinal).Name;
	}

	/// <summary>Returns the name of the data type for the column specified by the ordinal argument.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the data type of the column.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />). </exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual string GetDataTypeName(int ordinal)
	{
		EnsureSubclassOverride();
		SqlMetaData sqlMetaData = GetSqlMetaData(ordinal);
		if (SqlDbType.Udt == sqlMetaData.SqlDbType)
		{
			return sqlMetaData.UdtTypeName;
		}
		return MetaType.GetMetaTypeFromSqlDbType(sqlMetaData.SqlDbType, isMultiValued: false).TypeName;
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object representing the common language runtime (CLR) type that maps to the SQL Server type of the column specified by the <paramref name="ordinal" /> argument.</summary>
	/// <returns>The column type as a <see cref="T:System.Type" /> object.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />). </exception>
	/// <exception cref="T:System.TypeLoadException">The column is of a user-defined type that is not available to the calling application domain.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The column is of a user-defined type that is not available to the calling application domain.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual Type GetFieldType(int ordinal)
	{
		EnsureSubclassOverride();
		return MetaType.GetMetaTypeFromSqlDbType(GetSqlMetaData(ordinal).SqlDbType, isMultiValued: false).ClassType;
	}

	/// <summary>Returns the common language runtime (CLR) type value for the column specified by the ordinal argument.</summary>
	/// <returns>The CLR type value of the column specified by the ordinal.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual object GetValue(int ordinal)
	{
		EnsureSubclassOverride();
		SmiMetaData smiMetaData = GetSmiMetaData(ordinal);
		return ValueUtilsSmi.GetValue200(_eventSink, _recordBuffer, ordinal, smiMetaData);
	}

	/// <summary>Returns the values for all the columns in the record, expressed as common language runtime (CLR) types, in an array.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that indicates the number of columns copied.</returns>
	/// <param name="values">The array into which to copy the values column values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual int GetValues(object[] values)
	{
		EnsureSubclassOverride();
		if (values == null)
		{
			throw ADP.ArgumentNull("values");
		}
		int num = ((values.Length < FieldCount) ? values.Length : FieldCount);
		for (int i = 0; i < num; i++)
		{
			values[i] = GetValue(i);
		}
		return num;
	}

	/// <summary>Returns the column ordinal specified by the column name.</summary>
	/// <returns>The zero-based ordinal of the column as an integer.</returns>
	/// <param name="name">The name of the column to look up.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column name could not be found.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual int GetOrdinal(string name)
	{
		EnsureSubclassOverride();
		if (_fieldNameLookup == null)
		{
			string[] array = new string[FieldCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetSqlMetaData(i).Name;
			}
			_fieldNameLookup = new FieldNameLookup(array, -1);
		}
		return _fieldNameLookup.GetOrdinal(name);
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Boolean" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Boolean" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual bool GetBoolean(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetBoolean(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Byte" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Byte" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual byte GetByte(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetByte(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as an array of <see cref="T:System.Byte" /> objects.</summary>
	/// <returns>The number of bytes copied.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="fieldOffset">The offset into the field value to start retrieving bytes.</param>
	/// <param name="buffer">The target buffer to which to copy bytes.</param>
	/// <param name="bufferOffset">The offset into the buffer to which to start copying bytes.</param>
	/// <param name="length">The number of bytes to copy to the buffer.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetBytes(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length, throwOnNull: true);
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Char" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Char" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual char GetChar(int ordinal)
	{
		EnsureSubclassOverride();
		throw ADP.NotSupported();
	}

	/// <summary>Gets the value for the column specified by the ordinal as an array of <see cref="T:System.Char" /> objects.</summary>
	/// <returns>The number of characters copied.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="fieldOffset">The offset into the field value to start retrieving characters.</param>
	/// <param name="buffer">The target buffer to copy chars to.</param>
	/// <param name="bufferOffset">The offset into the buffer to start copying chars to.</param>
	/// <param name="length">The number of chars to copy to the buffer.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetChars(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Guid" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Guid" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual Guid GetGuid(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetGuid(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Int16" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Int16" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual short GetInt16(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetInt16(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Int32" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Int32" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual int GetInt32(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetInt32(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Int64" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Int64" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual long GetInt64(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetInt64(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a float.</summary>
	/// <returns>The column value as a float.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual float GetFloat(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSingle(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Double" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Double" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual double GetDouble(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetDouble(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.String" />.</summary>
	/// <returns>The column value as a <see cref="T:System.String" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual string GetString(int ordinal)
	{
		EnsureSubclassOverride();
		SmiMetaData smiMetaData = GetSmiMetaData(ordinal);
		if (_usesStringStorageForXml && SqlDbType.Xml == smiMetaData.SqlDbType)
		{
			return ValueUtilsSmi.GetString(_eventSink, _recordBuffer, ordinal, s_maxNVarCharForXml);
		}
		return ValueUtilsSmi.GetString(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Decimal" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Decimal" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual decimal GetDecimal(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetDecimal(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The column value as a <see cref="T:System.DateTime" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.Data.SqlTypes.SqlNullValueException">The column specified by <paramref name="ordinal" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual DateTime GetDateTime(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetDateTime(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Returns the specified column’s data as a <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.DateTimeOffset" />.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	public virtual DateTimeOffset GetDateTimeOffset(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetDateTimeOffset(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Returns the specified column’s data as a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The value of the specified column as a <see cref="T:System.TimeSpan" />.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	public virtual TimeSpan GetTimeSpan(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetTimeSpan(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Returns true if the column specified by the column ordinal parameter is null.</summary>
	/// <returns>true if the column is null; false otherwise.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual bool IsDBNull(int ordinal)
	{
		EnsureSubclassOverride();
		ThrowIfInvalidOrdinal(ordinal);
		return ValueUtilsSmi.IsDBNull(_eventSink, _recordBuffer, ordinal);
	}

	/// <summary>Returns a <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> object, describing the metadata of the column specified by the column ordinal.</summary>
	/// <returns>The column metadata as a <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> object.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlMetaData GetSqlMetaData(int ordinal)
	{
		EnsureSubclassOverride();
		return _columnMetaData[ordinal];
	}

	/// <summary>Returns a <see cref="T:System.Type" /> object that represents the type (as a SQL Server type, defined in <see cref="N:System.Data.SqlTypes" />) that maps to the SQL Server type of the column.</summary>
	/// <returns>The column type as a <see cref="T:System.Type" /> object.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />). </exception>
	/// <exception cref="T:System.TypeLoadException">The column is of a user-defined type that is not available to the calling application domain.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The column is of a user-defined type that is not available to the calling application domain.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual Type GetSqlFieldType(int ordinal)
	{
		EnsureSubclassOverride();
		return MetaType.GetMetaTypeFromSqlDbType(GetSqlMetaData(ordinal).SqlDbType, isMultiValued: false).SqlType;
	}

	/// <summary>Returns the data value stored in the column, expressed as a SQL Server type, specified by the column ordinal.</summary>
	/// <returns>The value of the column, expressed as a SQL Server type, as a <see cref="T:System.Object" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual object GetSqlValue(int ordinal)
	{
		EnsureSubclassOverride();
		SmiMetaData smiMetaData = GetSmiMetaData(ordinal);
		return ValueUtilsSmi.GetSqlValue200(_eventSink, _recordBuffer, ordinal, smiMetaData);
	}

	/// <summary>Returns the values for all the columns in the record, expressed as SQL Server types, in an array.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that indicates the number of columns copied.</returns>
	/// <param name="values">The array into which to copy the values column values. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual int GetSqlValues(object[] values)
	{
		EnsureSubclassOverride();
		if (values == null)
		{
			throw ADP.ArgumentNull("values");
		}
		int num = ((values.Length < FieldCount) ? values.Length : FieldCount);
		for (int i = 0; i < num; i++)
		{
			values[i] = GetSqlValue(i);
		}
		return num;
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlBinary" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlBinary" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlBinary GetSqlBinary(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlBinary(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlBytes" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlBytes" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlBytes GetSqlBytes(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlBytes(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlXml" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlXml" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlXml GetSqlXml(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlXml(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlBoolean" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlBoolean GetSqlBoolean(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlBoolean(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlByte" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlByte" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlByte GetSqlByte(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlByte(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlChars" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlChars" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlChars GetSqlChars(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlChars(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlInt16" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlInt16" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlInt16 GetSqlInt16(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlInt16(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlInt32" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlInt32" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlInt32 GetSqlInt32(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlInt32(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlInt64" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlInt64" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlInt64 GetSqlInt64(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlInt64(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlSingle" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlSingle" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlSingle GetSqlSingle(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlSingle(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlDouble" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlDouble" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlDouble GetSqlDouble(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlDouble(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlMoney" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlMoney" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlMoney GetSqlMoney(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlMoney(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlDateTime" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlDateTime" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlDateTime GetSqlDateTime(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlDateTime(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlDecimal" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlDecimal GetSqlDecimal(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlDecimal(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlString" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlString" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlString GetSqlString(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlString(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Gets the value for the column specified by the ordinal as a <see cref="T:System.Data.SqlTypes.SqlGuid" />.</summary>
	/// <returns>The column value as a <see cref="T:System.Data.SqlTypes.SqlGuid" />.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	/// <exception cref="T:System.InvalidCastException">There is a type mismatch.</exception>
	public virtual SqlGuid GetSqlGuid(int ordinal)
	{
		EnsureSubclassOverride();
		return ValueUtilsSmi.GetSqlGuid(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal));
	}

	/// <summary>Sets new values for all of the columns in the <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" />. These values are expressed as common language runtime (CLR) types.</summary>
	/// <returns>The number of column values set as an integer.</returns>
	/// <param name="values">The array of new values, expressed as CLR types boxed as <see cref="T:System.Object" /> references, for the <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The size of values does not match the number of columns in the <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" /> instance.</exception>
	public virtual int SetValues(params object[] values)
	{
		EnsureSubclassOverride();
		if (values == null)
		{
			throw ADP.ArgumentNull("values");
		}
		int num = ((values.Length > FieldCount) ? FieldCount : values.Length);
		ExtendedClrTypeCode[] array = new ExtendedClrTypeCode[num];
		for (int i = 0; i < num; i++)
		{
			SqlMetaData sqlMetaData = GetSqlMetaData(i);
			array[i] = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(sqlMetaData.SqlDbType, isMultiValued: false, values[i], sqlMetaData.Type);
			if (ExtendedClrTypeCode.Invalid == array[i])
			{
				throw ADP.InvalidCast();
			}
		}
		for (int j = 0; j < num; j++)
		{
			ValueUtilsSmi.SetCompatibleValueV200(_eventSink, _recordBuffer, j, GetSmiMetaData(j), values[j], array[j], 0, 0, null);
		}
		return num;
	}

	/// <summary>Sets a new value, expressed as a common language runtime (CLR) type, for the column specified by the column ordinal.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value for the specified column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetValue(int ordinal, object value)
	{
		EnsureSubclassOverride();
		SqlMetaData sqlMetaData = GetSqlMetaData(ordinal);
		ExtendedClrTypeCode extendedClrTypeCode = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(sqlMetaData.SqlDbType, isMultiValued: false, value, sqlMetaData.Type);
		if (ExtendedClrTypeCode.Invalid == extendedClrTypeCode)
		{
			throw ADP.InvalidCast();
		}
		ValueUtilsSmi.SetCompatibleValueV200(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value, extendedClrTypeCode, 0, 0, null);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Boolean" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	public virtual void SetBoolean(int ordinal, bool value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetBoolean(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Byte" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetByte(int ordinal, byte value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetByte(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified array of <see cref="T:System.Byte" /> values.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="fieldOffset">The offset into the field value to start copying bytes.</param>
	/// <param name="buffer">The target buffer from which to copy bytes.</param>
	/// <param name="bufferOffset">The offset into the buffer from which to start copying bytes.</param>
	/// <param name="length">The number of bytes to copy from the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetBytes(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Char" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetChar(int ordinal, char value)
	{
		EnsureSubclassOverride();
		throw ADP.NotSupported();
	}

	/// <summary>Sets the data stored in the column to the specified array of <see cref="T:System.Char" /> values.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="fieldOffset">The offset into the field value to start copying characters.</param>
	/// <param name="buffer">The target buffer from which to copy chars.</param>
	/// <param name="bufferOffset">The offset into the buffer from which to start copying chars.</param>
	/// <param name="length">The number of chars to copy from the buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetChars(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Int16" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetInt16(int ordinal, short value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetInt16(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Int32" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetInt32(int ordinal, int value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetInt32(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Int64" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetInt64(int ordinal, long value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetInt64(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified float value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetFloat(int ordinal, float value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSingle(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Double" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetDouble(int ordinal, double value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetDouble(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.String" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetString(int ordinal, string value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetString(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Decimal" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetDecimal(int ordinal, decimal value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetDecimal(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.DateTime" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetDateTime(int ordinal, DateTime value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetDateTime(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the value of the column specified to the <see cref="T:System.TimeSpan" />.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> passed in is a negative number.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.TimeSpan" /> value passed in is greater than 24 hours in length.</exception>
	public virtual void SetTimeSpan(int ordinal, TimeSpan value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetTimeSpan(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the value of the column specified to the <see cref="T:System.DateTimeOffset" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	public virtual void SetDateTimeOffset(int ordinal, DateTimeOffset value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetDateTimeOffset(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the value in the specified column to <see cref="T:System.DBNull" />.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	public virtual void SetDBNull(int ordinal)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetDBNull(_eventSink, _recordBuffer, ordinal, value: true);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Guid" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetGuid(int ordinal, Guid value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetGuid(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlBoolean" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlBoolean(int ordinal, SqlBoolean value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlBoolean(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlByte" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlByte(int ordinal, SqlByte value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlByte(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlInt16" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlInt16(int ordinal, SqlInt16 value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlInt16(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlInt32" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlInt32(int ordinal, SqlInt32 value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlInt32(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlInt64" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlInt64(int ordinal, SqlInt64 value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlInt64(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlSingle" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlSingle(int ordinal, SqlSingle value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlSingle(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlDouble" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlDouble(int ordinal, SqlDouble value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlDouble(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlMoney" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlMoney(int ordinal, SqlMoney value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlMoney(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlDateTime" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlDateTime(int ordinal, SqlDateTime value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlDateTime(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlXml" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlXml(int ordinal, SqlXml value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlXml(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlDecimal" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlDecimal(int ordinal, SqlDecimal value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlDecimal(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlString" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlString(int ordinal, SqlString value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlString(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlBinary" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlBinary(int ordinal, SqlBinary value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlBinary(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlGuid" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlGuid(int ordinal, SqlGuid value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlGuid(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlChars" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlChars(int ordinal, SqlChars value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlChars(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Sets the data stored in the column to the specified <see cref="T:System.Data.SqlTypes.SqlBytes" /> value.</summary>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <param name="value">The new value of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	public virtual void SetSqlBytes(int ordinal, SqlBytes value)
	{
		EnsureSubclassOverride();
		ValueUtilsSmi.SetSqlBytes(_eventSink, _recordBuffer, ordinal, GetSmiMetaData(ordinal), value);
	}

	/// <summary>Inititializes a new <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" /> instance with the schema based on the array of <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> objects passed as an argument.</summary>
	/// <param name="metaData">An array of <see cref="T:Microsoft.SqlServer.Server.SqlMetaData" /> objects that describe each column in the <see cref="T:Microsoft.SqlServer.Server.SqlDataRecord" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="metaData" /> is null. </exception>
	public SqlDataRecord(params SqlMetaData[] metaData)
	{
		if (metaData == null)
		{
			throw ADP.ArgumentNull("metaData");
		}
		_columnMetaData = new SqlMetaData[metaData.Length];
		_columnSmiMetaData = new SmiExtendedMetaData[metaData.Length];
		for (int i = 0; i < _columnSmiMetaData.Length; i++)
		{
			if (metaData[i] == null)
			{
				throw ADP.ArgumentNull(string.Format("{0}[{1}]", "metaData", i));
			}
			_columnMetaData[i] = metaData[i];
			_columnSmiMetaData[i] = MetaDataUtilsSmi.SqlMetaDataToSmiExtendedMetaData(_columnMetaData[i]);
		}
		_eventSink = new SmiEventSink_Default();
		SmiMetaData[] columnSmiMetaData = _columnSmiMetaData;
		_recordBuffer = new MemoryRecordBuffer(columnSmiMetaData);
		_usesStringStorageForXml = true;
		_eventSink.ProcessMessagesAndThrow();
	}

	internal SqlDataRecord(SmiRecordBuffer recordBuffer, params SmiExtendedMetaData[] metaData)
	{
		_columnMetaData = new SqlMetaData[metaData.Length];
		_columnSmiMetaData = new SmiExtendedMetaData[metaData.Length];
		for (int i = 0; i < _columnSmiMetaData.Length; i++)
		{
			_columnSmiMetaData[i] = metaData[i];
			_columnMetaData[i] = MetaDataUtilsSmi.SmiExtendedMetaDataToSqlMetaData(_columnSmiMetaData[i]);
		}
		_eventSink = new SmiEventSink_Default();
		_recordBuffer = recordBuffer;
		_eventSink.ProcessMessagesAndThrow();
	}

	internal SqlMetaData[] InternalGetMetaData()
	{
		return _columnMetaData;
	}

	internal SmiExtendedMetaData[] InternalGetSmiMetaData()
	{
		return _columnSmiMetaData;
	}

	internal SmiExtendedMetaData GetSmiMetaData(int ordinal)
	{
		return _columnSmiMetaData[ordinal];
	}

	internal void ThrowIfInvalidOrdinal(int ordinal)
	{
		if (0 > ordinal || FieldCount <= ordinal)
		{
			throw ADP.IndexOutOfRange(ordinal);
		}
	}

	private void EnsureSubclassOverride()
	{
		if (_recordBuffer == null)
		{
			throw SQL.SubclassMustOverride();
		}
	}

	/// <summary>Not supported in this release.</summary>
	/// <returns>
	///   <see cref="T:System.Data.IDataReader" />Always throws an exception.</returns>
	/// <param name="ordinal">The zero-based ordinal of the column.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="ordinal" /> is less than 0 or greater than the number of columns (that is, <see cref="P:Microsoft.SqlServer.Server.SqlDataRecord.FieldCount" />).</exception>
	IDataReader IDataRecord.GetData(int ordinal)
	{
		throw ADP.NotSupported();
	}
}
