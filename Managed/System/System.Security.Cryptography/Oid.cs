using Internal.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Represents a cryptographic object identifier. This class cannot be inherited.</summary>
public sealed class Oid
{
	private string _value;

	private string _friendlyName;

	private OidGroup _group;

	/// <summary>Gets or sets the dotted number of the identifier.</summary>
	/// <returns>The dotted number of the identifier.</returns>
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	/// <summary>Gets or sets the friendly name of the identifier.</summary>
	/// <returns>The friendly name of the identifier.</returns>
	public string FriendlyName
	{
		get
		{
			if (_friendlyName == null && _value != null)
			{
				_friendlyName = OidLookup.ToFriendlyName(_value, _group, fallBackToAllGroups: true);
			}
			return _friendlyName;
		}
		set
		{
			_friendlyName = value;
			if (_friendlyName != null)
			{
				string text = OidLookup.ToOid(_friendlyName, _group, fallBackToAllGroups: true);
				if (text != null)
				{
					_value = text;
				}
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class.</summary>
	public Oid()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using a string value of an <see cref="T:System.Security.Cryptography.Oid" /> object.</summary>
	/// <param name="oid">An object identifier.</param>
	public Oid(string oid)
	{
		string text = OidLookup.ToOid(oid, OidGroup.All, fallBackToAllGroups: false);
		if (text == null)
		{
			text = oid;
		}
		Value = text;
		_group = OidGroup.All;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using the specified value and friendly name.</summary>
	/// <param name="value">The dotted number of the identifier.</param>
	/// <param name="friendlyName">The friendly name of the identifier.</param>
	public Oid(string value, string friendlyName)
	{
		_value = value;
		_friendlyName = friendlyName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using the specified <see cref="T:System.Security.Cryptography.Oid" /> object.</summary>
	/// <param name="oid">The object identifier information to use to create the new object identifier.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oid " />is null.</exception>
	public Oid(Oid oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		_value = oid._value;
		_friendlyName = oid._friendlyName;
		_group = oid._group;
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.Oid" /> object from an OID friendly name by searching the specified group.</summary>
	/// <returns>An object that represents the specified OID.</returns>
	/// <param name="friendlyName">The friendly name of the identifier.</param>
	/// <param name="group">The group to search in.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="friendlyName " /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The OID was not found.</exception>
	public static Oid FromFriendlyName(string friendlyName, OidGroup group)
	{
		if (friendlyName == null)
		{
			throw new ArgumentNullException("friendlyName");
		}
		return new Oid(OidLookup.ToOid(friendlyName, group, fallBackToAllGroups: false) ?? throw new CryptographicException("No OID value matches this name."), friendlyName, group);
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.Oid" /> object by using the specified OID value and group.</summary>
	/// <returns>A new instance of an <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	/// <param name="oidValue">The OID value.</param>
	/// <param name="group">The group to search in.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oidValue" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The friendly name for the OID value was not found.</exception>
	public static Oid FromOidValue(string oidValue, OidGroup group)
	{
		if (oidValue == null)
		{
			throw new ArgumentNullException("oidValue");
		}
		string text = OidLookup.ToFriendlyName(oidValue, group, fallBackToAllGroups: false);
		if (text == null)
		{
			throw new CryptographicException("The OID value is invalid.");
		}
		return new Oid(oidValue, text, group);
	}

	private Oid(string value, string friendlyName, OidGroup group)
	{
		_value = value;
		_friendlyName = friendlyName;
		_group = group;
	}
}
