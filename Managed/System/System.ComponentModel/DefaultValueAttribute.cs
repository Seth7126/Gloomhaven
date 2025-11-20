using System.Globalization;
using System.Threading;

namespace System.ComponentModel;

/// <summary>Specifies the default value for a property.</summary>
[AttributeUsage(AttributeTargets.All)]
public class DefaultValueAttribute : Attribute
{
	private object _value;

	private static object s_convertFromInvariantString;

	/// <summary>Gets the default value of the property this attribute is bound to.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the default value of the property this attribute is bound to.</returns>
	public virtual object Value => _value;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class, converting the specified value to the specified type, and using an invariant culture as the translation context.</summary>
	/// <param name="type">A <see cref="T:System.Type" /> that represents the type to convert the value to. </param>
	/// <param name="value">A <see cref="T:System.String" /> that can be converted to the type using the <see cref="T:System.ComponentModel.TypeConverter" /> for the type and the U.S. English culture. </param>
	public DefaultValueAttribute(Type type, string value)
	{
		try
		{
			if (TryConvertFromInvariantString(type, value, out var conversionResult))
			{
				_value = conversionResult;
			}
			else if (type.IsSubclassOf(typeof(Enum)))
			{
				_value = Enum.Parse(type, value, ignoreCase: true);
			}
			else if (type == typeof(TimeSpan))
			{
				_value = TimeSpan.Parse(value);
			}
			else
			{
				_value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}
			static bool TryConvertFromInvariantString(Type typeToConvert, string stringValue, out object reference)
			{
				reference = null;
				if (s_convertFromInvariantString == null)
				{
					Type type2 = Type.GetType("System.ComponentModel.TypeDescriptor, System.ComponentModel.TypeConverter", throwOnError: false);
					Volatile.Write(ref s_convertFromInvariantString, (type2 == null) ? new object() : Delegate.CreateDelegate(typeof(Func<Type, string, object>), type2, "ConvertFromInvariantString", ignoreCase: false));
				}
				if (!(s_convertFromInvariantString is Func<Type, string, object> func))
				{
					return false;
				}
				reference = func(typeToConvert, stringValue);
				return true;
			}
		}
		catch
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a Unicode character.</summary>
	/// <param name="value">A Unicode character that is the default value. </param>
	public DefaultValueAttribute(char value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using an 8-bit unsigned integer.</summary>
	/// <param name="value">An 8-bit unsigned integer that is the default value. </param>
	public DefaultValueAttribute(byte value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 16-bit signed integer.</summary>
	/// <param name="value">A 16-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(short value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 32-bit signed integer.</summary>
	/// <param name="value">A 32-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(int value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a 64-bit signed integer.</summary>
	/// <param name="value">A 64-bit signed integer that is the default value. </param>
	public DefaultValueAttribute(long value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a single-precision floating point number.</summary>
	/// <param name="value">A single-precision floating point number that is the default value. </param>
	public DefaultValueAttribute(float value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a double-precision floating point number.</summary>
	/// <param name="value">A double-precision floating point number that is the default value. </param>
	public DefaultValueAttribute(double value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a <see cref="T:System.Boolean" /> value.</summary>
	/// <param name="value">A <see cref="T:System.Boolean" /> that is the default value. </param>
	public DefaultValueAttribute(bool value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class using a <see cref="T:System.String" />.</summary>
	/// <param name="value">A <see cref="T:System.String" /> that is the default value. </param>
	public DefaultValueAttribute(string value)
	{
		_value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DefaultValueAttribute" /> class.</summary>
	/// <param name="value">An <see cref="T:System.Object" /> that represents the default value. </param>
	public DefaultValueAttribute(object value)
	{
		_value = value;
	}

	[CLSCompliant(false)]
	public DefaultValueAttribute(sbyte value)
	{
		_value = value;
	}

	[CLSCompliant(false)]
	public DefaultValueAttribute(ushort value)
	{
		_value = value;
	}

	[CLSCompliant(false)]
	public DefaultValueAttribute(uint value)
	{
		_value = value;
	}

	[CLSCompliant(false)]
	public DefaultValueAttribute(ulong value)
	{
		_value = value;
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.DefaultValueAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is DefaultValueAttribute defaultValueAttribute)
		{
			if (Value != null)
			{
				return Value.Equals(defaultValueAttribute.Value);
			}
			return defaultValueAttribute.Value == null;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Sets the default value for the property to which this attribute is bound.</summary>
	/// <param name="value">The default value.</param>
	protected void SetValue(object value)
	{
		_value = value;
	}
}
