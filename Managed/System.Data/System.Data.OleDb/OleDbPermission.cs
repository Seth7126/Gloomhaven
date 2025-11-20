using System.ComponentModel;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb;

/// <summary>Enables the .NET Framework Data Provider for OLE DB to help make sure that a user has a security level sufficient to access an OLE DB data source.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class OleDbPermission : DBDataPermission
{
	private string[] _providerRestriction;

	private string _providers;

	/// <summary>This property has been marked as obsolete. Setting this property will have no effect.</summary>
	/// <returns>This property has been marked as obsolete. Setting this property will have no effect.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Provider property has been deprecated.  Use the Add method.  http://go.microsoft.com/fwlink/?linkid=14202")]
	public string Provider
	{
		get
		{
			string text = _providers;
			if (text == null)
			{
				string[] providerRestriction = _providerRestriction;
				if (providerRestriction != null && providerRestriction.Length != 0)
				{
					text = providerRestriction[0];
					for (int i = 1; i < providerRestriction.Length; i++)
					{
						text = text + ";" + providerRestriction[i];
					}
				}
			}
			if (text == null)
			{
				return ADP.StrEmpty;
			}
			return text;
		}
		set
		{
			string[] providerRestriction = null;
			if (!ADP.IsEmpty(value))
			{
				providerRestriction = value.Split(new char[1] { ';' });
				providerRestriction = DBConnectionString.RemoveDuplicates(providerRestriction);
			}
			_providerRestriction = providerRestriction;
			_providers = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbPermission" /> class.</summary>
	[Obsolete("OleDbPermission() has been deprecated.  Use the OleDbPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public OleDbPermission()
		: this(PermissionState.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbPermission" /> class.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	public OleDbPermission(PermissionState state)
		: base(state)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbPermission" /> class.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <param name="allowBlankPassword">Indicates whether a blank password is allowed. </param>
	[Obsolete("OleDbPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the OleDbPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	public OleDbPermission(PermissionState state, bool allowBlankPassword)
		: this(state)
	{
		base.AllowBlankPassword = allowBlankPassword;
	}

	private OleDbPermission(OleDbPermission permission)
		: base(permission)
	{
	}

	internal OleDbPermission(OleDbPermissionAttribute permissionAttribute)
		: base(permissionAttribute)
	{
	}

	internal OleDbPermission(OleDbConnectionString constr)
		: base(constr)
	{
		if (constr == null || constr.IsEmpty)
		{
			base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
		}
	}

	/// <summary>Returns the <see cref="T:System.Data.OleDb.OleDbPermission" /> as an <see cref="T:System.Security.IPermission" />.</summary>
	/// <returns>A copy of the current permission object.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override IPermission Copy()
	{
		return new OleDbPermission(this);
	}
}
