using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Provides an attribute that compares two properties.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CompareAttribute : ValidationAttribute
{
	/// <summary>Gets the property to compare with the current property.</summary>
	/// <returns>The other property.</returns>
	public string OtherProperty { get; private set; }

	/// <summary>Gets the display name of the other property.</summary>
	/// <returns>The display name of the other property.</returns>
	public string OtherPropertyDisplayName { get; internal set; }

	/// <summary>Gets a value that indicates whether the attribute requires validation context.</summary>
	/// <returns>true if the attribute requires validation context; otherwise, false.</returns>
	public override bool RequiresValidationContext => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.CompareAttribute" /> class.</summary>
	/// <param name="otherProperty">The property to compare with the current property.</param>
	public CompareAttribute(string otherProperty)
		: base("'{0}' and '{1}' do not match.")
	{
		if (otherProperty == null)
		{
			throw new ArgumentNullException("otherProperty");
		}
		OtherProperty = otherProperty;
	}

	/// <summary>Applies formatting to an error message, based on the data field where the error occurred.</summary>
	/// <returns>The formatted error message.</returns>
	/// <param name="name">The name of the field that caused the validation failure.</param>
	public override string FormatErrorMessage(string name)
	{
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty);
	}

	/// <summary>Determines whether a specified object is valid.</summary>
	/// <returns>true if <paramref name="value" /> is valid; otherwise, false.</returns>
	/// <param name="value">The object to validate.</param>
	/// <param name="validationContext">An object that contains information about the validation request.</param>
	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		PropertyInfo property = validationContext.ObjectType.GetProperty(OtherProperty);
		if (property == null)
		{
			return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Could not find a property named {0}.", OtherProperty));
		}
		object value2 = property.GetValue(validationContext.ObjectInstance, null);
		if (!object.Equals(value, value2))
		{
			if (OtherPropertyDisplayName == null)
			{
				OtherPropertyDisplayName = GetDisplayNameForProperty(validationContext.ObjectType, OtherProperty);
			}
			return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
		}
		return null;
	}

	private static string GetDisplayNameForProperty(Type containerType, string propertyName)
	{
		IEnumerable<Attribute> source = (GetTypeDescriptor(containerType).GetProperties().Find(propertyName, ignoreCase: true) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The property {0}.{1} could not be found.", containerType.FullName, propertyName))).Attributes.Cast<Attribute>();
		DisplayAttribute displayAttribute = source.OfType<DisplayAttribute>().FirstOrDefault();
		if (displayAttribute != null)
		{
			return displayAttribute.GetName();
		}
		DisplayNameAttribute displayNameAttribute = source.OfType<DisplayNameAttribute>().FirstOrDefault();
		if (displayNameAttribute != null)
		{
			return displayNameAttribute.DisplayName;
		}
		return propertyName;
	}

	private static ICustomTypeDescriptor GetTypeDescriptor(Type type)
	{
		return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
	}
}
