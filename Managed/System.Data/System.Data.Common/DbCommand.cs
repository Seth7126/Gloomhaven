using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Common;

/// <summary>Represents an SQL statement or stored procedure to execute against a data source. Provides a base class for database-specific classes that represent commands. <see cref="Overload:System.Data.Common.DbCommand.ExecuteNonQueryAsync" /></summary>
/// <filterpriority>1</filterpriority>
public abstract class DbCommand : Component, IDbCommand, IDisposable, IAsyncDisposable
{
	/// <summary>Gets or sets the text command to run against the data source.</summary>
	/// <returns>The text command to execute. The default value is an empty string ("").</returns>
	/// <filterpriority>1</filterpriority>
	[RefreshProperties(RefreshProperties.All)]
	[DefaultValue("")]
	public abstract string CommandText { get; set; }

	/// <summary>Gets or sets the wait time before terminating the attempt to execute a command and generating an error.</summary>
	/// <returns>The time in seconds to wait for the command to execute.</returns>
	/// <filterpriority>2</filterpriority>
	public abstract int CommandTimeout { get; set; }

	/// <summary>Indicates or specifies how the <see cref="P:System.Data.Common.DbCommand.CommandText" /> property is interpreted.</summary>
	/// <returns>One of the <see cref="T:System.Data.CommandType" /> values. The default is Text.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(CommandType.Text)]
	[RefreshProperties(RefreshProperties.All)]
	public abstract CommandType CommandType { get; set; }

