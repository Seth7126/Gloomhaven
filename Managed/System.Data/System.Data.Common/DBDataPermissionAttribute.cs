using System.ComponentModel;
using System.Security.Permissions;

namespace System.Data.Common;

/// <summary>Associates a security action with a custom security attribute. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute
{
	private bool _allowBlankPassword;

	private string _connectionString;

	private string _restrictions;

	private KeyRestrictionBehavior _behavior;

	/// <summary>Gets or sets a value indicating whether a blank password is allowed.</summary>
	/// <returns>true if a blank password is allowed; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool AllowBlankPassword
	{
		get
		{
			return _allowBlankPassword;
		}
		set
		{
			_allowBlankPassword = value;
		}
	}

	/// <summary>Gets or sets a permitted connection string.</summary>
	/// <returns>A permitted connection string.</returns>
	/// <filterpriority>2</filterpriority>
	public string ConnectionString
	{
		get
		{
			string connectionString = _connectionString;
			if (connectionString == null)
			{
				return string.Empty;
			}
			return connectionString;
		}
		set
		{
			_connectionString = value;
		}
	}

	/// <summary>Identifies whether the list of connection string parameters identified by the <see cref="P:System.Data.Common.DBDataPermissionAttribute.KeyRestrictions" /> property are the only connection string parameters allowed.</summary>
	/// <returns>One of the <see cref="T:System.Data.KeyRestrictionBehavior" /> values.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public KeyRestrictionBehavior KeyRestrictionBehavior
	{
		get
		{
			return _behavior;
		}
		set
		{
			if ((uint)value <= 1u)
			{
				_behavior = value;
				return;
			}
			throw ADP.InvalidKeyRestrictionBehavior(value);
		}
	}

	/// <summary>Gets or sets connection string parameters that are allowed or disallowed.</summary>
	/// <returns>One or more connection string parameters that are allowed or disallowed.</returns>
	/// <filterpriority>2</filterpriority>
	public string KeyRestrictions
	{
		get
		{
			string restrictions = _restrictions;
			if (restrictions == null)
			{
				return ADP.StrEmpty;
			}
			return restrictions;
		}
		set
		{
			_restrictions = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DBDataPermissionAttribute" />.</summary>
	/// <param name="action">One of the security action values representing an action that can be performed by declarative security.</param>
	protected DBDataPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Identifies whether the attribute should serialize the connection string.</summary>
	/// <returns>true if the attribute should serialize the connection string; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeConnectionString()
	{
		return _connectionString != null;
	}

	/// <summary>Identifies whether the attribute should serialize the set of key restrictions.</summary>
	/// <returns>true if the attribute should serialize the set of key restrictions; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeKeyRestrictions()
	{
		return _restrictions != null;
	}
}
