using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the minimum and maximum length of characters that are allowed in a data field.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class StringLengthAttribute : ValidationAttribute
{
	/// <summary>Gets or sets the maximum length of a string.</summary>
	/// <returns>The maximum length a string. </returns>
	public int MaximumLength { get; private set; }

	/// <summary>Gets or sets the minimum length of a string.</summary>
	/// <returns>The minimum length of a string.</returns>
	public int MinimumLength { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.StringLengthAttribute" /> class by using a specified maximum length.</summary>
	/// <param name="maximumLength">The maximum length of a string. </param>
	public StringLengthAttribute(int maximumLength)
		: base(() => "The field {0} must be a string with a maximum length of {1}.")
	{
		MaximumLength = maximumLength;
	}

	/// <summary>Determines whether a specified object is valid.</summary>
	/// <returns>true if the specified object is valid; otherwise, false.</returns>
	/// <param name="value">The object to validate.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumLength" /> is negative.-or-<paramref name="maximumLength" /> is less than <see cref="P:System.ComponentModel.DataAnnotations.StringLengthAttribute.MinimumLength" />.</exception>
	public override bool IsValid(object value)
	{
		EnsureLegalLengths();
		int num = ((value != null) ? ((string)value).Length : 0);
		if (value != null)
		{
			if (num >= MinimumLength)
			{
				return num <= MaximumLength;
			}
			return false;
		}
		return true;
	}

	/// <summary>Applies formatting to a specified error message.</summary>
	/// <returns>The formatted error message.</returns>
	/// <param name="name">The name of the field that caused the validation failure.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maximumLength" /> is negative. -or-<paramref name="maximumLength" /> is less than <paramref name="minimumLength" />.</exception>
	public override string FormatErrorMessage(string name)
	{
		EnsureLegalLengths();
		string format = ((MinimumLength != 0 && !base.CustomErrorMessageSet) ? "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}." : base.ErrorMessageString);
		return string.Format(CultureInfo.CurrentCulture, format, name, MaximumLength, MinimumLength);
	}

	private void EnsureLegalLengths()
	{
		if (MaximumLength < 0)
		{
			throw new InvalidOperationException("The maximum length must be a nonnegative integer.");
		}
		if (MaximumLength < MinimumLength)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The maximum value '{0}' must be greater than or equal to the minimum value '{1}'.", MaximumLength, MinimumLength));
		}
	}
}
