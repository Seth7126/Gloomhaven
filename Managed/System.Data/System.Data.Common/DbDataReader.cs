using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Common;

/// <summary>Reads a forward-only stream of rows from a data source.</summary>
/// <filterpriority>1</filterpriority>
public abstract class DbDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable, IAsyncDisposable
{
	/// <summary>Gets a value indicating the depth of nesting for the current row.</summary>
	/// <returns>The depth of nesting for the current row.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract int Depth { get; }

	/// <summary>Gets the number of columns in the current row.</summary>
	/// <returns>The number of columns in the current row.</returns>
	/// <exception cref="T:System.NotSupportedException">There is no current connection to an instance of SQL Server. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract int FieldCount { get; }

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Data.Common.DbDataReader" /> contains one or more rows.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbDataReader" /> contains one or more rows; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool HasRows { get; }

	/// <summary>Gets a value indicating whether the <see cref="T:System.Data.Common.DbDataReader" /> is closed.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbDataReader" /> is closed; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlDataReader" /> is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract bool IsClosed { get; }

	/// <summary>Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. </summary>
	/// <returns>The number of rows changed, inserted, or deleted. -1 for SELECT statements; 0 if no rows were affected or the statement failed.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract int RecordsAffected { get; }

	/// <summary>Gets the number of fields in the <see cref="T:System.Data.Common.DbDataReader" /> that are not hidden.</summary>
	/// <returns>The number of fields that are not hidden.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual int VisibleFieldCount => FieldCount;

	/// <summary>Gets the value of the specified column as an instance of <see cref="T:System.Object" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract object this[int ordinal] { get; }

	/// <summary>Gets the value of the specified column as an instance of <see cref="T:System.Object" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="name">The name of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract object this[string name] { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbDataReader" /> class.</summary>
	protected DbDataReader()
	{
	}

	/// <summary>Closes the <see cref="T:System.Data.Common.DbDataReader" /> object.</summary>
	/// <filterpriority>1</filterpriority>
	public virtual void Close()
	{
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Data.Common.DbDataReader" /> class.</summary>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the managed resources used by the <see cref="T:System.Data.Common.DbDataReader" /> and optionally releases the unmanaged resources.</summary>
	/// <param name="disposing">true to release managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}

	/// <summary>Gets name of the data type of the specified column.</summary>
	/// <returns>A string representing the name of the data type.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract string GetDataTypeName(int ordinal);

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the rows in the data reader.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the rows in the data reader.</returns>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract IEnumerator GetEnumerator();

	/// <summary>Gets the data type of the specified column.</summary>
	/// <returns>The data type of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract Type GetFieldType(int ordinal);

	/// <summary>Gets the name of the column, given the zero-based column ordinal.</summary>
	/// <returns>The name of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>1</filterpriority>
	public abstract string GetName(int ordinal);

	/// <summary>Gets the column ordinal given the name of the column.</summary>
	/// <returns>The zero-based column ordinal.</returns>
	/// <param name="name">The name of the column.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The name specified is not a valid column name.</exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetOrdinal(string name);

	/// <summary>Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.Common.DbDataReader" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> that describes the column metadata.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlDataReader" /> is closed. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual DataTable GetSchemaTable()
	{
		throw new NotSupportedException();
	}

	/// <summary>Gets the value of the specified column as a Boolean.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract bool GetBoolean(int ordinal);

	/// <summary>Gets the value of the specified column as a byte.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract byte GetByte(int ordinal);

	/// <summary>Reads a stream of bytes from the specified column, starting at location indicated by <paramref name="dataOffset" />, into the buffer, starting at the location indicated by <paramref name="bufferOffset" />.</summary>
	/// <returns>The actual number of bytes read.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
	/// <param name="buffer">The buffer into which to copy the data.</param>
	/// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
	/// <param name="length">The maximum number of characters to read.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);

	/// <summary>Gets the value of the specified column as a single character.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract char GetChar(int ordinal);

	/// <summary>Reads a stream of characters from the specified column, starting at location indicated by <paramref name="dataOffset" />, into the buffer, starting at the location indicated by <paramref name="bufferOffset" />.</summary>
	/// <returns>The actual number of characters read.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
	/// <param name="buffer">The buffer into which to copy the data.</param>
	/// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
	/// <param name="length">The maximum number of characters to read.</param>
	/// <filterpriority>1</filterpriority>
	public abstract long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);

