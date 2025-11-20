using System.Data.Common;
using System.Data.ProviderBase;
using System.IO;
using System.Reflection;

namespace System.Data.Odbc;

internal sealed class OdbcConnectionFactory : DbConnectionFactory
{
	public static readonly OdbcConnectionFactory SingletonInstance = new OdbcConnectionFactory();

	public override DbProviderFactory ProviderFactory => OdbcFactory.Instance;

	private OdbcConnectionFactory()
	{
	}

	protected override DbConnectionInternal CreateConnection(DbConnectionOptions options, DbConnectionPoolKey poolKey, object poolGroupProviderInfo, DbConnectionPool pool, DbConnection owningObject)
	{
		return new OdbcConnectionOpen(owningObject as OdbcConnection, options as OdbcConnectionString);
	}

	protected override DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous)
	{
		return new OdbcConnectionString(connectionString, previous != null);
	}

	protected override DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions connectionOptions)
	{
		return null;
	}

	internal override DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
	{
		return new OdbcConnectionPoolGroupProviderInfo();
	}

	protected override DbMetaDataFactory CreateMetaDataFactory(DbConnectionInternal internalConnection, out bool cacheMetaDataFactory)
	{
		cacheMetaDataFactory = false;
		OdbcConnection outerConnection = ((OdbcConnectionOpen)internalConnection).OuterConnection;
		string infoStringUnhandled = outerConnection.GetInfoStringUnhandled(ODBC32.SQL_INFO.DRIVER_NAME);
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("System.Data.Odbc.OdbcMetaData.xml");
		cacheMetaDataFactory = true;
		string infoStringUnhandled2 = outerConnection.GetInfoStringUnhandled(ODBC32.SQL_INFO.DBMS_VER);
		return new OdbcMetaDataFactory(manifestResourceStream, infoStringUnhandled2, infoStringUnhandled2, outerConnection);
	}

	internal override DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
	{
		if (connection is OdbcConnection odbcConnection)
		{
			return odbcConnection.PoolGroup;
		}
		return null;
	}

	internal override DbConnectionInternal GetInnerConnection(DbConnection connection)
	{
		if (connection is OdbcConnection odbcConnection)
		{
			return odbcConnection.InnerConnection;
		}
		return null;
	}

	internal override void PermissionDemand(DbConnection outerConnection)
	{
		if (outerConnection is OdbcConnection odbcConnection)
		{
			odbcConnection.PermissionDemand();
		}
	}

	internal override void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup)
	{
		if (outerConnection is OdbcConnection odbcConnection)
		{
			odbcConnection.PoolGroup = poolGroup;
		}
	}

	internal override void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to)
	{
		if (owningObject is OdbcConnection odbcConnection)
		{
			odbcConnection.SetInnerConnectionEvent(to);
		}
	}

	internal override bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from)
	{
		if (owningObject is OdbcConnection odbcConnection)
		{
			return odbcConnection.SetInnerConnectionFrom(to, from);
		}
		return false;
	}

	internal override void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to)
	{
		if (owningObject is OdbcConnection odbcConnection)
		{
			odbcConnection.SetInnerConnectionTo(to);
		}
	}
}
