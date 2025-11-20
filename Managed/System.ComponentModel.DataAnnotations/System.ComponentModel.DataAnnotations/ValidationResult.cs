using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Represents a container for the results of a validation request.</summary>
public class ValidationResult
{
	private IEnumerable<string> _memberNames;

	private string _errorMessage;

	/// <summary>Represents the success of the validation (true if validation was successful; otherwise, false).</summary>
	public static readonly ValidationResult Success;

	/// <summary>Gets the collection of member names that indicate which fields have validation errors.</summary>
	/// <returns>The collection of member names that indicate which fields have validation errors.</returns>
	public IEnumerable<string> MemberNames => _memberNames;

	/// <summary>Gets the error message for the validation.</summary>
	/// <returns>The error message for the validation.</returns>
	public string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		set
		{
			_errorMessage = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using an error message.</summary>
	/// <param name="errorMessage">The error message.</param>
	public ValidationResult(string errorMessage)
		: this(errorMessage, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using an error message and a list of members that have validation errors.</summary>
	/// <param name="errorMessage">The error message.</param>
	/// <param name="memberNames">The list of member names that have validation errors.</param>
	public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
	{
		_errorMessage = errorMessage;
		_memberNames = memberNames ?? new string[0];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using a <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> object.</summary>
	/// <param name="validationResult">The validation result object.</param>
	protected ValidationResult(ValidationResult validationResult)
	{
		if (validationResult == null)
		{
			throw new ArgumentNullException("validationResult");
		}
		_errorMessage = validationResult._errorMessage;
		_memberNames = validationResult._memberNames;
	}

	/// <summary>Returns a string representation of the current validation result.</summary>
	/// <returns>The current validation result.</returns>
	public override string ToString()
	{
		return ErrorMessage ?? base.ToString();
	}
}
