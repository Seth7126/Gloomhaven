using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Transactions;

namespace System.Data.Odbc;

internal sealed class OdbcConnectionHandle : OdbcHandle
{
	private enum HandleState
	{
		Allocated,
		Connected,
		Transacted,
		TransactionInProgress
	}

	private HandleState _handleState;

	internal OdbcConnectionHandle(OdbcConnection connection, OdbcConnectionString constr, OdbcEnvironmentHandle environmentHandle)
		: base(ODBC32.SQL_HANDLE.DBC, environmentHandle)
	{
		if (connection == null)
		{
			throw ADP.ArgumentNull("connection");
		}
		if (constr == null)
		{
			throw ADP.ArgumentNull("constr");
		}
		int connectionTimeout = connection.ConnectionTimeout;
		ODBC32.RetCode retCode = SetConnectionAttribute2(ODBC32.SQL_ATTR.LOGIN_TIMEOUT, (IntPtr)connectionTimeout, -5);
		string connectionString = constr.UsersConnectionString(hidePassword: false);
		retCode = Connect(connectionString);
		connection.HandleError(this, retCode);
	}

	private ODBC32.RetCode AutoCommitOff()
	{
		RuntimeHelpers.PrepareConstrainedRegions();
		ODBC32.RetCode retCode;
		try
		{
		}
		finally
		{
			retCode = global::Interop.Odbc.SQLSetConnectAttrW(this, ODBC32.SQL_ATTR.AUTOCOMMIT, ODBC32.SQL_AUTOCOMMIT_OFF, -5);
			if ((uint)retCode <= 1u)
			{
				_handleState = HandleState.Transacted;
			}
		}
		ODBC.TraceODBC(3, "SQLSetConnectAttrW", retCode);
		return retCode;
	}

	internal ODBC32.RetCode BeginTransaction(ref IsolationLevel isolevel)
	{
		ODBC32.RetCode retCode = ODBC32.RetCode.SUCCESS;
		if (IsolationLevel.Unspecified != isolevel)
		{
			ODBC32.SQL_TRANSACTION sQL_TRANSACTION;
			ODBC32.SQL_ATTR attribute;
			switch (isolevel)
			{
			case IsolationLevel.ReadUncommitted:
				sQL_TRANSACTION = ODBC32.SQL_TRANSACTION.READ_UNCOMMITTED;
				attribute = ODBC32.SQL_ATTR.TXN_ISOLATION;
				break;
			case IsolationLevel.ReadCommitted:
				sQL_TRANSACTION = ODBC32.SQL_TRANSACTION.READ_COMMITTED;
				attribute = ODBC32.SQL_ATTR.TXN_ISOLATION;
				break;
			case IsolationLevel.RepeatableRead:
				sQL_TRANSACTION = ODBC32.SQL_TRANSACTION.REPEATABLE_READ;
				attribute = ODBC32.SQL_ATTR.TXN_ISOLATION;
				break;
			case IsolationLevel.Serializable:
				sQL_TRANSACTION = ODBC32.SQL_TRANSACTION.SERIALIZABLE;
				attribute = ODBC32.SQL_ATTR.TXN_ISOLATION;
				break;
			case IsolationLevel.Snapshot:
				sQL_TRANSACTION = ODBC32.SQL_TRANSACTION.SNAPSHOT;
				attribute = ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION;
				break;
			case IsolationLevel.Chaos:
				throw ODBC.NotSupportedIsolationLevel(isolevel);
			default:
				throw ADP.InvalidIsolationLevel(isolevel);
			}
			retCode = SetConnectionAttribute2(attribute, (IntPtr)(int)sQL_TRANSACTION, -6);
			if (ODBC32.RetCode.SUCCESS_WITH_INFO == retCode)
			{
				isolevel = IsolationLevel.Unspecified;
			}
		}
		if ((uint)retCode <= 1u)
		{
			retCode = AutoCommitOff();
			_handleState = HandleState.TransactionInProgress;
		}
		return retCode;
	}

