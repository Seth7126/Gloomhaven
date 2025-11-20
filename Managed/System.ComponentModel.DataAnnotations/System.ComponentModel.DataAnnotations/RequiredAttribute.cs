namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that a data field value is required.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RequiredAttribute : ValidationAttribute
{
	/// <summary>Gets or sets a value that indicates whether an empty string is allowed.</summary>
	/// <returns>true if an empty string is allowed; otherwise, false. The default value is false.</returns>
	public bool AllowEmptyStrings { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.RequiredAttribute" /> class.</summary>
	public RequiredAttribute()
		: base(() => "The {0} field is required.")
	{
	}

	/// <summary>Checks that the value of the required data field is not empty.</summary>
	/// <returns>true if validation is successful; otherwise, false.</returns>
	/// <param name="value">The data field value to validate.</param>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The data field value was null.</exception>
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value is string text && !AllowEmptyStrings)
		{
			return text.Trim().Length != 0;
		}
		return true;
	}
}
