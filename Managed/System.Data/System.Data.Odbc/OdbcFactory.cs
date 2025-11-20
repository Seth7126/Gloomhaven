using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc;

/// <summary>Represents a set of methods for creating instances of the ODBC provider's implementation of the data source classes.</summary>
/// <filterpriority>2</filterpriority>
public sealed class OdbcFactory : DbProviderFactory
{
	/// <summary>Gets an instance of the <see cref="T:System.Data.Odbc.OdbcFactory" />, which can be used to retrieve strongly-typed data objects.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly OdbcFactory Instance = new OdbcFactory();

	private OdbcFactory()
	{
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbCommand" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbCommand" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbCommand CreateCommand()
	{
		return new OdbcCommand();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbCommandBuilder" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbCommandBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbCommandBuilder CreateCommandBuilder()
	{
		return new OdbcCommandBuilder();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbConnection" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbConnection" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DbConnection CreateConnection()
	{
		return new OdbcConnection();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		return new OdbcConnectionStringBuilder();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbDataAdapter" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbDataAdapter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbDataAdapter CreateDataAdapter()
	{
		return new OdbcDataAdapter();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Data.Common.DbParameter" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Data.Common.DbParameter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbParameter CreateParameter()
	{
		return new OdbcParameter();
	}

	/// <summary>Returns a strongly-typed <see cref="T:System.Security.CodeAccessPermission" /> instance.</summary>
	/// <returns>A new strongly-typed instance of <see cref="T:System.Security.CodeAccessPermission" />. </returns>
	/// <param name="state">A member of the <see cref="T:System.Security.Permissions.PermissionState" /> enumeration.</param>
	/// <filterpriority>2</filterpriority>
	public override CodeAccessPermission CreatePermission(PermissionState state)
	{
		return new OdbcPermission(state);
	}
}
