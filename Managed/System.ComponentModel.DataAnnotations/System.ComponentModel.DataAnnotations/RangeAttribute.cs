using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the numeric range constraints for the value of a data field. </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RangeAttribute : ValidationAttribute
{
	/// <summary>Gets the minimum allowed field value.</summary>
	/// <returns>The minimu value that is allowed for the data field.</returns>
	public object Minimum { get; private set; }

	/// <summary>Gets the maximum allowed field value.</summary>
	/// <returns>The maximum value that is allowed for the data field.</returns>
	public object Maximum { get; private set; }

	/// <summary>Gets the type of the data field whose value must be validated.</summary>
	/// <returns>The type of the data field whose value must be validated.</returns>
	public Type OperandType { get; private set; }

	private Func<object, object> Conversion { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.RangeAttribute" /> class by using the specified minimum and maximum values.</summary>
	/// <param name="minimum">Specifies the minimum value allowed for the data field value.</param>
	/// <param name="maximum">Specifies the maximum value allowed for the data field value.</param>
	public RangeAttribute(int minimum, int maximum)
		: this()
	{
		Minimum = minimum;
		Maximum = maximum;
		OperandType = typeof(int);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.RangeAttribute" /> class by using the specified minimum and maximum values. </summary>
	/// <param name="minimum">Specifies the minimum value allowed for the data field value.</param>
	/// <param name="maximum">Specifies the maximum value allowed for the data field value.</param>
	public RangeAttribute(double minimum, double maximum)
		: this()
	{
		Minimum = minimum;
		Maximum = maximum;
		OperandType = typeof(double);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.RangeAttribute" /> class by using the specified minimum and maximum values and the specific type.</summary>
	/// <param name="type">Specifies the type of the object to test.</param>
	/// <param name="minimum">Specifies the minimum value allowed for the data field value.</param>
	/// <param name="maximum">Specifies the maximum value allowed for the data field value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public RangeAttribute(Type type, string minimum, string maximum)
		: this()
	{
		OperandType = type;
		Minimum = minimum;
		Maximum = maximum;
	}

	private RangeAttribute()
		: base(() => "The field {0} must be between {1} and {2}.")
	{
	}

	private void Initialize(IComparable minimum, IComparable maximum, Func<object, object> conversion)
	{
		if (minimum.CompareTo(maximum) > 0)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The maximum value '{0}' must be greater than or equal to the minimum value '{1}'.", maximum, minimum));
		}
		Minimum = minimum;
		Maximum = maximum;
		Conversion = conversion;
	}

	/// <summary>Checks that the value of the data field is in the specified range.</summary>
	/// <returns>true if the specified value is in the range; otherwise, false.</returns>
	/// <param name="value">The data field value to validate.</param>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The data field value was outside the allowed range.</exception>
	public override bool IsValid(object value)
	{
		SetupConversion();
		if (value == null)
		{
			return true;
		}
		if (value is string value2 && string.IsNullOrEmpty(value2))
		{
			return true;
		}
		object obj = null;
		try
		{
			obj = Conversion(value);
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
		IComparable obj2 = (IComparable)Minimum;
		IComparable comparable = (IComparable)Maximum;
		if (obj2.CompareTo(obj) <= 0)
		{
			return comparable.CompareTo(obj) >= 0;
		}
		return false;
	}

	/// <summary>Formats the error message that is displayed when range validation fails.</summary>
	/// <returns>The formatted error message.</returns>
	/// <param name="name">The name of the field that caused the validation failure. </param>
	public override string FormatErrorMessage(string name)
	{
		SetupConversion();
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, Minimum, Maximum);
	}

	private void SetupConversion()
	{
		if (Conversion != null)
		{
			return;
		}
		object minimum = Minimum;
		object maximum = Maximum;
		if (minimum == null || maximum == null)
		{
			throw new InvalidOperationException("The minimum and maximum values must be set.");
		}
		Type type = minimum.GetType();
		if (type == typeof(int))
		{
			Initialize((int)minimum, (int)maximum, (object v) => Convert.ToInt32(v, CultureInfo.InvariantCulture));
			return;
		}
		if (type == typeof(double))
		{
			Initialize((double)minimum, (double)maximum, (object v) => Convert.ToDouble(v, CultureInfo.InvariantCulture));
			return;
		}
		Type type2 = OperandType;
		if (type2 == null)
		{
			throw new InvalidOperationException("The OperandType must be set when strings are used for minimum and maximum values.");
		}
		Type typeFromHandle = typeof(IComparable);
		if (!typeFromHandle.IsAssignableFrom(type2))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The type {0} must implement {1}.", type2.FullName, typeFromHandle.FullName));
		}
		TypeConverter converter = TypeDescriptor.GetConverter(type2);
		IComparable minimum2 = (IComparable)converter.ConvertFromString((string)minimum);
		IComparable maximum2 = (IComparable)converter.ConvertFromString((string)maximum);
		Func<object, object> conversion = (object value) => (value == null || !(value.GetType() == type2)) ? converter.ConvertFrom(value) : value;
		Initialize(minimum2, maximum2, conversion);
	}
}