	/// <summary>Returns a <see cref="T:System.Data.Common.DbDataReader" /> object for the requested column ordinal.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbDataReader" /> object.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public DbDataReader GetData(int ordinal)
	{
		return GetDbDataReader(ordinal);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDataRecord.GetData(System.Int32)" />.</summary>
	/// <returns>An instance of <see cref="T:System.Data.IDataReader" /> to be used when the field points to more remote structured data.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	IDataReader IDataRecord.GetData(int ordinal)
	{
		return GetDbDataReader(ordinal);
	}

	/// <summary>Returns a <see cref="T:System.Data.Common.DbDataReader" /> object for the requested column ordinal that can be overridden with a provider-specific implementation.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbDataReader" /> object.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	protected virtual DbDataReader GetDbDataReader(int ordinal)
	{
		throw ADP.NotSupported();
	}

	/// <summary>Gets the value of the specified column as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract DateTime GetDateTime(int ordinal);

	/// <summary>Gets the value of the specified column as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract decimal GetDecimal(int ordinal);

	/// <summary>Gets the value of the specified column as a double-precision floating point number.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract double GetDouble(int ordinal);

	/// <summary>Gets the value of the specified column as a single-precision floating point number.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract float GetFloat(int ordinal);

	/// <summary>Gets the value of the specified column as a globally-unique identifier (GUID).</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract Guid GetGuid(int ordinal);

	/// <summary>Gets the value of the specified column as a 16-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract short GetInt16(int ordinal);

	/// <summary>Gets the value of the specified column as a 32-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract int GetInt32(int ordinal);

	/// <summary>Gets the value of the specified column as a 64-bit signed integer.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract long GetInt64(int ordinal);

	/// <summary>Returns the provider-specific field type of the specified column.</summary>
	/// <returns>The <see cref="T:System.Type" /> object that describes the data type of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual Type GetProviderSpecificFieldType(int ordinal)
	{
		return GetFieldType(ordinal);
	}

	/// <summary>Gets the value of the specified column as an instance of <see cref="T:System.Object" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual object GetProviderSpecificValue(int ordinal)
	{
		return GetValue(ordinal);
	}

	/// <summary>Gets all provider-specific attribute columns in the collection for the current row.</summary>
	/// <returns>The number of instances of <see cref="T:System.Object" /> in the array.</returns>
	/// <param name="values">An array of <see cref="T:System.Object" /> into which to copy the attribute columns.</param>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual int GetProviderSpecificValues(object[] values)
	{
		return GetValues(values);
	}

	/// <summary>Gets the value of the specified column as an instance of <see cref="T:System.String" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public abstract string GetString(int ordinal);

	/// <summary>Retrieves data as a <see cref="T:System.IO.Stream" />.</summary>
	/// <returns>The returned object.</returns>
	/// <param name="ordinal">Retrieves data as a <see cref="T:System.IO.Stream" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Tried to read a previously-read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	/// <exception cref="T:System.InvalidCastException">The returned type was not one of the types below:binaryimagevarbinaryudt</exception>
	public virtual Stream GetStream(int ordinal)
	{
		using MemoryStream memoryStream = new MemoryStream();
		long num = 0L;
		long num2 = 0L;
		byte[] array = new byte[4096];
		do
		{
			num = GetBytes(ordinal, num2, array, 0, array.Length);
			memoryStream.Write(array, 0, (int)num);
			num2 += num;
		}
		while (num > 0);
		return new MemoryStream(memoryStream.ToArray(), writable: false);
	}

	/// <summary>Retrieves data as a <see cref="T:System.IO.TextReader" />.</summary>
	/// <returns>The returned object.</returns>
	/// <param name="ordinal">Retrieves data as a <see cref="T:System.IO.TextReader" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Tried to read a previously-read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	/// <exception cref="T:System.InvalidCastException">The returned type was not one of the types below:charncharntextnvarchartextvarchar</exception>
	public virtual TextReader GetTextReader(int ordinal)
	{
		if (IsDBNull(ordinal))
		{
			return new StringReader(string.Empty);
		}
		return new StringReader(GetString(ordinal));
	}

	/// <summary>Gets the value of the specified column as an instance of <see cref="T:System.Object" />.</summary>
	/// <returns>The value of the specified column.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>1</filterpriority>
	public abstract object GetValue(int ordinal);

	/// <summary>Synchronously gets the value of the specified column as a type.</summary>
	/// <returns>The column to be retrieved.</returns>
	/// <param name="ordinal">The column to be retrieved.</param>
	/// <typeparam name="T">Synchronously gets the value of the specified column as a type.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.SqlClient.SqlDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.SqlClient.SqlDataReader.Read" /> hasn't been called, or returned false).Tried to read a previously-read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="T" /> doesn’t match the type returned by SQL Server or cannot be cast.</exception>
	public virtual T GetFieldValue<T>(int ordinal)
	{
		return (T)GetValue(ordinal);
	}

	/// <summary>Asynchronously gets the value of the specified column as a type.</summary>
	/// <returns>The type of the value to be returned.</returns>
	/// <param name="ordinal">The type of the value to be returned.</param>
	/// <typeparam name="T">The type of the value to be returned. See the remarks section for more information.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Tried to read a previously-read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="T" /> doesn’t match the type returned by the data source  or cannot be cast.</exception>
	public Task<T> GetFieldValueAsync<T>(int ordinal)
	{
		return GetFieldValueAsync<T>(ordinal, CancellationToken.None);
	}

	/// <summary>Asynchronously gets the value of the specified column as a type.</summary>
	/// <returns>The type of the value to be returned.</returns>
	/// <param name="ordinal">The type of the value to be returned.</param>
	/// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled. This does not guarantee the cancellation. A setting of CancellationToken.None makes this method equivalent to <see cref="M:System.Data.Common.DbDataReader.GetFieldValueAsync``1(System.Int32)" />. The returned task must be marked as cancelled.</param>
	/// <typeparam name="T">The type of the value to be returned. See the remarks section for more information.</typeparam>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Tried to read a previously-read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="T" /> doesn’t match the type returned by the data source or cannot be cast.</exception>
	public virtual Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<T>();
		}
		try
		{
			return Task.FromResult(GetFieldValue<T>(ordinal));
		}
		catch (Exception exception)
		{
			return Task.FromException<T>(exception);
		}
	}

	/// <summary>Populates an array of objects with the column values of the current row.</summary>
	/// <returns>The number of instances of <see cref="T:System.Object" /> in the array.</returns>
	/// <param name="values">An array of <see cref="T:System.Object" /> into which to copy the attribute columns.</param>
	/// <filterpriority>1</filterpriority>
	public abstract int GetValues(object[] values);

	/// <summary>Gets a value that indicates whether the column contains nonexistent or missing values.</summary>
	/// <returns>true if the specified column is equivalent to <see cref="T:System.DBNull" />; otherwise false.</returns>
	/// <param name="ordinal">The zero-based column ordinal.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool IsDBNull(int ordinal);

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.IsDBNull(System.Int32)" />, which gets a value that indicates whether the column contains non-existent or missing values.</summary>
	/// <returns>true if the specified column value is equivalent to DBNull otherwise false.</returns>
	/// <param name="ordinal">The zero-based column to be retrieved.</param>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Trying to read a previously read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	public Task<bool> IsDBNullAsync(int ordinal)
	{
		return IsDBNullAsync(ordinal, CancellationToken.None);
	}

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.IsDBNull(System.Int32)" />, which gets a value that indicates whether the column contains non-existent or missing values. Optionally, sends a notification that operations should be cancelled.</summary>
	/// <returns>true if the specified column value is equivalent to DBNull otherwise false.</returns>
	/// <param name="ordinal">The zero-based column to be retrieved.</param>
	/// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled. This does not guarantee the cancellation. A setting of CancellationToken.None makes this method equivalent to <see cref="M:System.Data.Common.DbDataReader.IsDBNullAsync(System.Int32)" />. The returned task must be marked as cancelled.</param>
	/// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.The <see cref="T:System.Data.Common.DbDataReader" /> is closed during the data retrieval.There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read" /> hasn't been called, or returned false).Trying to read a previously read column in sequential mode.There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
	public virtual Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<bool>();
		}
		try
		{
			return IsDBNull(ordinal) ? ADP.TrueTask : ADP.FalseTask;
		}
		catch (Exception exception)
		{
			return Task.FromException<bool>(exception);
		}
	}

	/// <summary>Advances the reader to the next result when reading the results of a batch of statements.</summary>
	/// <returns>true if there are more result sets; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool NextResult();

	/// <summary>Advances the reader to the next record in a result set.</summary>
	/// <returns>true if there are more rows; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract bool Read();

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.Read" />, which advances the reader to the next record in a result set. This method invokes <see cref="M:System.Data.Common.DbDataReader.ReadAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public Task<bool> ReadAsync()
	{
		return ReadAsync(CancellationToken.None);
	}

	/// <summary>This is the asynchronous version of <see cref="M:System.Data.Common.DbDataReader.Read" />.  Providers should override with an appropriate implementation. The cancellationToken may optionally be ignored.The default implementation invokes the synchronous <see cref="M:System.Data.Common.DbDataReader.Read" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellationToken.  Exceptions thrown by Read will be communicated via the returned Task Exception property.Do not invoke other methods and properties of the DbDataReader object until the returned Task is complete.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The cancellation instruction.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public virtual Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<bool>();
		}
		try
		{
			return Read() ? ADP.TrueTask : ADP.FalseTask;
		}
		catch (Exception exception)
		{
			return Task.FromException<bool>(exception);
		}
	}

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.NextResult" />, which advances the reader to the next result when reading the results of a batch of statements.Invokes <see cref="M:System.Data.Common.DbDataReader.NextResultAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public Task<bool> NextResultAsync()
	{
		return NextResultAsync(CancellationToken.None);
	}

	/// <summary>This is the asynchronous version of <see cref="M:System.Data.Common.DbDataReader.NextResult" />. Providers should override with an appropriate implementation. The <paramref name="cancellationToken" /> may optionally be ignored.The default implementation invokes the synchronous <see cref="M:System.Data.Common.DbDataReader.NextResult" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled <paramref name="cancellationToken" />. Exceptions thrown by <see cref="M:System.Data.Common.DbDataReader.NextResult" /> will be communicated via the returned Task Exception property.Other methods and properties of the DbDataReader object should not be invoked while the returned Task is not yet completed.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The cancellation instruction.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public virtual Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<bool>();
		}
		try
		{
			return NextResult() ? ADP.TrueTask : ADP.FalseTask;
		}
		catch (Exception exception)
		{
			return Task.FromException<bool>(exception);
		}
	}

	public virtual Task CloseAsync()
	{
		try
		{
			Close();
			return Task.CompletedTask;
		}
		catch (Exception exception)
		{
			return Task.FromException(exception);
		}
	}

	public virtual ValueTask DisposeAsync()
	{
		Dispose();
		return default(ValueTask);
	}
}
