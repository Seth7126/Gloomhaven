using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc;

/// <summary>Enables the .NET Framework Data Provider for ODBC to help make sure that a user has a security level sufficient to access an ODBC data source. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class OdbcPermission : DBDataPermission
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcPermission" /> class.</summary>
	[Obsolete("OdbcPermission() has been deprecated.  Use the OdbcPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public OdbcPermission()
		: this(PermissionState.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcPermission" /> class with one of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	public OdbcPermission(PermissionState state)
		: base(state)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcPermission" /> class.</summary>
	/// <param name="state">One of the System.Security.Permissions.PermissionState values. </param>
	/// <param name="allowBlankPassword">Indicates whether a blank password is allowed. </param>
	[Obsolete("OdbcPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the OdbcPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public OdbcPermission(PermissionState state, bool allowBlankPassword)
		: this(state)
	{
		base.AllowBlankPassword = allowBlankPassword;
	}

	private OdbcPermission(OdbcPermission permission)
		: base(permission)
	{
	}

	internal OdbcPermission(OdbcPermissionAttribute permissionAttribute)
		: base(permissionAttribute)
	{
	}

	internal OdbcPermission(OdbcConnectionString constr)
		: base(constr)
	{
		if (constr == null || constr.IsEmpty)
		{
			base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
		}
	}

	/// <summary>Adds access for the specified connection string to the existing state of the permission.</summary>
	/// <param name="connectionString">A permitted connection string. </param>
	/// <param name="restrictions">String that identifies connection string parameters that are allowed or disallowed. </param>
	/// <param name="behavior">One of the <see cref="T:System.Data.KeyRestrictionBehavior" /> values. </param>
	/// <filterpriority>2</filterpriority>
	public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
	{
		DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, null, useOdbcRules: true);
		AddPermissionEntry(entry);
	}

	/// <summary>Returns the <see cref="T:System.Data.Odbc.OdbcPermission" /> as an <see cref="T:System.Security.IPermission" />.</summary>
	/// <returns>A copy of the current permission object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override IPermission Copy()
	{
		return new OdbcPermission(this);
	}
}
