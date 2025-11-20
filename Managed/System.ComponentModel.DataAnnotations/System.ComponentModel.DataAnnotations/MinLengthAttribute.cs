using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the minimum length of array of string data allowed in a property.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MinLengthAttribute : ValidationAttribute
{
	/// <summary>Gets or sets the minimum allowable length of the array or string data.</summary>
	/// <returns>The minimum allowable length of the array or string data.</returns>
	public int Length { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.MinLengthAttribute" /> class.</summary>
	/// <param name="length">The length of the array or string data.</param>
	public MinLengthAttribute(int length)
		: base("The field {0} must be a string or array type with a minimum length of '{1}'.")
	{
		Length = length;
	}

	/// <summary>Determines whether a specified object is valid.</summary>
	/// <returns>true if the specified object is valid; otherwise, false.</returns>
	/// <param name="value">The object to validate.</param>
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
		return num >= Length;
	}

	/// <summary>Applies formatting to a specified error message.</summary>
	/// <returns>A localized string to describe the minimum acceptable length.</returns>
	/// <param name="name">The name to include in the formatted string.</param>
	public override string FormatErrorMessage(string name)
	{
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, Length);
	}

	private void EnsureLegalLengths()
	{
		if (Length < 0)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "MinLengthAttribute must have a Length value that is zero or greater."));
		}
	}
}
