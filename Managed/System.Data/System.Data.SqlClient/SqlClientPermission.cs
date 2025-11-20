using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient;

/// <summary>Enables the .NET Framework Data Provider for SQL Server to help make sure that a user has a security level sufficient to access a data source. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class SqlClientPermission : DBDataPermission
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlClientPermission" /> class.</summary>
	[Obsolete("SqlClientPermission() has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public SqlClientPermission()
		: this(PermissionState.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlClientPermission" /> class.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	public SqlClientPermission(PermissionState state)
		: base(state)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlClient.SqlClientPermission" /> class.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <param name="allowBlankPassword">Indicates whether a blank password is allowed. </param>
	[Obsolete("SqlClientPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public SqlClientPermission(PermissionState state, bool allowBlankPassword)
		: this(state)
	{
		base.AllowBlankPassword = allowBlankPassword;
	}

	private SqlClientPermission(SqlClientPermission permission)
		: base(permission)
	{
	}

	internal SqlClientPermission(SqlClientPermissionAttribute permissionAttribute)
		: base(permissionAttribute)
	{
	}

	internal SqlClientPermission(SqlConnectionString constr)
		: base(constr)
	{
		if (constr == null || constr.IsEmpty)
		{
			base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
		}
	}

	/// <summary>Adds a new connection string and a set of restricted keywords to the <see cref="T:System.Data.SqlClient.SqlClientPermission" /> object.</summary>
	/// <param name="connectionString">The connection string.</param>
	/// <param name="restrictions">The key restrictions.</param>
	/// <param name="behavior">One of the <see cref="T:System.Data.KeyRestrictionBehavior" /> enumerations.</param>
	/// <filterpriority>2</filterpriority>
	public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
	{
		DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, SqlConnectionString.GetParseSynonyms(), useOdbcRules: false);
		AddPermissionEntry(entry);
	}

	/// <summary>Returns the <see cref="T:System.Data.SqlClient.SqlClientPermission" /> as an <see cref="T:System.Security.IPermission" />.</summary>
	/// <returns>A copy of the current permission object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override IPermission Copy()
	{
		return new SqlClientPermission(this);
	}
}
