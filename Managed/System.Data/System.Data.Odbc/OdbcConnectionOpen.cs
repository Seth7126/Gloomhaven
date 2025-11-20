using System.Data.Common;
using System.Data.ProviderBase;
using System.Transactions;

namespace System.Data.Odbc;

internal sealed class OdbcConnectionOpen : DbConnectionInternal
{
	internal OdbcConnection OuterConnection
	{
		get
		{
			OdbcConnection odbcConnection = (OdbcConnection)base.Owner;
			if (odbcConnection == null)
			{
				throw ODBC.OpenConnectionNoOwner();
			}
			return odbcConnection;
		}
	}

	public override string ServerVersion => OuterConnection.Open_GetServerVersion();

	internal OdbcConnectionOpen(OdbcConnection outerConnection, OdbcConnectionString connectionOptions)
	{
		OdbcEnvironmentHandle globalEnvironmentHandle = OdbcEnvironment.GetGlobalEnvironmentHandle();
		outerConnection.ConnectionHandle = new OdbcConnectionHandle(outerConnection, connectionOptions, globalEnvironmentHandle);
	}

	protected override void Activate(Transaction transaction)
	{
	}

	public override DbTransaction BeginTransaction(IsolationLevel isolevel)
	{
		return BeginOdbcTransaction(isolevel);
	}

	internal OdbcTransaction BeginOdbcTransaction(IsolationLevel isolevel)
	{
		return OuterConnection.Open_BeginTransaction(isolevel);
	}

	public override void ChangeDatabase(string value)
	{
		OuterConnection.Open_ChangeDatabase(value);
	}

	protected override DbReferenceCollection CreateReferenceCollection()
	{
		return new OdbcReferenceCollection();
	}

	protected override void Deactivate()
	{
		NotifyWeakReference(0);
	}

	public override void EnlistTransaction(Transaction transaction)
	{
	}
}
