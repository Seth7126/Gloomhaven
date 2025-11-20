using System.Security;
using System.Security.Permissions;

namespace System.Data.Common;

/// <summary>Represents a set of methods for creating instances of a provider's implementation of the data source classes.</summary>
/// <filterpriority>2</filterpriority>
public abstract class DbProviderFactory
{
	private bool? _canCreateDataAdapter;

	private bool? _canCreateCommandBuilder;

	/// <summary>Specifies whether the specific <see cref="T:System.Data.Common.DbProviderFactory" /> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator" /> class.</summary>
	/// <returns>true if the instance of the <see cref="T:System.Data.Common.DbProviderFactory" /> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator" /> class; otherwise false.</returns>
	public virtual bool CanCreateDataSourceEnumerator => false;

	public virtual bool CanCreateDataAdapter
	{
		get
		{
			if (!_canCreateDataAdapter.HasValue)
			{
				using DbDataAdapter dbDataAdapter = CreateDataAdapter();
				_canCreateDataAdapter = dbDataAdapter != null;
			}
			return _canCreateDataAdapter.Value;
		}
	}

	public virtual bool CanCreateCommandBuilder
	{
		get
		{
			if (!_canCreateCommandBuilder.HasValue)
			{
				using DbCommandBuilder dbCommandBuilder = CreateCommandBuilder();
				_canCreateCommandBuilder = dbCommandBuilder != null;
			}
			return _canCreateCommandBuilder.Value;
		}
	}

	/// <summary>Returns a new instance of the provider's class that implements the provider's version of the <see cref="T:System.Security.CodeAccessPermission" /> class.</summary>
	/// <returns>A <see cref="T:System.Security.CodeAccessPermission" /> object for the specified <see cref="T:System.Security.Permissions.PermissionState" />.</returns>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	/// <filterpriority>2</filterpriority>
	public virtual CodeAccessPermission CreatePermission(PermissionState state)
	{
		return null;
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Data.Common.DbProviderFactory" /> class.</summary>
	protected DbProviderFactory()
	{
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbCommand" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbCommand" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbCommand CreateCommand()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbCommandBuilder" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbCommandBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbCommandBuilder CreateCommandBuilder()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbConnection" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbConnection CreateConnection()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataAdapter" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbDataAdapter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbDataAdapter CreateDataAdapter()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbParameter" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbParameter CreateParameter()
	{
		return null;
	}

	/// <summary>Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataSourceEnumerator" /> class.</summary>
	/// <returns>A new instance of <see cref="T:System.Data.Common.DbDataSourceEnumerator" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual DbDataSourceEnumerator CreateDataSourceEnumerator()
	{
		return null;
	}
}
