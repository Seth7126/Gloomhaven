using System.Globalization;

namespace System.ComponentModel.DataAnnotations.Schema;

/// <summary>Represents an inverse property attribute.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class InversePropertyAttribute : Attribute
{
	private readonly string _property;

	/// <summary>Gets the property of the attribute.</summary>
	/// <returns>The property of the attribute.</returns>
	public string Property => _property;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute" /> class using the specified property.</summary>
	/// <param name="property">The property of the attribute.</param>
	public InversePropertyAttribute(string property)
	{
		if (string.IsNullOrWhiteSpace(property))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The argument '{0}' cannot be null, empty or contain only white space.", "property"));
		}
		_property = property;
	}
}
