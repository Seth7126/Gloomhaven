using System.ComponentModel;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb;

/// <summary>Associates a security action with a custom security attribute.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute
{
	private string _providers;

	/// <summary>Gets or sets a comma-delimited string that contains a list of supported providers.</summary>
	/// <returns>A comma-delimited list of providers allowed by the security policy.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Provider property has been deprecated.  Use the Add method.  http://go.microsoft.com/fwlink/?linkid=14202")]
	public string Provider
	{
		get
		{
			string providers = _providers;
			if (providers == null)
			{
				return ADP.StrEmpty;
			}
			return providers;
		}
		set
		{
			_providers = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbPermissionAttribute" /> class.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values representing an action that can be performed by using declarative security. </param>
	public OleDbPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Returns an <see cref="T:System.Data.OleDb.OleDbPermission" /> object that is configured according to the attribute properties.</summary>
	/// <returns>An <see cref="T:System.Data.OleDb.OleDbPermission" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		return new OleDbPermission(this);
	}
}
