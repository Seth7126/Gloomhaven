namespace System.ComponentModel;

/// <summary>Specifies what type to use as a converter for the object this attribute is bound to.</summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class TypeConverterAttribute : Attribute
{
	/// <summary>Specifies the type to use as a converter for the object this attribute is bound to. </summary>
	public static readonly TypeConverterAttribute Default = new TypeConverterAttribute();

	/// <summary>Gets the fully qualified type name of the <see cref="T:System.Type" /> to use as a converter for the object this attribute is bound to.</summary>
	/// <returns>The fully qualified type name of the <see cref="T:System.Type" /> to use as a converter for the object this attribute is bound to, or an empty string ("") if none exists. The default value is an empty string ("").</returns>
	public string ConverterTypeName { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeConverterAttribute" /> class with the default type converter, which is an empty string ("").</summary>
	public TypeConverterAttribute()
	{
		ConverterTypeName = string.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeConverterAttribute" /> class, using the specified type as the data converter for the object this attribute is bound to.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type of the converter class to use for data conversion for the object this attribute is bound to. </param>
	public TypeConverterAttribute(Type type)
	{
		ConverterTypeName = type.AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeConverterAttribute" /> class, using the specified type name as the data converter for the object this attribute is bound to.</summary>
	/// <param name="typeName">The fully qualified name of the class to use for data conversion for the object this attribute is bound to. </param>
	public TypeConverterAttribute(string typeName)
	{
		ConverterTypeName = typeName;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.TypeConverterAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current <see cref="T:System.ComponentModel.TypeConverterAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj is TypeConverterAttribute typeConverterAttribute)
		{
			return typeConverterAttribute.ConverterTypeName == ConverterTypeName;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.TypeConverterAttribute" />.</returns>
	public override int GetHashCode()
	{
		return ConverterTypeName.GetHashCode();
	}
}
