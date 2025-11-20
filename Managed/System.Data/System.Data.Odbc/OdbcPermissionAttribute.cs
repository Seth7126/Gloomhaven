using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Odbc;

/// <summary>Associates a security action with a custom security attribute.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class OdbcPermissionAttribute : DBDataPermissionAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Odbc.OdbcPermissionAttribute" /> class with one of the <see cref="T:System.Security.Permissions.SecurityAction" /> values.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values representing an action that can be performed by using declarative security. </param>
	public OdbcPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Returns an <see cref="T:System.Data.Odbc.OdbcPermission" /> object that is configured according to the attribute properties.</summary>
	/// <returns>An <see cref="T:System.Data.Odbc.OdbcPermission" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		return new OdbcPermission(this);
	}
}
