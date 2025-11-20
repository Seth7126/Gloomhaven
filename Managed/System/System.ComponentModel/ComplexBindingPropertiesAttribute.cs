namespace System.ComponentModel;

/// <summary>Specifies the data source and data member properties for a component that supports complex data binding. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ComplexBindingPropertiesAttribute : Attribute
{
	/// <summary>Represents the default value for the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> class.</summary>
	public static readonly ComplexBindingPropertiesAttribute Default = new ComplexBindingPropertiesAttribute();

	/// <summary>Gets the name of the data source property for the component to which the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> is bound.</summary>
	/// <returns>The name of the data source property for the component to which <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> is bound.</returns>
	public string DataSource { get; }

	/// <summary>Gets the name of the data member property for the component to which the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> is bound.</summary>
	/// <returns>The name of the data member property for the component to which <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> is bound</returns>
	public string DataMember { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> class using no parameters. </summary>
	public ComplexBindingPropertiesAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> class using the specified data source. </summary>
	/// <param name="dataSource">The name of the property to be used as the data source.</param>
	public ComplexBindingPropertiesAttribute(string dataSource)
	{
		DataSource = dataSource;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> class using the specified data source and data member. </summary>
	/// <param name="dataSource">The name of the property to be used as the data source.</param>
	/// <param name="dataMember">The name of the property to be used as the source for data.</param>
	public ComplexBindingPropertiesAttribute(string dataSource, string dataMember)
	{
		DataSource = dataSource;
		DataMember = dataMember;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> instance. </summary>
	/// <returns>true if the object is equal to the current instance; otherwise, false, indicating they are not equal.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.ComponentModel.ComplexBindingPropertiesAttribute" /> instance </param>
	public override bool Equals(object obj)
	{
		if (obj is ComplexBindingPropertiesAttribute complexBindingPropertiesAttribute && complexBindingPropertiesAttribute.DataSource == DataSource)
		{
			return complexBindingPropertiesAttribute.DataMember == DataMember;
		}
		return false;
	}

	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
