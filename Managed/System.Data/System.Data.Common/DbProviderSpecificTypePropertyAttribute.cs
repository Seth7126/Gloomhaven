namespace System.Data.Common;

/// <summary>Identifies which provider-specific property in the strongly typed parameter classes is to be used when setting a provider-specific type.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DbProviderSpecificTypePropertyAttribute : Attribute
{
	/// <summary>Indicates whether the attributed property is a provider-specific type.</summary>
	/// <returns>true if the property that this attribute is applied to is a provider-specific type property; otherwise false.</returns>
	public bool IsProviderSpecificTypeProperty { get; }

	/// <summary>Initializes a new instance of a <see cref="T:System.Data.Common.DbProviderSpecificTypePropertyAttribute" /> class.</summary>
	/// <param name="isProviderSpecificTypeProperty">Specifies whether this property is a provider-specific property.</param>
	public DbProviderSpecificTypePropertyAttribute(bool isProviderSpecificTypeProperty)
	{
		IsProviderSpecificTypeProperty = isProviderSpecificTypeProperty;
	}
}
