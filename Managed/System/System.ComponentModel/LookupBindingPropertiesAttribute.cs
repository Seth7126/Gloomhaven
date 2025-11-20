namespace System.ComponentModel;

/// <summary>Specifies the properties that support lookup-based binding. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LookupBindingPropertiesAttribute : Attribute
{
	/// <summary>Represents the default value for the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> class.</summary>
	public static readonly LookupBindingPropertiesAttribute Default = new LookupBindingPropertiesAttribute();

	/// <summary>Gets the name of the data source property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</summary>
	/// <returns>The data source property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</returns>
	public string DataSource { get; }

	/// <summary>Gets the name of the display member property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</summary>
	/// <returns>The name of the display member property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</returns>
	public string DisplayMember { get; }

	/// <summary>Gets the name of the value member property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</summary>
	/// <returns>The name of the value member property for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</returns>
	public string ValueMember { get; }

	/// <summary>Gets the name of the lookup member for the component to which this attribute is bound.</summary>
	/// <returns>The name of the lookup member for the component to which the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> is bound.</returns>
	public string LookupMember { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> class using no parameters. </summary>
	public LookupBindingPropertiesAttribute()
	{
		DataSource = null;
		DisplayMember = null;
		ValueMember = null;
		LookupMember = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> class. </summary>
	/// <param name="dataSource">The name of the property to be used as the data source.</param>
	/// <param name="displayMember">The name of the property to be used for the display name.</param>
	/// <param name="valueMember">The name of the property to be used as the source for values.</param>
	/// <param name="lookupMember">The name of the property to be used for lookups.</param>
	public LookupBindingPropertiesAttribute(string dataSource, string displayMember, string valueMember, string lookupMember)
	{
		DataSource = dataSource;
		DisplayMember = displayMember;
		ValueMember = valueMember;
		LookupMember = lookupMember;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> instance. </summary>
	/// <returns>true if the object is equal to the current instance; otherwise, false, indicating they are not equal.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" /> instance </param>
	public override bool Equals(object obj)
	{
		if (obj is LookupBindingPropertiesAttribute lookupBindingPropertiesAttribute && lookupBindingPropertiesAttribute.DataSource == DataSource && lookupBindingPropertiesAttribute.DisplayMember == DisplayMember && lookupBindingPropertiesAttribute.ValueMember == ValueMember)
		{
			return lookupBindingPropertiesAttribute.LookupMember == LookupMember;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.LookupBindingPropertiesAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
