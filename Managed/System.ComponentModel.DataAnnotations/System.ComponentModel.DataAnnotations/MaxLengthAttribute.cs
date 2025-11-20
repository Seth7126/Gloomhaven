using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the maximum length of array or string data allowed in a property.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MaxLengthAttribute : ValidationAttribute
{
	private const int MaxAllowableLength = -1;

	/// <summary>Gets the maximum allowable length of the array or string data.</summary>
	/// <returns>The maximum allowable length of the array or string data.</returns>
	public int Length { get; private set; }

	private static string DefaultErrorMessageString => "The field {0} must be a string or array type with a maximum length of '{1}'.";

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.MaxLengthAttribute" /> class based on the <paramref name="length" /> parameter.</summary>
	/// <param name="length">The maximum allowable length of array or string data.</param>
	public MaxLengthAttribute(int length)
		: base(() => DefaultErrorMessageString)
	{
		Length = length;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.MaxLengthAttribute" /> class.</summary>
	public MaxLengthAttribute()
		: base(() => DefaultErrorMessageString)
	{
		Length = -1;
	}

	/// <summary>Determines whether a specified object is valid.</summary>
	/// <returns>true if the value is null, or if the value is less than or equal to the specified maximum length; otherwise, false.</returns>
	/// <param name="value">The object to validate.</param>
	/// <exception cref="Sytem.InvalidOperationException">Length is zero or less than negative one.</exception>
	public override bool IsValid(object value)
	{
		EnsureLegalLengths();
		int num = 0;
		if (value == null)
		{
			return true;
		}
		if (value is string text)
		{
			num = text.Length;
		}
		else
		{
			if (!CountPropertyHelper.TryGetCount(value, out var count))
			{
				throw new InvalidCastException($"The field of type {value.GetType()} must be a string, array or ICollection type.");
			}
			num = count;
		}
		if (-1 != Length)
		{
			return num <= Length;
		}
		return true;
	}

	/// <summary>Applies formatting to a specified error message.</summary>
	/// <returns>A localized string to describe the maximum acceptable length.</returns>
	/// <param name="name">The name to include in the formatted string.</param>
	public override string FormatErrorMessage(string name)
	{
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, Length);
	}

	private void EnsureLegalLengths()
	{
		if (Length == 0 || Length < -1)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "MaxLengthAttribute must have a Length value that is greater than zero. Use MaxLength() without parameters to indicate that the string or array can have the maximum allowable length."));
		}
	}
}
