using System;
using System.Linq;
using System.Reflection;

namespace Manatee.Trello.Internal.Validation;

internal class EnumerationRule<T> : IValidationRule<T>
{
	private readonly Type _enumType;

	private readonly bool _isNullable;

	public static IValidationRule<T> Instance { get; }

	static EnumerationRule()
	{
		Instance = new EnumerationRule<T>();
	}

	private EnumerationRule()
	{
		_enumType = typeof(T);
		if (_enumType.GetTypeInfo().IsGenericType)
		{
			if (_enumType.GetGenericTypeDefinition() != typeof(Nullable<>))
			{
				throw new ArgumentException($"Type {_enumType} must be an enumeration or a nullable enumeration.");
			}
			_enumType = _enumType.GetTypeInfo().GenericTypeArguments.First();
			_isNullable = true;
		}
		if (!_enumType.GetTypeInfo().IsEnum)
		{
			throw new ArgumentException($"Type {_enumType} must be an enumeration or a nullable enumeration.");
		}
	}

	public string Validate(T oldValue, T newValue)
	{
		if (_isNullable && object.Equals(newValue, default(T)))
		{
			return null;
		}
		if (Enum.GetValues(_enumType).Cast<T>().Contains(newValue))
		{
			return null;
		}
		return $"{newValue} is not defined in type {_enumType.Name}.";
	}
}
