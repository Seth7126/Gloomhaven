using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Represents an SQL statement or stored procedure to execute against a data source.</summary>
/// <filterpriority>1</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbCommand : DbCommand, IDbCommand, IDisposable, ICloneable
{
	/// <summary>Gets or sets the SQL statement or stored procedure to execute at the data source.</summary>
	/// <returns>The SQL statement or stored procedure to execute. The default value is an empty string.</returns>
	/// <filterpriority>1</filterpriority>
	public override string CommandText
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the wait time before terminating an attempt to execute a command and generating an error.</summary>
	/// <returns>The time (in seconds) to wait for the command to execute. The default is 30 seconds.</returns>
	public override int CommandTimeout
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets a value that indicates how the <see cref="P:System.Data.OleDb.OleDbCommand.CommandText" /> property is interpreted.</summary>
	/// <returns>One of the <see cref="P:System.Data.OleDb.OleDbCommand.CommandType" /> values. The default is Text.</returns>
	/// <exception cref="T:System.ArgumentException">The value was not a valid <see cref="P:System.Data.OleDb.OleDbCommand.CommandType" />.</exception>
	public override CommandType CommandType
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.OleDb.OleDbConnection" /> used by this instance of the <see cref="T:System.Data.OleDb.OleDbCommand" />.</summary>
	/// <returns>The connection to a data source. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.OleDb.OleDbCommand.Connection" /> property was changed while a transaction was in progress. </exception>
	/// <filterpriority>1</filterpriority>
	public new OleDbConnection Connection
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	protected override DbConnection DbConnection
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	protected override DbParameterCollection DbParameterCollection
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	protected override DbTransaction DbTransaction
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets a value that indicates whether the command object should be visible in a customized Windows Forms Designer control.</summary>
	/// <returns>A value that indicates whether the command object should be visible in a control. The default is true.</returns>
	public override bool DesignTimeVisible
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets the <see cref="T:System.Data.OleDb.OleDbParameterCollection" />.</summary>
	/// <returns>The parameters of the SQL statement or stored procedure. The default is an empty collection.</returns>
	/// <filterpriority>1</filterpriority>
	public new OleDbParameterCollection Parameters
	{
		get
		{
			throw ADP.OleDb();
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.OleDb.OleDbTransaction" /> within which the <see cref="T:System.Data.OleDb.OleDbCommand" /> executes.</summary>
	/// <returns>The <see cref="T:System.Data.OleDb.OleDbTransaction" />. The default value is null.</returns>
	/// <filterpriority>2</filterpriority>
	public new OleDbTransaction Transaction
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow" /> when used by the Update method of the <see cref="T:System.Data.OleDb.OleDbDataAdapter" />.</summary>
	/// <returns>One of the <see cref="T:System.Data.UpdateRowSource" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">The value entered was not one of the <see cref="T:System.Data.UpdateRowSource" /> values.</exception>
	public override UpdateRowSource UpdatedRowSource
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbCommand" /> class.</summary>
	public OleDbCommand()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbCommand" /> class with the text of the query.</summary>
	/// <param name="cmdText">The text of the query. </param>
	public OleDbCommand(string cmdText)
	{
		throw ADP.OleDb();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbCommand" /> class with the text of the query and an <see cref="T:System.Data.OleDb.OleDbConnection" />.</summary>
	/// <param name="cmdText">The text of the query. </param>
	/// <param name="connection">An <see cref="T:System.Data.OleDb.OleDbConnection" /> that represents the connection to a data source. </param>
	public OleDbCommand(string cmdText, OleDbConnection connection)
	{
		throw ADP.OleDb();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbCommand" /> class with the text of the query, an <see cref="T:System.Data.OleDb.OleDbConnection" />, and the <see cref="P:System.Data.OleDb.OleDbCommand.Transaction" />.</summary>
	/// <param name="cmdText">The text of the query. </param>
	/// <param name="connection">An <see cref="T:System.Data.OleDb.OleDbConnection" /> that represents the connection to a data source. </param>
	/// <param name="transaction">The transaction in which the <see cref="T:System.Data.OleDb.OleDbCommand" /> executes. </param>
	public OleDbCommand(string cmdText, OleDbConnection connection, OleDbTransaction transaction)
	{
		throw ADP.OleDb();
	}

	/// <summary>Tries to cancel the execution of an <see cref="T:System.Data.OleDb.OleDbCommand" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Cancel()
	{
	}

	/// <summary>Creates a new <see cref="T:System.Data.OleDb.OleDbCommand" /> object that is a copy of the current instance.</summary>
	/// <returns>A new <see cref="T:System.Data.OleDb.OleDbCommand" /> object that is a copy of this instance.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public OleDbCommand Clone()
	{
		throw ADP.OleDb();
	}

	protected override DbParameter CreateDbParameter()
	{
		throw ADP.OleDb();
	}

	/// <summary>Creates a new instance of an <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbParameter" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public new OleDbParameter CreateParameter()
	{
		throw ADP.OleDb();
	}

	protected override void Dispose(bool disposing)
	{
		throw ADP.OleDb();
	}

	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		throw ADP.OleDb();
	}

	/// <summary>Executes an SQL statement against the <see cref="P:System.Data.OleDb.OleDbCommand.Connection" /> and returns the number of rows affected.</summary>
	/// <returns>The number of rows affected.</returns>
	/// <exception cref="T:System.InvalidOperationException">The connection does not exist.-or- The connection is not open.-or- Cannot execute a command within a transaction context that differs from the context in which the connection was originally enlisted. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeSubWindows" />
	///   <IPermission class="System.Data.OleDb.OleDbPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int ExecuteNonQuery()
	{
		throw ADP.OleDb();
	}

	/// <summary>Sends the <see cref="P:System.Data.OleDb.OleDbCommand.CommandText" /> to the <see cref="P:System.Data.OleDb.OleDbCommand.Connection" /> and builds an <see cref="T:System.Data.OleDb.OleDbDataReader" />.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbDataReader" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">Cannot execute a command within a transaction context that differs from the context in which the connection was originally enlisted. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeSubWindows" />
	///   <IPermission class="System.Data.OleDb.OleDbPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new OleDbDataReader ExecuteReader()
	{
		throw ADP.OleDb();
	}

	/// <summary>Sends the <see cref="P:System.Data.OleDb.OleDbCommand.CommandText" /> to the <see cref="P:System.Data.OleDb.OleDbCommand.Connection" />, and builds an <see cref="T:System.Data.OleDb.OleDbDataReader" /> using one of the <see cref="T:System.Data.CommandBehavior" /> values.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbDataReader" /> object.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values. </param>
	/// <exception cref="T:System.InvalidOperationException">Cannot execute a command within a transaction context that differs from the context in which the connection was originally enlisted. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeSubWindows" />
	///   <IPermission class="System.Data.OleDb.OleDbPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public new OleDbDataReader ExecuteReader(CommandBehavior behavior)
	{
		throw ADP.OleDb();
	}

	/// <summary>Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.</summary>
	/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
	/// <exception cref="T:System.InvalidOperationException">Cannot execute a command within a transaction context that differs from the context in which the connection was originally enlisted. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeSubWindows" />
	///   <IPermission class="System.Data.OleDb.OleDbPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ExecuteScalar()
	{
		throw ADP.OleDb();
	}

	/// <summary>Creates a prepared (or compiled) version of the command on the data source.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.OleDb.OleDbCommand.Connection" /> is not set.-or- The <see cref="P:System.Data.OleDb.OleDbCommand.Connection" /> is not open. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Prepare()
	{
		throw ADP.OleDb();
	}

	/// <summary>Resets the <see cref="P:System.Data.OleDb.OleDbCommand.CommandTimeout" /> property to the default value.</summary>
	public void ResetCommandTimeout()
	{
		throw ADP.OleDb();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Data.IDbCommand.ExecuteReader" />.</summary>
	/// <returns>An <see cref="T:System.Data.IDataReader" /> object.</returns>
	IDataReader IDbCommand.ExecuteReader()
	{
		throw ADP.OleDb();
	}

	/// <summary>Executes the <see cref="P:System.Data.IDbCommand.CommandText" /> against the <see cref="P:System.Data.IDbCommand.Connection" />, and builds an <see cref="T:System.Data.IDataReader" /> using one of the <see cref="T:System.Data.CommandBehavior" /> values.</summary>
	/// <returns>An <see cref="T:System.Data.IDataReader" /> built using one of the <see cref="T:System.Data.CommandBehavior" /> values.</returns>
	/// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
	IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
	{
		throw ADP.OleDb();
	}

	/// <summary>For a description of this member, see <see cref="M:System.ICloneable.Clone" />.</summary>
	/// <returns>A new <see cref="T:System.Object" /> that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		throw ADP.OleDb();
	}
}
