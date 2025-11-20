namespace System.ComponentModel;

/// <summary>Specifies the <see cref="T:System.ComponentModel.LicenseProvider" /> to use with a class. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class LicenseProviderAttribute : Attribute
{
	/// <summary>Specifies the default value, which is no provider. This static field is read-only.</summary>
	public static readonly LicenseProviderAttribute Default = new LicenseProviderAttribute();

	private Type _licenseProviderType;

	private string _licenseProviderName;

	/// <summary>Gets the license provider that must be used with the associated class.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type of the license provider. The default value is null.</returns>
	public Type LicenseProvider
	{
		get
		{
			if (_licenseProviderType == null && _licenseProviderName != null)
			{
				_licenseProviderType = Type.GetType(_licenseProviderName);
			}
			return _licenseProviderType;
		}
	}

	/// <summary>Indicates a unique ID for this attribute type.</summary>
	/// <returns>A unique ID for this attribute type.</returns>
	public override object TypeId
	{
		get
		{
			string text = _licenseProviderName;
			if (text == null && _licenseProviderType != null)
			{
				text = _licenseProviderType.FullName;
			}
			return GetType().FullName + text;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.LicenseProviderAttribute" /> class without a license provider.</summary>
	public LicenseProviderAttribute()
		: this((string)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.LicenseProviderAttribute" /> class with the specified type.</summary>
	/// <param name="typeName">The fully qualified name of the license provider class. </param>
	public LicenseProviderAttribute(string typeName)
	{
		_licenseProviderName = typeName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.LicenseProviderAttribute" /> class with the specified type of license provider.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of the license provider class. </param>
	public LicenseProviderAttribute(Type type)
	{
		_licenseProviderType = type;
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <returns>true if <paramref name="value" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="value">Another object to compare to. </param>
	public override bool Equals(object value)
	{
		if (value is LicenseProviderAttribute && value != null)
		{
			Type licenseProvider = ((LicenseProviderAttribute)value).LicenseProvider;
			if (licenseProvider == LicenseProvider)
			{
				return true;
			}
			if (licenseProvider != null && licenseProvider.Equals(LicenseProvider))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.LicenseProviderAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
