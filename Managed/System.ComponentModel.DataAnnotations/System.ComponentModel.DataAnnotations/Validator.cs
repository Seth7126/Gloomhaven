using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Defines a helper class that can be used to validate objects, properties, and methods when it is included in their associated <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute" /> attributes.</summary>
public static class Validator
{
	private class ValidationError
	{
		internal object Value { get; set; }

		internal ValidationAttribute ValidationAttribute { get; set; }

		internal ValidationResult ValidationResult { get; set; }

		internal ValidationError(ValidationAttribute validationAttribute, object value, ValidationResult validationResult)
		{
			ValidationAttribute = validationAttribute;
			ValidationResult = validationResult;
			Value = value;
		}

		internal void ThrowValidationException()
		{
			throw new ValidationException(ValidationResult, ValidationAttribute, Value);
		}
	}

	private static ValidationAttributeStore _store = ValidationAttributeStore.Instance;

	/// <summary>Validates the property.</summary>
	/// <returns>true if the property validates; otherwise, false.</returns>
	/// <param name="value">The value to validate.</param>
	/// <param name="validationContext">The context that describes the property to validate.</param>
	/// <param name="validationResults">A collection to hold each failed validation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> cannot be assigned to the property.-or-<paramref name="value " />is null.</exception>
	public static bool TryValidateProperty(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
	{
		Type propertyType = _store.GetPropertyType(validationContext);
		EnsureValidPropertyType(validationContext.MemberName, propertyType, value);
		bool result = true;
		bool breakOnFirstError = validationResults == null;
		IEnumerable<ValidationAttribute> propertyValidationAttributes = _store.GetPropertyValidationAttributes(validationContext);
		foreach (ValidationError validationError in GetValidationErrors(value, validationContext, propertyValidationAttributes, breakOnFirstError))
		{
			result = false;
			validationResults?.Add(validationError.ValidationResult);
		}
		return result;
	}

	/// <summary>Determines whether the specified object is valid using the validation context and validation results collection.</summary>
	/// <returns>true if the object validates; otherwise, false.</returns>
	/// <param name="instance">The object to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <param name="validationResults">A collection to hold each failed validation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public static bool TryValidateObject(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
	{
		return TryValidateObject(instance, validationContext, validationResults, validateAllProperties: false);
	}

	/// <summary>Determines whether the specified object is valid using the validation context, validation results collection, and a value that specifies whether to validate all properties.</summary>
	/// <returns>true if the object validates; otherwise, false.</returns>
	/// <param name="instance">The object to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <param name="validationResults">A collection to hold each failed validation.</param>
	/// <param name="validateAllProperties">true to validate all properties; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public static bool TryValidateObject(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults, bool validateAllProperties)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (validationContext != null && instance != validationContext.ObjectInstance)
		{
			throw new ArgumentException("The instance provided must match the ObjectInstance on the ValidationContext supplied.", "instance");
		}
		bool result = true;
		bool breakOnFirstError = validationResults == null;
		foreach (ValidationError objectValidationError in GetObjectValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError))
		{
			result = false;
			validationResults?.Add(objectValidationError.ValidationResult);
		}
		return result;
	}

	/// <summary>Returns a value that indicates whether the specified value is valid with the specified attributes.</summary>
	/// <returns>true if the object validates; otherwise, false.</returns>
	/// <param name="value">The value to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <param name="validationResults">A collection to hold failed validations. </param>
	/// <param name="validationAttributes">The validation attributes.</param>
	public static bool TryValidateValue(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults, IEnumerable<ValidationAttribute> validationAttributes)
	{
		bool result = true;
		bool breakOnFirstError = validationResults == null;
		foreach (ValidationError validationError in GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
		{
			result = false;
			validationResults?.Add(validationError.ValidationResult);
		}
		return result;
	}

	/// <summary>Validates the property.</summary>
	/// <param name="value">The value to validate.</param>
	/// <param name="validationContext">The context that describes the property to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> cannot be assigned to the property.</exception>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The <paramref name="value" /> parameter is not valid.</exception>
	public static void ValidateProperty(object value, ValidationContext validationContext)
	{
		Type propertyType = _store.GetPropertyType(validationContext);
		EnsureValidPropertyType(validationContext.MemberName, propertyType, value);
		IEnumerable<ValidationAttribute> propertyValidationAttributes = _store.GetPropertyValidationAttributes(validationContext);
		GetValidationErrors(value, validationContext, propertyValidationAttributes, breakOnFirstError: false).FirstOrDefault()?.ThrowValidationException();
	}

	/// <summary>Determines whether the specified object is valid using the validation context.</summary>
	/// <param name="instance">The object to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The object is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public static void ValidateObject(object instance, ValidationContext validationContext)
	{
		ValidateObject(instance, validationContext, validateAllProperties: false);
	}

	/// <summary>Determines whether the specified object is valid using the validation context, and a value that specifies whether to validate all properties.</summary>
	/// <param name="instance">The object to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <param name="validateAllProperties">true to validate all properties; otherwise, false.</param>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">
	///   <paramref name="instance" /> is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public static void ValidateObject(object instance, ValidationContext validationContext, bool validateAllProperties)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		if (instance != validationContext.ObjectInstance)
		{
			throw new ArgumentException("The instance provided must match the ObjectInstance on the ValidationContext supplied.", "instance");
		}
		GetObjectValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError: false).FirstOrDefault()?.ThrowValidationException();
	}

	/// <summary>Validates the specified attributes.</summary>
	/// <param name="value">The value to validate.</param>
	/// <param name="validationContext">The context that describes the object to validate.</param>
	/// <param name="validationAttributes">The validation attributes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="validationContext" /> parameter is null.</exception>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The <paramref name="value" /> parameter does not validate with the <paramref name="validationAttributes" /> parameter.</exception>
	public static void ValidateValue(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> validationAttributes)
	{
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError: false).FirstOrDefault()?.ThrowValidationException();
	}

	internal static ValidationContext CreateValidationContext(object instance, ValidationContext validationContext)
	{
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		return new ValidationContext(instance, validationContext, validationContext.Items);
	}

	private static bool CanBeAssigned(Type destinationType, object value)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value == null)
		{
			if (destinationType.IsValueType)
			{
				if (destinationType.IsGenericType)
				{
					return destinationType.GetGenericTypeDefinition() == typeof(Nullable<>);
				}
				return false;
			}
			return true;
		}
		return destinationType.IsAssignableFrom(value.GetType());
	}

	private static void EnsureValidPropertyType(string propertyName, Type propertyType, object value)
	{
		if (!CanBeAssigned(propertyType, value))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The value for property '{0}' must be of type '{1}'.", propertyName, propertyType), "value");
		}
	}

	private static IEnumerable<ValidationError> GetObjectValidationErrors(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		List<ValidationError> list = new List<ValidationError>();
		list.AddRange(GetObjectPropertyValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError));
		if (list.Any())
		{
			return list;
		}
		IEnumerable<ValidationAttribute> typeValidationAttributes = _store.GetTypeValidationAttributes(validationContext);
		list.AddRange(GetValidationErrors(instance, validationContext, typeValidationAttributes, breakOnFirstError));
		if (list.Any())
		{
			return list;
		}
		if (instance is IValidatableObject validatableObject)
		{
			foreach (ValidationResult item in from r in validatableObject.Validate(validationContext)
				where r != ValidationResult.Success
				select r)
			{
				list.Add(new ValidationError(null, instance, item));
			}
		}
		return list;
	}

	private static IEnumerable<ValidationError> GetObjectPropertyValidationErrors(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
	{
		ICollection<KeyValuePair<ValidationContext, object>> propertyValues = GetPropertyValues(instance, validationContext);
		List<ValidationError> list = new List<ValidationError>();
		foreach (KeyValuePair<ValidationContext, object> item in propertyValues)
		{
			IEnumerable<ValidationAttribute> propertyValidationAttributes = _store.GetPropertyValidationAttributes(item.Key);
			if (validateAllProperties)
			{
				list.AddRange(GetValidationErrors(item.Value, item.Key, propertyValidationAttributes, breakOnFirstError));
			}
			else if (propertyValidationAttributes.FirstOrDefault((ValidationAttribute a) => a is RequiredAttribute) is RequiredAttribute requiredAttribute)
			{
				ValidationResult validationResult = requiredAttribute.GetValidationResult(item.Value, item.Key);
				if (validationResult != ValidationResult.Success)
				{
					list.Add(new ValidationError(requiredAttribute, item.Value, validationResult));
				}
			}
			if (breakOnFirstError && list.Any())
			{
				break;
			}
		}
		return list;
	}

	private static ICollection<KeyValuePair<ValidationContext, object>> GetPropertyValues(object instance, ValidationContext validationContext)
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
		List<KeyValuePair<ValidationContext, object>> list = new List<KeyValuePair<ValidationContext, object>>(properties.Count);
		foreach (PropertyDescriptor item in properties)
		{
			ValidationContext validationContext2 = CreateValidationContext(instance, validationContext);
			validationContext2.MemberName = item.Name;
			if (_store.GetPropertyValidationAttributes(validationContext2).Any())
			{
				list.Add(new KeyValuePair<ValidationContext, object>(validationContext2, item.GetValue(instance)));
			}
		}
		return list;
	}

	private static IEnumerable<ValidationError> GetValidationErrors(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> attributes, bool breakOnFirstError)
	{
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		List<ValidationError> list = new List<ValidationError>();
		RequiredAttribute requiredAttribute = attributes.FirstOrDefault((ValidationAttribute a) => a is RequiredAttribute) as RequiredAttribute;
		if (requiredAttribute != null && !TryValidate(value, validationContext, requiredAttribute, out var validationError))
		{
			list.Add(validationError);
			return list;
		}
		foreach (ValidationAttribute attribute in attributes)
		{
			if (attribute != requiredAttribute && !TryValidate(value, validationContext, attribute, out validationError))
			{
				list.Add(validationError);
				if (breakOnFirstError)
				{
					break;
				}
			}
		}
		return list;
	}

	private static bool TryValidate(object value, ValidationContext validationContext, ValidationAttribute attribute, out ValidationError validationError)
	{
		if (validationContext == null)
		{
			throw new ArgumentNullException("validationContext");
		}
		ValidationResult validationResult = attribute.GetValidationResult(value, validationContext);
		if (validationResult != ValidationResult.Success)
		{
			validationError = new ValidationError(attribute, value, validationResult);
			return false;
		}
		validationError = null;
		return true;
	}
}
