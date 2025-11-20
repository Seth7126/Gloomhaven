using System.Data.Common;
using Unity;

namespace System.Data.SqlClient;

/// <summary>Represents a Transact-SQL transaction to be made in a SQL Server database. This class cannot be inherited. </summary>
/// <filterpriority>1</filterpriority>
public sealed class SqlTransaction : DbTransaction
{
	private static readonly DiagnosticListener s_diagnosticListener = new DiagnosticListener("SqlClientDiagnosticListener");

	internal readonly IsolationLevel _isolationLevel;

	private SqlInternalTransaction _internalTransaction;

	private SqlConnection _connection;

	private bool _isFromAPI;

	/// <summary>Gets the <see cref="T:System.Data.SqlClient.SqlConnection" /> object associated with the transaction, or null if the transaction is no longer valid.</summary>
	/// <returns>The <see cref="T:System.Data.SqlClient.SqlConnection" /> object associated with the transaction.</returns>
	/// <filterpriority>2</filterpriority>
	public new SqlConnection Connection
	{
		get
		{
			if (IsZombied)
			{
				return null;
			}
			return _connection;
		}
	}

	protected override DbConnection DbConnection => Connection;

	internal SqlInternalTransaction InternalTransaction => _internalTransaction;

	/// <summary>Specifies the <see cref="T:System.Data.IsolationLevel" /> for this transaction.</summary>
	/// <returns>The <see cref="T:System.Data.IsolationLevel" /> for this transaction. The default is ReadCommitted.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override IsolationLevel IsolationLevel
	{
		get
		{
			ZombieCheck();
			return _isolationLevel;
		}
	}

	private bool IsYukonPartialZombie
	{
		get
		{
			if (_internalTransaction != null)
			{
				return _internalTransaction.IsCompleted;
			}
			return false;
		}
	}

	internal bool IsZombied
	{
		get
		{
			if (_internalTransaction != null)
			{
				return _internalTransaction.IsCompleted;
			}
			return true;
		}
	}

	internal SqlStatistics Statistics
	{
		get
		{
			if (_connection != null && _connection.StatisticsEnabled)
			{
				return _connection.Statistics;
			}
			return null;
		}
	}

	internal SqlTransaction(SqlInternalConnection internalConnection, SqlConnection con, IsolationLevel iso, SqlInternalTransaction internalTransaction)
	{
		_isolationLevel = IsolationLevel.ReadCommitted;
		base._002Ector();
		_isolationLevel = iso;
		_connection = con;
		if (internalTransaction == null)
		{
			_internalTransaction = new SqlInternalTransaction(internalConnection, TransactionType.LocalFromAPI, this);
			return;
		}
		_internalTransaction = internalTransaction;
		_internalTransaction.InitParent(this);
	}

	/// <summary>Commits the database transaction.</summary>
	/// <exception cref="T:System.Exception">An error occurred while trying to commit the transaction. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy, ControlAppDomain" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Commit()
	{
		Exception ex = null;
		Guid operationId = s_diagnosticListener.WriteTransactionCommitBefore(_isolationLevel, _connection, "Commit");
		ZombieCheck();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			_isFromAPI = true;
			_internalTransaction.Commit();
		}
		catch (Exception ex2)
		{
			ex = ex2;
			throw;
		}
		finally
		{
			if (ex != null)
			{
				s_diagnosticListener.WriteTransactionCommitError(operationId, _isolationLevel, _connection, ex, "Commit");
			}
			else
			{
				s_diagnosticListener.WriteTransactionCommitAfter(operationId, _isolationLevel, _connection, "Commit");
			}
			_isFromAPI = false;
			SqlStatistics.StopTimer(statistics);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !IsZombied && !IsYukonPartialZombie)
		{
			_internalTransaction.Dispose();
		}
		base.Dispose(disposing);
	}

	/// <summary>Rolls back a transaction from a pending state.</summary>
	/// <exception cref="T:System.Exception">An error occurred while trying to commit the transaction. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy, ControlAppDomain" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Data.SqlClient.SqlClientPermission, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void Rollback()
	{
		Exception ex = null;
		Guid operationId = s_diagnosticListener.WriteTransactionRollbackBefore(_isolationLevel, _connection, null, "Rollback");
		if (IsYukonPartialZombie)
		{
			_internalTransaction = null;
			return;
		}
		ZombieCheck();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			_isFromAPI = true;
			_internalTransaction.Rollback();
		}
		catch (Exception ex2)
		{
			ex = ex2;
			throw;
		}
		finally
		{
			if (ex != null)
			{
				s_diagnosticListener.WriteTransactionRollbackError(operationId, _isolationLevel, _connection, null, ex, "Rollback");
			}
			else
			{
				s_diagnosticListener.WriteTransactionRollbackAfter(operationId, _isolationLevel, _connection, null, "Rollback");
			}
			_isFromAPI = false;
			SqlStatistics.StopTimer(statistics);
		}
	}

	/// <summary>Rolls back a transaction from a pending state, and specifies the transaction or savepoint name.</summary>
	/// <param name="transactionName">The name of the transaction to roll back, or the savepoint to which to roll back. </param>
	/// <exception cref="T:System.ArgumentException">No transaction name was specified. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	public void Rollback(string transactionName)
	{
		Exception ex = null;
		Guid operationId = s_diagnosticListener.WriteTransactionRollbackBefore(_isolationLevel, _connection, transactionName, "Rollback");
		ZombieCheck();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			_isFromAPI = true;
			_internalTransaction.Rollback(transactionName);
		}
		catch (Exception ex2)
		{
			ex = ex2;
			throw;
		}
		finally
		{
			if (ex != null)
			{
				s_diagnosticListener.WriteTransactionRollbackError(operationId, _isolationLevel, _connection, transactionName, ex, "Rollback");
			}
			else
			{
				s_diagnosticListener.WriteTransactionRollbackAfter(operationId, _isolationLevel, _connection, transactionName, "Rollback");
			}
			_isFromAPI = false;
			SqlStatistics.StopTimer(statistics);
		}
	}

	/// <summary>Creates a savepoint in the transaction that can be used to roll back a part of the transaction, and specifies the savepoint name.</summary>
	/// <param name="savePointName">The name of the savepoint. </param>
	/// <exception cref="T:System.Exception">An error occurred while trying to commit the transaction. </exception>
	/// <exception cref="T:System.InvalidOperationException">The transaction has already been committed or rolled back.-or- The connection is broken. </exception>
	/// <filterpriority>2</filterpriority>
	public void Save(string savePointName)
	{
		ZombieCheck();
		SqlStatistics statistics = null;
		try
		{
			statistics = SqlStatistics.StartTimer(Statistics);
			_internalTransaction.Save(savePointName);
		}
		finally
		{
			SqlStatistics.StopTimer(statistics);
		}
	}

	internal void Zombie()
	{
		if (!(_connection.InnerConnection is SqlInternalConnection) || _isFromAPI)
		{
			_internalTransaction = null;
		}
	}

	private void ZombieCheck()
	{
		if (IsZombied)
		{
			if (IsYukonPartialZombie)
			{
				_internalTransaction = null;
			}
			throw ADP.TransactionZombied(this);
		}
	}

	internal SqlTransaction()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