	/// <summary>Gets or sets the <see cref="T:System.Data.Common.DbConnection" /> used by this <see cref="T:System.Data.Common.DbCommand" />.</summary>
	/// <returns>The connection to the data source.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[DefaultValue(null)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DbConnection Connection
	{
		get
		{
			return DbConnection;
		}
		set
		{
			DbConnection = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.IDbConnection" /> used by this instance of the <see cref="T:System.Data.IDbCommand" />.</summary>
	/// <returns>The connection to the data source.</returns>
	IDbConnection IDbCommand.Connection
	{
		get
		{
			return DbConnection;
		}
		set
		{
			DbConnection = (DbConnection)value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.Common.DbConnection" /> used by this <see cref="T:System.Data.Common.DbCommand" />.</summary>
	/// <returns>The connection to the data source.</returns>
	protected abstract DbConnection DbConnection { get; set; }

	/// <summary>Gets the collection of <see cref="T:System.Data.Common.DbParameter" /> objects.</summary>
	/// <returns>The parameters of the SQL statement or stored procedure.</returns>
	protected abstract DbParameterCollection DbParameterCollection { get; }

	/// <summary>Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction" /> within which this <see cref="T:System.Data.Common.DbCommand" /> object executes.</summary>
	/// <returns>The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).</returns>
	protected abstract DbTransaction DbTransaction { get; set; }

	/// <summary>Gets or sets a value indicating whether the command object should be visible in a customized interface control.</summary>
	/// <returns>true, if the command object should be visible in a control; otherwise false. The default is true.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[DesignOnly(true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DefaultValue(true)]
	public abstract bool DesignTimeVisible { get; set; }

	/// <summary>Gets the collection of <see cref="T:System.Data.Common.DbParameter" /> objects. For more information on parameters, see Configuring Parameters and Parameter Data Types (ADO.NET).</summary>
	/// <returns>The parameters of the SQL statement or stored procedure.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DbParameterCollection Parameters => DbParameterCollection;

	/// <summary>Gets the <see cref="T:System.Data.IDataParameterCollection" />.</summary>
	/// <returns>The parameters of the SQL statement or stored procedure.</returns>
	IDataParameterCollection IDbCommand.Parameters => DbParameterCollection;

	/// <summary>Gets or sets the <see cref="T:System.Data.Common.DbTransaction" /> within which this <see cref="T:System.Data.Common.DbCommand" /> object executes.</summary>
	/// <returns>The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(null)]
	[Browsable(false)]
	public DbTransaction Transaction
	{
		get
		{
			return DbTransaction;
		}
		set
		{
			DbTransaction = value;
		}
	}

	/// <summary>Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction" /> within which this <see cref="T:System.Data.Common.DbCommand" /> object executes.</summary>
	/// <returns>The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).</returns>
	IDbTransaction IDbCommand.Transaction
	{
		get
		{
			return DbTransaction;
		}
		set
		{
			DbTransaction = (DbTransaction)value;
		}
	}

	/// <summary>Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow" /> when used by the Update method of a <see cref="T:System.Data.Common.DbDataAdapter" />.</summary>
	/// <returns>One of the <see cref="T:System.Data.UpdateRowSource" /> values. The default is Both unless the command is automatically generated. Then the default is None.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(UpdateRowSource.Both)]
	public abstract UpdateRowSource UpdatedRowSource { get; set; }

	/// <summary>Constructs an instance of the <see cref="T:System.Data.Common.DbCommand" /> object.</summary>
	protected DbCommand()
	{
	}

	internal void CancelIgnoreFailure()
	{
		try
		{
			Cancel();
		}
		catch (Exception)
		{
		}
	}

	/// <summary>Attempts to cancels the execution of a <see cref="T:System.Data.Common.DbCommand" />.</summary>
	/// <filterpriority>1</filterpriority>
	public abstract void Cancel();

	/// <summary>Creates a new instance of a <see cref="T:System.Data.Common.DbParameter" /> object.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbParameter" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public DbParameter CreateParameter()
	{
		return CreateDbParameter();
	}

	/// <summary>Creates a new instance of an <see cref="T:System.Data.IDbDataParameter" /> object.</summary>
	/// <returns>An IDbDataParameter object.</returns>
	IDbDataParameter IDbCommand.CreateParameter()
	{
		return CreateDbParameter();
	}

	/// <summary>Creates a new instance of a <see cref="T:System.Data.Common.DbParameter" /> object.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbParameter" /> object.</returns>
	protected abstract DbParameter CreateDbParameter();

	/// <summary>Executes the command text against the connection.</summary>
	/// <returns>A task representing the operation.</returns>
	/// <param name="behavior">An instance of <see cref="T:System.Data.CommandBehavior" />.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	protected abstract DbDataReader ExecuteDbDataReader(CommandBehavior behavior);

	/// <summary>Executes a SQL statement against a connection object.</summary>
	/// <returns>The number of rows affected.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract int ExecuteNonQuery();

	/// <summary>Executes the <see cref="P:System.Data.Common.DbCommand.CommandText" /> against the <see cref="P:System.Data.Common.DbCommand.Connection" />, and returns an <see cref="T:System.Data.Common.DbDataReader" />.</summary>
	/// <returns>A <see cref="T:System.Data.Common.DbDataReader" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public DbDataReader ExecuteReader()
	{
		return ExecuteDbDataReader(CommandBehavior.Default);
	}

	/// <summary>Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the <see cref="P:System.Data.IDbCommand.Connection" /> and builds an <see cref="T:System.Data.IDataReader" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDataReader" /> object.</returns>
	IDataReader IDbCommand.ExecuteReader()
	{
		return ExecuteDbDataReader(CommandBehavior.Default);
	}

	/// <summary>Executes the <see cref="P:System.Data.Common.DbCommand.CommandText" /> against the <see cref="P:System.Data.Common.DbCommand.Connection" />, and returns an <see cref="T:System.Data.Common.DbDataReader" /> using one of the <see cref="T:System.Data.CommandBehavior" /> values. </summary>
	/// <returns>An <see cref="T:System.Data.Common.DbDataReader" /> object.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
	/// <filterpriority>1</filterpriority>
	public DbDataReader ExecuteReader(CommandBehavior behavior)
	{
		return ExecuteDbDataReader(behavior);
	}

	/// <summary>Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the <see cref="P:System.Data.IDbCommand.Connection" />, and builds an <see cref="T:System.Data.IDataReader" /> using one of the <see cref="T:System.Data.CommandBehavior" /> values.</summary>
	/// <returns>An <see cref="T:System.Data.IDataReader" /> object.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
	IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
	{
		return ExecuteDbDataReader(behavior);
	}

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" />, which executes a SQL statement against a connection object.Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteNonQueryAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public Task<int> ExecuteNonQueryAsync()
	{
		return ExecuteNonQueryAsync(CancellationToken.None);
	}

	/// <summary>This is the asynchronous version of <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" />. Providers should override with an appropriate implementation. The cancellation token may optionally be ignored.The default implementation invokes the synchronous <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellation token.  Exceptions thrown by <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery" /> will be communicated via the returned Task Exception property.Do not invoke other methods and properties of the DbCommand object until the returned Task is complete.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<int>();
		}
		CancellationTokenRegistration cancellationTokenRegistration = default(CancellationTokenRegistration);
		if (cancellationToken.CanBeCanceled)
		{
			cancellationTokenRegistration = cancellationToken.Register(delegate(object s)
			{
				((DbCommand)s).CancelIgnoreFailure();
			}, this);
		}
		try
		{
			return Task.FromResult(ExecuteNonQuery());
		}
		catch (Exception exception)
		{
			return Task.FromException<int>(exception);
		}
		finally
		{
			cancellationTokenRegistration.Dispose();
		}
	}

	/// <summary>An asynchronous version of <see cref="Overload:System.Data.Common.DbCommand.ExecuteReader" />, which executes the <see cref="P:System.Data.Common.DbCommand.CommandText" /> against the <see cref="P:System.Data.Common.DbCommand.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />.Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	public Task<DbDataReader> ExecuteReaderAsync()
	{
		return ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);
	}

	/// <summary>An asynchronous version of <see cref="Overload:System.Data.Common.DbCommand.ExecuteReader" />, which executes the <see cref="P:System.Data.Common.DbCommand.CommandText" /> against the <see cref="P:System.Data.Common.DbCommand.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />. This method propagates a notification that operations should be canceled.Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	public Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
	{
		return ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
	}

	/// <summary>An asynchronous version of <see cref="Overload:System.Data.Common.DbCommand.ExecuteReader" />, which executes the <see cref="P:System.Data.Common.DbCommand.CommandText" /> against the <see cref="P:System.Data.Common.DbCommand.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />.Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	public Task<DbDataReader> ExecuteReaderAsync(CommandBehavior behavior)
	{
		return ExecuteReaderAsync(behavior, CancellationToken.None);
	}

	/// <summary>Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	public Task<DbDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		return ExecuteDbDataReaderAsync(behavior, cancellationToken);
	}

