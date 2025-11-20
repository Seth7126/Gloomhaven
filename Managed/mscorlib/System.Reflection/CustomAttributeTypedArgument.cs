using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace System.Reflection;

/// <summary>Represents an argument of a custom attribute in the reflection-only context, or an element of an array argument.</summary>
public struct CustomAttributeTypedArgument
{
	/// <summary>Gets the type of the argument or of the array argument element.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the type of the argument or of the array element.</returns>
	public Type ArgumentType { get; }

	/// <summary>Gets the value of the argument for a simple argument or for an element of an array argument; gets a collection of values for an array argument.</summary>
	/// <returns>An object that represents the value of the argument or element, or a generic <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> objects that represent the values of an array-type argument.</returns>
	public object Value { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> class with the specified value.</summary>
	/// <param name="value">The value of the custom attribute argument.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CustomAttributeTypedArgument(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Value = CanonicalizeValue(value);
		ArgumentType = value.GetType();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> class with the specified type and value.</summary>
	/// <param name="argumentType">The type of the custom attribute argument.</param>
	/// <param name="value">The value of the custom attribute argument.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="argumentType" /> is null.</exception>
	public CustomAttributeTypedArgument(Type argumentType, object value)
	{
		if (argumentType == null)
		{
			throw new ArgumentNullException("argumentType");
		}
		Value = ((value == null) ? null : CanonicalizeValue(value));
		ArgumentType = argumentType;
		if (value is Array array)
		{
			Type elementType = array.GetType().GetElementType();
			CustomAttributeTypedArgument[] array2 = new CustomAttributeTypedArgument[array.GetLength(0)];
			for (int i = 0; i < array2.Length; i++)
			{
				object value2 = array.GetValue(i);
				Type argumentType2 = ((elementType == typeof(object) && value2 != null) ? value2.GetType() : elementType);
				array2[i] = new CustomAttributeTypedArgument(argumentType2, value2);
			}
			Value = new ReadOnlyCollection<CustomAttributeTypedArgument>(array2);
		}
	}

	/// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
	/// <param name="obj">The object to compare with the current instance. </param>
	public override bool Equals(object obj)
	{
		return obj == (object)this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are equivalent.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the left of the equality operator.</param>
	/// <param name="right">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the right of the equality operator.</param>
	public static bool operator ==(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
	{
		return left.Equals(right);
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are different.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the left of the inequality operator.</param>
	/// <param name="right">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the right of the inequality operator.</param>
	public static bool operator !=(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a string consisting of the argument name, the equal sign, and a string representation of the argument value.</summary>
	/// <returns>A string consisting of the argument name, the equal sign, and a string representation of the argument value.</returns>
	public override string ToString()
	{
		return ToString(typed: false);
	}

	internal string ToString(bool typed)
	{
		if (ArgumentType == null)
		{
			return base.ToString();
		}
		try
		{
			if (ArgumentType.IsEnum)
			{
				return string.Format(CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", Value, ArgumentType.FullNameOrDefault);
			}
			if (Value == null)
			{
				return string.Format(CultureInfo.CurrentCulture, typed ? "null" : "({0})null", ArgumentType.NameOrDefault);
			}
			if (ArgumentType == typeof(string))
			{
				return string.Format(CultureInfo.CurrentCulture, "\"{0}\"", Value);
			}
			if (ArgumentType == typeof(char))
			{
				return string.Format(CultureInfo.CurrentCulture, "'{0}'", Value);
			}
			if (ArgumentType == typeof(Type))
			{
				return string.Format(CultureInfo.CurrentCulture, "typeof({0})", ((Type)Value).FullNameOrDefault);
			}
			if (ArgumentType.IsArray)
			{
				string text = null;
				IList<CustomAttributeTypedArgument> list = Value as IList<CustomAttributeTypedArgument>;
				Type elementType = ArgumentType.GetElementType();
				text = string.Format(CultureInfo.CurrentCulture, "new {0}[{1}] {{ ", elementType.IsEnum ? elementType.FullNameOrDefault : elementType.NameOrDefault, list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					text += string.Format(CultureInfo.CurrentCulture, (i == 0) ? "{0}" : ", {0}", list[i].ToString(elementType != typeof(object)));
				}
				return text += " }";
			}
			return string.Format(CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", Value, ArgumentType.NameOrDefault);
		}
		catch (MissingMetadataException)
		{
			return base.ToString();
		}
	}

	private static object CanonicalizeValue(object value)
	{
		if (value.GetType().IsEnum)
		{
			return ((Enum)value).GetValue();
		}
		return value;
	}
}
