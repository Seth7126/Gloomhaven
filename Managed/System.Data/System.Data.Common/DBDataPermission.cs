using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace System.Data.Common;

/// <summary>Enables a .NET Framework data provider to help ensure that a user has a security level adequate for accessing data.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)]
public abstract class DBDataPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private static class XmlStr
	{
		internal const string _class = "class";

		internal const string _IPermission = "IPermission";

		internal const string _Permission = "Permission";

		internal const string _Unrestricted = "Unrestricted";

		internal const string _AllowBlankPassword = "AllowBlankPassword";

		internal const string _true = "true";

		internal const string _Version = "version";

		internal const string _VersionNumber = "1";

		internal const string _add = "add";

		internal const string _ConnectionString = "ConnectionString";

		internal const string _KeyRestrictions = "KeyRestrictions";

		internal const string _KeyRestrictionBehavior = "KeyRestrictionBehavior";
	}

	private bool _isUnrestricted;

	private bool _allowBlankPassword;

	private NameValuePermission _keyvaluetree = NameValuePermission.Default;

	private ArrayList _keyvalues;

	/// <summary>Gets a value indicating whether a blank password is allowed.</summary>
	/// <returns>true if a blank password is allowed, otherwise, false.</returns>
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

	/// <summary>Initializes a new instance of a DBDataPermission class.</summary>
	[Obsolete("DBDataPermission() has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	protected DBDataPermission()
		: this(PermissionState.None)
	{
	}

	/// <summary>Initializes a new instance of a DBDataPermission class with the specified <see cref="T:System.Security.Permissions.PermissionState" /> value.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	protected DBDataPermission(PermissionState state)
	{
		switch (state)
		{
		case PermissionState.Unrestricted:
			_isUnrestricted = true;
			break;
		case PermissionState.None:
			_isUnrestricted = false;
			break;
		default:
			throw ADP.InvalidPermissionState(state);
		}
	}

	/// <summary>Initializes a new instance of a DBDataPermission class with the specified <see cref="T:System.Security.Permissions.PermissionState" /> value, and a value indicating whether a blank password is allowed.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <param name="allowBlankPassword">Indicates whether a blank password is allowed. </param>
	[Obsolete("DBDataPermission(PermissionState state,Boolean allowBlankPassword) has been deprecated.  Use the DBDataPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
	protected DBDataPermission(PermissionState state, bool allowBlankPassword)
		: this(state)
	{
		AllowBlankPassword = allowBlankPassword;
	}

	/// <summary>Initializes a new instance of a DBDataPermission class using an existing DBDataPermission.</summary>
	/// <param name="permission">An existing DBDataPermission used to create a new DBDataPermission. </param>
	protected DBDataPermission(DBDataPermission permission)
	{
		if (permission == null)
		{
			throw ADP.ArgumentNull("permissionAttribute");
		}
		CopyFrom(permission);
	}

	/// <summary>Initializes a new instance of a DBDataPermission class with the specified DBDataPermissionAttribute.</summary>
	/// <param name="permissionAttribute">A security action associated with a custom security attribute. </param>
	protected DBDataPermission(DBDataPermissionAttribute permissionAttribute)
	{
		if (permissionAttribute == null)
		{
			throw ADP.ArgumentNull("permissionAttribute");
		}
		_isUnrestricted = permissionAttribute.Unrestricted;
		if (!_isUnrestricted)
		{
			_allowBlankPassword = permissionAttribute.AllowBlankPassword;
			if (permissionAttribute.ShouldSerializeConnectionString() || permissionAttribute.ShouldSerializeKeyRestrictions())
			{
				Add(permissionAttribute.ConnectionString, permissionAttribute.KeyRestrictions, permissionAttribute.KeyRestrictionBehavior);
			}
		}
	}

	internal DBDataPermission(DbConnectionOptions connectionOptions)
	{
		if (connectionOptions != null)
		{
			_allowBlankPassword = connectionOptions.HasBlankPassword;
			AddPermissionEntry(new DBConnectionString(connectionOptions));
		}
	}

	/// <summary>Adds access for the specified connection string to the existing state of the DBDataPermission. </summary>
	/// <param name="connectionString">A permitted connection string.</param>
	/// <param name="restrictions">String that identifies connection string parameters that are allowed or disallowed.</param>
	/// <param name="behavior">One of the <see cref="T:System.Data.KeyRestrictionBehavior" /> properties.</param>
	/// <filterpriority>2</filterpriority>
	public virtual void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
	{
		DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, null, useOdbcRules: false);
		AddPermissionEntry(entry);
	}

	internal void AddPermissionEntry(DBConnectionString entry)
	{
		if (_keyvaluetree == null)
		{
			_keyvaluetree = new NameValuePermission();
		}
		if (_keyvalues == null)
		{
			_keyvalues = new ArrayList();
		}
		NameValuePermission.AddEntry(_keyvaluetree, _keyvalues, entry);
		_isUnrestricted = false;
	}

	/// <summary>Removes all permissions that were previous added using the <see cref="M:System.Data.Common.DBDataPermission.Add(System.String,System.String,System.Data.KeyRestrictionBehavior)" /> method.</summary>
	protected void Clear()
	{
		_keyvaluetree = null;
		_keyvalues = null;
	}

	/// <summary>Creates and returns an identical copy of the current permission object.</summary>
	/// <returns>A copy of the current permission object.</returns>
	/// <filterpriority>2</filterpriority>
	public override IPermission Copy()
	{
		DBDataPermission dBDataPermission = CreateInstance();
		dBDataPermission.CopyFrom(this);
		return dBDataPermission;
	}

	private void CopyFrom(DBDataPermission permission)
	{
		_isUnrestricted = permission.IsUnrestricted();
		if (_isUnrestricted)
		{
			return;
		}
		_allowBlankPassword = permission.AllowBlankPassword;
		if (permission._keyvalues != null)
		{
			_keyvalues = (ArrayList)permission._keyvalues.Clone();
			if (permission._keyvaluetree != null)
			{
				_keyvaluetree = permission._keyvaluetree.CopyNameValue();
			}
		}
	}

	/// <summary>Creates a new instance of the DBDataPermission class.</summary>
	/// <returns>A new DBDataPermission object.</returns>
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	protected virtual DBDataPermission CreateInstance()
	{
		return Activator.CreateInstance(GetType(), BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture, null) as DBDataPermission;
	}

	/// <summary>Returns a new permission object representing the intersection of the current permission object and the specified permission object.</summary>
	/// <returns>A new permission object that represents the intersection of the current permission object and the specified permission object. This new permission object is a null reference (Nothing in Visual Basic) if the intersection is empty.</returns>
	/// <param name="target">A permission object to intersect with the current permission object. It must be of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not a null reference (Nothing in Visual Basic) and is not an instance of the same class as the current permission object. </exception>
	/// <filterpriority>2</filterpriority>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (target.GetType() != GetType())
		{
			throw ADP.PermissionTypeMismatch();
		}
		if (IsUnrestricted())
		{
			return target.Copy();
		}
		DBDataPermission dBDataPermission = (DBDataPermission)target;
		if (dBDataPermission.IsUnrestricted())
		{
			return Copy();
		}
		DBDataPermission dBDataPermission2 = (DBDataPermission)dBDataPermission.Copy();
		dBDataPermission2._allowBlankPassword &= AllowBlankPassword;
		if (_keyvalues != null && dBDataPermission2._keyvalues != null)
		{
			dBDataPermission2._keyvalues.Clear();
			dBDataPermission2._keyvaluetree.Intersect(dBDataPermission2._keyvalues, _keyvaluetree);
		}
		else
		{
			dBDataPermission2._keyvalues = null;
			dBDataPermission2._keyvaluetree = null;
		}
		if (dBDataPermission2.IsEmpty())
		{
			dBDataPermission2 = null;
		}
		return dBDataPermission2;
	}

	private bool IsEmpty()
	{
		ArrayList keyvalues = _keyvalues;
		if (!IsUnrestricted() && !AllowBlankPassword)
		{
			if (keyvalues != null)
			{
				return keyvalues.Count == 0;
			}
			return true;
		}
		return false;
	}

	/// <summary>Returns a value indicating whether the current permission object is a subset of the specified permission object.</summary>
	/// <returns>true if the current permission object is a subset of the specified permission object, otherwise false.</returns>
	/// <param name="target">A permission object that is to be tested for the subset relationship. This object must be of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is an object that is not of the same type as the current permission object. </exception>
	/// <filterpriority>2</filterpriority>
	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			return IsEmpty();
		}
		if (target.GetType() != GetType())
		{
			throw ADP.PermissionTypeMismatch();
		}
		DBDataPermission dBDataPermission = target as DBDataPermission;
		bool flag = dBDataPermission.IsUnrestricted();
		if (!flag && !IsUnrestricted() && (!AllowBlankPassword || dBDataPermission.AllowBlankPassword) && (_keyvalues == null || dBDataPermission._keyvaluetree != null))
		{
			flag = true;
			if (_keyvalues != null)
			{
				foreach (DBConnectionString keyvalue in _keyvalues)
				{
					if (!dBDataPermission._keyvaluetree.CheckValueForKeyPermit(keyvalue))
					{
						flag = false;
						break;
					}
				}
			}
		}
		return flag;
	}

	/// <summary>Returns a value indicating whether the permission can be represented as unrestricted without any knowledge of the permission semantics.</summary>
	/// <returns>true if the permission can be represented as unrestricted.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsUnrestricted()
	{
		return _isUnrestricted;
	}

	/// <summary>Returns a new permission object that is the union of the current and specified permission objects.</summary>
	/// <returns>A new permission object that represents the union of the current permission object and the specified permission object.</returns>
	/// <param name="target">A permission object to combine with the current permission object. It must be of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> object is not the same type as the current permission object.</exception>
	/// <filterpriority>2</filterpriority>
	public override IPermission Union(IPermission target)
	{
		if (target == null)
		{
			return Copy();
		}
		if (target.GetType() != GetType())
		{
			throw ADP.PermissionTypeMismatch();
		}
		if (IsUnrestricted())
		{
			return Copy();
		}
		DBDataPermission dBDataPermission = (DBDataPermission)target.Copy();
		if (!dBDataPermission.IsUnrestricted())
		{
			dBDataPermission._allowBlankPassword |= AllowBlankPassword;
			if (_keyvalues != null)
			{
				foreach (DBConnectionString keyvalue in _keyvalues)
				{
					dBDataPermission.AddPermissionEntry(keyvalue);
				}
			}
		}
		if (!dBDataPermission.IsEmpty())
		{
			return dBDataPermission;
		}
		return null;
	}

	private string DecodeXmlValue(string value)
	{
		if (value != null && 0 < value.Length)
		{
			value = value.Replace("&quot;", "\"");
			value = value.Replace("&apos;", "'");
			value = value.Replace("&lt;", "<");
			value = value.Replace("&gt;", ">");
			value = value.Replace("&amp;", "&");
		}
		return value;
	}

	private string EncodeXmlValue(string value)
	{
		if (value != null && 0 < value.Length)
		{
			value = value.Replace('\0', ' ');
			value = value.Trim();
			value = value.Replace("&", "&amp;");
			value = value.Replace(">", "&gt;");
			value = value.Replace("<", "&lt;");
			value = value.Replace("'", "&apos;");
			value = value.Replace("\"", "&quot;");
		}
		return value;
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="securityElement">The XML encoding to use to reconstruct the security object. </param>
	/// <filterpriority>2</filterpriority>
	public override void FromXml(SecurityElement securityElement)
	{
		if (securityElement == null)
		{
			throw ADP.ArgumentNull("securityElement");
		}
		string tag = securityElement.Tag;
		if (!tag.Equals("Permission") && !tag.Equals("IPermission"))
		{
			throw ADP.NotAPermissionElement();
		}
		string text = securityElement.Attribute("version");
		if (text != null && !text.Equals("1"))
		{
			throw ADP.InvalidXMLBadVersion();
		}
		string text2 = securityElement.Attribute("Unrestricted");
		_isUnrestricted = text2 != null && bool.Parse(text2);
		Clear();
		if (!_isUnrestricted)
		{
			string text3 = securityElement.Attribute("AllowBlankPassword");
			_allowBlankPassword = text3 != null && bool.Parse(text3);
			ArrayList children = securityElement.Children;
			if (children == null)
			{
				return;
			}
			{
				foreach (SecurityElement item in children)
				{
					tag = item.Tag;
					if ("add" == tag || (tag != null && "add" == tag.ToLower(CultureInfo.InvariantCulture)))
					{
						string value = item.Attribute("ConnectionString");
						string value2 = item.Attribute("KeyRestrictions");
						string text4 = item.Attribute("KeyRestrictionBehavior");
						KeyRestrictionBehavior behavior = KeyRestrictionBehavior.AllowOnly;
						if (text4 != null)
						{
							behavior = (KeyRestrictionBehavior)Enum.Parse(typeof(KeyRestrictionBehavior), text4, ignoreCase: true);
						}
						value = DecodeXmlValue(value);
						value2 = DecodeXmlValue(value2);
						Add(value, value2, behavior);
					}
				}
				return;
			}
		}
		_allowBlankPassword = false;
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override SecurityElement ToXml()
	{
		Type type = GetType();
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", type.AssemblyQualifiedName.Replace('"', '\''));
		securityElement.AddAttribute("version", "1");
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			securityElement.AddAttribute("AllowBlankPassword", _allowBlankPassword.ToString(CultureInfo.InvariantCulture));
			if (_keyvalues != null)
			{
				foreach (DBConnectionString keyvalue in _keyvalues)
				{
					SecurityElement securityElement2 = new SecurityElement("add");
					string connectionString = keyvalue.ConnectionString;
					connectionString = EncodeXmlValue(connectionString);
					if (!ADP.IsEmpty(connectionString))
					{
						securityElement2.AddAttribute("ConnectionString", connectionString);
					}
					connectionString = keyvalue.Restrictions;
					connectionString = EncodeXmlValue(connectionString);
					if (connectionString == null)
					{
						connectionString = ADP.StrEmpty;
					}
					securityElement2.AddAttribute("KeyRestrictions", connectionString);
					connectionString = keyvalue.Behavior.ToString();
					securityElement2.AddAttribute("KeyRestrictionBehavior", connectionString);
					securityElement.AddChild(securityElement2);
				}
			}
		}
		return securityElement;
	}
}