	/// <summary>Providers should implement this method to provide a non-default implementation for <see cref="Overload:System.Data.Common.DbCommand.ExecuteReader" /> overloads.The default implementation invokes the synchronous <see cref="M:System.Data.Common.DbCommand.ExecuteReader" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellation token. Exceptions thrown by ExecuteReader will be communicated via the returned Task Exception property.This method accepts a cancellation token that can be used to request the operation to be cancelled early. Implementations may ignore this request.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="behavior">Options for statement execution and data retrieval.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	/// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
	protected virtual Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<DbDataReader>();
		}
		CancellationTokenRegistration cancellationTokenRegistration = default(CancellationTokenRegistration);
		if (cancellationToken.CanBeCanceled)
		{
			cancellationTokenRegistration = cancellationToken.Register(delegate(object s)
			{
				((DbCommand)s).CancelIgnoreFailure();
			}, this);
		}
		try
		{
			return Task.FromResult(ExecuteReader(behavior));
		}
		catch (Exception exception)
		{
			return Task.FromException<DbDataReader>(exception);
		}
		finally
		{
			cancellationTokenRegistration.Dispose();
		}
	}

	/// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbCommand.ExecuteScalar" />, which executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.Invokes <see cref="M:System.Data.Common.DbCommand.ExecuteScalarAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public Task<object> ExecuteScalarAsync()
	{
		return ExecuteScalarAsync(CancellationToken.None);
	}

	/// <summary>This is the asynchronous version of <see cref="M:System.Data.Common.DbCommand.ExecuteScalar" />. Providers should override with an appropriate implementation. The cancellation token may optionally be ignored.The default implementation invokes the synchronous <see cref="M:System.Data.Common.DbCommand.ExecuteScalar" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellation token. Exceptions thrown by ExecuteScalar will be communicated via the returned Task Exception property.Do not invoke other methods and properties of the DbCommand object until the returned Task is complete.</summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
	public virtual Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ADP.CreatedTaskWithCancellation<object>();
		}
		CancellationTokenRegistration cancellationTokenRegistration = default(CancellationTokenRegistration);
		if (cancellationToken.CanBeCanceled)
		{
			cancellationTokenRegistration = cancellationToken.Register(delegate(object s)
			{
				((DbCommand)s).CancelIgnoreFailure();
			}, this);
		}
		try
		{
			return Task.FromResult(ExecuteScalar());
		}
		catch (Exception exception)
		{
			return Task.FromException<object>(exception);
		}
		finally
		{
			cancellationTokenRegistration.Dispose();
		}
	}

	/// <summary>Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.</summary>
	/// <returns>The first column of the first row in the result set.</returns>
	/// <filterpriority>1</filterpriority>
	public abstract object ExecuteScalar();

	/// <summary>Creates a prepared (or compiled) version of the command on the data source.</summary>
	/// <filterpriority>1</filterpriority>
	public abstract void Prepare();

	public virtual Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return Task.FromCanceled(cancellationToken);
		}
		try
		{
			Prepare();
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
