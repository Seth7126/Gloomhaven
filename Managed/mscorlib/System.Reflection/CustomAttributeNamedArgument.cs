using System.Globalization;
using Internal.Runtime.Augments;

namespace System.Reflection;

/// <summary>Represents a named argument of a custom attribute in the reflection-only context.</summary>
public struct CustomAttributeNamedArgument
{
	private readonly Type _attributeType;

	private volatile MemberInfo _lazyMemberInfo;

	/// <summary>Gets a <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure that can be used to obtain the type and value of the current named argument.</summary>
	/// <returns>A structure that can be used to obtain the type and value of the current named argument.</returns>
	public CustomAttributeTypedArgument TypedValue { get; }

	/// <summary>Gets a value that indicates whether the named argument is a field.</summary>
	/// <returns>true if the named argument is a field; otherwise, false.</returns>
	public bool IsField { get; }

	/// <summary>Gets the name of the attribute member that would be used to set the named argument.</summary>
	/// <returns>The name of the attribute member that would be used to set the named argument.</returns>
	public string MemberName { get; }

	/// <summary>Gets the attribute member that would be used to set the named argument.</summary>
	/// <returns>The attribute member that would be used to set the named argument.</returns>
	public MemberInfo MemberInfo
	{
		get
		{
			MemberInfo memberInfo = _lazyMemberInfo;
			if (memberInfo == null)
			{
				memberInfo = ((!IsField) ? ((MemberInfo)_attributeType.GetProperty(MemberName, BindingFlags.Instance | BindingFlags.Public)) : ((MemberInfo)_attributeType.GetField(MemberName, BindingFlags.Instance | BindingFlags.Public)));
				if (memberInfo == null)
				{
					throw RuntimeAugments.Callbacks.CreateMissingMetadataException(_attributeType);
				}
				_lazyMemberInfo = memberInfo;
			}
			return memberInfo;
		}
	}

	internal CustomAttributeNamedArgument(Type attributeType, string memberName, bool isField, CustomAttributeTypedArgument typedValue)
	{
		IsField = isField;
		MemberName = memberName;
		TypedValue = typedValue;
		_attributeType = attributeType;
		_lazyMemberInfo = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> class, which represents the specified field or property of the custom attribute, and specifies the value of the field or property.</summary>
	/// <param name="memberInfo">A field or property of the custom attribute. The new <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> object represents this member and its value.</param>
	/// <param name="value">The value of the field or property of the custom attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="memberInfo" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="memberInfo" /> is not a field or property of the custom attribute.</exception>
	public CustomAttributeNamedArgument(MemberInfo memberInfo, object value)
	{
		if (memberInfo == null)
		{
			throw new ArgumentNullException("memberInfo");
		}
		Type type = null;
		FieldInfo fieldInfo = memberInfo as FieldInfo;
		PropertyInfo propertyInfo = memberInfo as PropertyInfo;
		if (fieldInfo != null)
		{
			type = fieldInfo.FieldType;
		}
		else
		{
			if (!(propertyInfo != null))
			{
				throw new ArgumentException("The member must be either a field or a property.");
			}
			type = propertyInfo.PropertyType;
		}
		_lazyMemberInfo = memberInfo;
		_attributeType = memberInfo.DeclaringType;
		if (value is CustomAttributeTypedArgument customAttributeTypedArgument)
		{
			TypedValue = customAttributeTypedArgument;
		}
		else
		{
			TypedValue = new CustomAttributeTypedArgument(type, value);
		}
		IsField = fieldInfo != null;
		MemberName = memberInfo.Name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> class, which represents the specified field or property of the custom attribute, and specifies a <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> object that describes the type and value of the field or property.</summary>
	/// <param name="memberInfo">A field or property of the custom attribute. The new <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> object represents this member and its value.</param>
	/// <param name="typedArgument">An object that describes the type and value of the field or property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="memberInfo" /> is null.</exception>
	public CustomAttributeNamedArgument(MemberInfo memberInfo, CustomAttributeTypedArgument typedArgument)
	{
		if (memberInfo == null)
		{
			throw new ArgumentNullException("memberInfo");
		}
		_lazyMemberInfo = memberInfo;
		_attributeType = memberInfo.DeclaringType;
		TypedValue = typedArgument;
		IsField = memberInfo is FieldInfo;
		MemberName = memberInfo.Name;
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		return obj == (object)this;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are equivalent.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The structure to the left of the equality operator.</param>
	/// <param name="right">The structure to the right of the equality operator.</param>
	public static bool operator ==(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
	{
		return left.Equals(right);
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are different.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The structure to the left of the inequality operator.</param>
	/// <param name="right">The structure to the right of the inequality operator.</param>
	public static bool operator !=(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a string that consists of the argument name, the equal sign, and a string representation of the argument value.</summary>
	/// <returns>A string that consists of the argument name, the equal sign, and a string representation of the argument value.</returns>
	public override string ToString()
	{
		if (_attributeType == null)
		{
			return base.ToString();
		}
		try
		{
			bool flag = _lazyMemberInfo == null || (IsField ? ((FieldInfo)_lazyMemberInfo).FieldType : ((PropertyInfo)_lazyMemberInfo).PropertyType) != typeof(object);
			return string.Format(CultureInfo.CurrentCulture, "{0} = {1}", MemberName, TypedValue.ToString(flag));
		}
		catch (MissingMetadataException)
		{
			return base.ToString();
		}
	}
}