	internal ODBC32.RetCode CompleteTransaction(short transactionOperation)
	{
		bool success = false;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			DangerousAddRef(ref success);
			return CompleteTransaction(transactionOperation, handle);
		}
		finally
		{
			if (success)
			{
				DangerousRelease();
			}
		}
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private ODBC32.RetCode CompleteTransaction(short transactionOperation, IntPtr handle)
	{
		ODBC32.RetCode retCode = ODBC32.RetCode.SUCCESS;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
		}
		finally
		{
			if (HandleState.TransactionInProgress == _handleState)
			{
				retCode = global::Interop.Odbc.SQLEndTran(base.HandleType, handle, transactionOperation);
				if (retCode == ODBC32.RetCode.SUCCESS || ODBC32.RetCode.SUCCESS_WITH_INFO == retCode)
				{
					_handleState = HandleState.Transacted;
				}
			}
			if (HandleState.Transacted == _handleState)
			{
				retCode = global::Interop.Odbc.SQLSetConnectAttrW(handle, ODBC32.SQL_ATTR.AUTOCOMMIT, ODBC32.SQL_AUTOCOMMIT_ON, -5);
				_handleState = HandleState.Connected;
			}
		}
		return retCode;
	}

	private ODBC32.RetCode Connect(string connectionString)
	{
		RuntimeHelpers.PrepareConstrainedRegions();
		ODBC32.RetCode retCode;
		try
		{
		}
		finally
		{
			retCode = global::Interop.Odbc.SQLDriverConnectW(this, ADP.PtrZero, connectionString, -3, ADP.PtrZero, 0, out var _, 0);
			if ((uint)retCode <= 1u)
			{
				_handleState = HandleState.Connected;
			}
		}
		ODBC.TraceODBC(3, "SQLDriverConnectW", retCode);
		return retCode;
	}

	protected override bool ReleaseHandle()
	{
		CompleteTransaction(1, handle);
		if (HandleState.Connected == _handleState || HandleState.TransactionInProgress == _handleState)
		{
			global::Interop.Odbc.SQLDisconnect(handle);
			_handleState = HandleState.Allocated;
		}
		return base.ReleaseHandle();
	}

	internal ODBC32.RetCode GetConnectionAttribute(ODBC32.SQL_ATTR attribute, byte[] buffer, out int cbActual)
	{
		return global::Interop.Odbc.SQLGetConnectAttrW(this, attribute, buffer, buffer.Length, out cbActual);
	}

	internal ODBC32.RetCode GetFunctions(ODBC32.SQL_API fFunction, out short fExists)
	{
		ODBC32.RetCode retCode = global::Interop.Odbc.SQLGetFunctions(this, fFunction, out fExists);
		ODBC.TraceODBC(3, "SQLGetFunctions", retCode);
		return retCode;
	}

	internal ODBC32.RetCode GetInfo2(ODBC32.SQL_INFO info, byte[] buffer, out short cbActual)
	{
		return global::Interop.Odbc.SQLGetInfoW(this, info, buffer, checked((short)buffer.Length), out cbActual);
	}

	internal ODBC32.RetCode GetInfo1(ODBC32.SQL_INFO info, byte[] buffer)
	{
		return global::Interop.Odbc.SQLGetInfoW(this, info, buffer, checked((short)buffer.Length), ADP.PtrZero);
	}

	internal ODBC32.RetCode SetConnectionAttribute2(ODBC32.SQL_ATTR attribute, IntPtr value, int length)
	{
		ODBC32.RetCode retCode = global::Interop.Odbc.SQLSetConnectAttrW(this, attribute, value, length);
		ODBC.TraceODBC(3, "SQLSetConnectAttrW", retCode);
		return retCode;
	}

	internal ODBC32.RetCode SetConnectionAttribute3(ODBC32.SQL_ATTR attribute, string buffer, int length)
	{
		return global::Interop.Odbc.SQLSetConnectAttrW(this, attribute, buffer, length);
	}

	internal ODBC32.RetCode SetConnectionAttribute4(ODBC32.SQL_ATTR attribute, IDtcTransaction transaction, int length)
	{
		ODBC32.RetCode retCode = global::Interop.Odbc.SQLSetConnectAttrW(this, attribute, transaction, length);
		ODBC.TraceODBC(3, "SQLSetConnectAttrW", retCode);
		return retCode;
	}
}
