using System.Globalization;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies a custom validation method that is used to validate a property or class instance.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class CustomValidationAttribute : ValidationAttribute
{
	private Type _validatorType;

	private string _method;

	private MethodInfo _methodInfo;

	private bool _isSingleArgumentMethod;

	private string _lastMessage;

	private Type _valuesType;

	private Lazy<string> _malformedErrorMessage;

	private Tuple<string, Type> _typeId;

	/// <summary>Gets the type that performs custom validation.</summary>
	/// <returns>The type that performs custom validation.</returns>
	public Type ValidatorType => _validatorType;

	/// <summary>Gets the validation method.</summary>
	/// <returns>The name of the validation method.</returns>
	public string Method => _method;

	/// <summary>Gets a unique identifier for this attribute.</summary>
	/// <returns>The object that identifies this attribute.</returns>
	public override object TypeId
	{
		get
		{
			if (_typeId == null)
			{
				_typeId = new Tuple<string, Type>(_method, _validatorType);
			}
			return _typeId;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.CustomValidationAttribute" /> class.</summary>
	/// <param name="validatorType">The type that contains the method that performs custom validation.</param>
	/// <param name="method">The method that performs custom validation.</param>
	public CustomValidationAttribute(Type validatorType, string method)
		: base(() => "{0} is not valid.")
	{
		_validatorType = validatorType;
		_method = method;
		_malformedErrorMessage = new Lazy<string>(CheckAttributeWellFormed);
	}

	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		ThrowIfAttributeNotWellFormed();
		MethodInfo methodInfo = _methodInfo;
		if (!TryConvertValue(value, out var convertedValue))
		{
			return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "Could not convert the value of type '{0}' to '{1}' as expected by method {2}.{3}.", (value != null) ? value.GetType().ToString() : "null", _valuesType, _validatorType, _method));
		}
		try
		{
			object[] parameters = ((!_isSingleArgumentMethod) ? new object[2] { convertedValue, validationContext } : new object[1] { convertedValue });
			ValidationResult validationResult = (ValidationResult)methodInfo.Invoke(null, parameters);
			_lastMessage = null;
			if (validationResult != null)
			{
				_lastMessage = validationResult.ErrorMessage;
			}
			return validationResult;
		}
		catch (TargetInvocationException ex)
		{
			if (ex.InnerException != null)
			{
				throw ex.InnerException;
			}
			throw;
		}
	}

	/// <summary>Formats a validation error message.</summary>
	/// <returns>An instance of the formatted error message.</returns>
	/// <param name="name">The name to include in the formatted message.</param>
	public override string FormatErrorMessage(string name)
	{
		ThrowIfAttributeNotWellFormed();
		if (!string.IsNullOrEmpty(_lastMessage))
		{
			return string.Format(CultureInfo.CurrentCulture, _lastMessage, name);
		}
		return base.FormatErrorMessage(name);
	}

	private string CheckAttributeWellFormed()
	{
		return ValidateValidatorTypeParameter() ?? ValidateMethodParameter();
	}

	private string ValidateValidatorTypeParameter()
	{
		if (_validatorType == null)
		{
			return "The CustomValidationAttribute.ValidatorType was not specified.";
		}
		if (!_validatorType.IsVisible)
		{
			return string.Format(CultureInfo.CurrentCulture, "The custom validation type '{0}' must be public.", _validatorType.Name);
		}
		return null;
	}

	private string ValidateMethodParameter()
	{
		if (string.IsNullOrEmpty(_method))
		{
			return "The CustomValidationAttribute.Method was not specified.";
		}
		MethodInfo method = _validatorType.GetMethod(_method, BindingFlags.Static | BindingFlags.Public);
		if (method == null)
		{
			return string.Format(CultureInfo.CurrentCulture, "The CustomValidationAttribute method '{0}' does not exist in type '{1}' or is not public and static.", _method, _validatorType.Name);
		}
		if (method.ReturnType != typeof(ValidationResult))
		{
			return string.Format(CultureInfo.CurrentCulture, "The CustomValidationAttribute method '{0}' in type '{1}' must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success to represent success.", _method, _validatorType.Name);
		}
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0 || parameters[0].ParameterType.IsByRef)
		{
			return string.Format(CultureInfo.CurrentCulture, "The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static ValidationResult {0}(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.", _method, _validatorType.Name);
		}
		_isSingleArgumentMethod = parameters.Length == 1;
		if (!_isSingleArgumentMethod && (parameters.Length != 2 || parameters[1].ParameterType != typeof(ValidationContext)))
		{
			return string.Format(CultureInfo.CurrentCulture, "The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static ValidationResult {0}(object value, ValidationContext context).  The value can be strongly typed.  The ValidationContext parameter is optional.", _method, _validatorType.Name);
		}
		_methodInfo = method;
		_valuesType = parameters[0].ParameterType;
		return null;
	}

	private void ThrowIfAttributeNotWellFormed()
	{
		string value = _malformedErrorMessage.Value;
		if (value != null)
		{
			throw new InvalidOperationException(value);
		}
	}

	private bool TryConvertValue(object value, out object convertedValue)
	{
		convertedValue = null;
		Type valuesType = _valuesType;
		if (value == null)
		{
			if (valuesType.IsValueType && (!valuesType.IsGenericType || valuesType.GetGenericTypeDefinition() != typeof(Nullable<>)))
			{
				return false;
			}
			return true;
		}
		if (valuesType.IsAssignableFrom(value.GetType()))
		{
			convertedValue = value;
			return true;
		}
		try
		{
			convertedValue = Convert.ChangeType(value, valuesType, CultureInfo.CurrentCulture);
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (InvalidCastException)
		{
			return false;
		}
		catch (NotSupportedException)
		{
			return false;
		}
	}
}
