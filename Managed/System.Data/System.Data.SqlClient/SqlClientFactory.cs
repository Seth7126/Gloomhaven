using System.Data.Common;
using System.Data.Sql;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Data.SqlClient;

/// <summary>Represents a set of methods for creating instances of the <see cref="N:System.Data.SqlClient" /> provider's implementation of the data source classes.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SqlClientFactory : DbProviderFactory, IServiceProvider
{
	/// <summary>Gets an instance of the <see cref="T:System.Data.SqlClient.SqlClientFactory" />. This can be used to retrieve strongly typed data objects.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly SqlClientFactory Instance = new SqlClientFactory();

	/// <summary>Returns true if a <see cref="T:System.Data.Sql.SqlDataSourceEnumerator" /> can be created; otherwise false .</summary>
	/// <returns>true if a <see cref="T:System.Data.Sql.SqlDataSourceEnumerator" /> can be created; otherwise false.</returns>
	public override bool CanCreateDataSourceEnumerator => true;

	private SqlClientFactory()
	{
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbCommand" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbCommand" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbCommand CreateCommand()
	{
		return new SqlCommand();
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbCommandBuilder" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbCommandBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DbCommandBuilder CreateCommandBuilder()
	{
		return new SqlCommandBuilder();
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbConnection" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbConnection" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override DbConnection CreateConnection()
	{
		return new SqlConnection();
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		return new SqlConnectionStringBuilder();
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbDataAdapter" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbDataAdapter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbDataAdapter CreateDataAdapter()
	{
		return new SqlDataAdapter();
	}

	/// <summary>Returns a strongly typed <see cref="T:System.Data.Common.DbParameter" /> instance.</summary>
	/// <returns>A new strongly typed instance of <see cref="T:System.Data.Common.DbParameter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbParameter CreateParameter()
	{
		return new SqlParameter();
	}

	/// <summary>Returns a new <see cref="T:System.Data.Sql.SqlDataSourceEnumerator" />.</summary>
	/// <returns>A new data source enumerator.</returns>
	/// <filterpriority>2</filterpriority>
	public override DbDataSourceEnumerator CreateDataSourceEnumerator()
	{
		return SqlDataSourceEnumerator.Instance;
	}

	/// <summary>Returns a new <see cref="T:System.Security.CodeAccessPermission" />.</summary>
	/// <returns>A strongly typed instance of <see cref="T:System.Security.CodeAccessPermission" />.</returns>
	/// <param name="state">A member of the <see cref="T:System.Security.Permissions.PermissionState" /> enumeration.</param>
	/// <filterpriority>2</filterpriority>
	public override CodeAccessPermission CreatePermission(PermissionState state)
	{
		return new SqlClientPermission(state);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IServiceProvider.GetService(System.Type)" />.</summary>
	/// <returns>A service object.</returns>
	/// <param name="serviceType">An object that specifies the type of service object to get. </param>
	object IServiceProvider.GetService(Type serviceType)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
