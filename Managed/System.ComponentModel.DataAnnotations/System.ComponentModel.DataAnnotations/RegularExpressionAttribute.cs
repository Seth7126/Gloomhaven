using System.Globalization;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that a data field value in ASP.NET Dynamic Data must match the specified regular expression.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RegularExpressionAttribute : ValidationAttribute
{
	private int _matchTimeoutInMilliseconds;

	private bool _matchTimeoutSet;

	/// <summary>Gets the regular expression pattern.</summary>
	/// <returns>The pattern to match.</returns>
	public string Pattern { get; private set; }

	public int MatchTimeoutInMilliseconds
	{
		get
		{
			return _matchTimeoutInMilliseconds;
		}
		set
		{
			_matchTimeoutInMilliseconds = value;
			_matchTimeoutSet = true;
		}
	}

	private Regex Regex { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.RegularExpressionAttribute" /> class.</summary>
	/// <param name="pattern">The regular expression that is used to validate the data field value. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pattern" /> is null.</exception>
	public RegularExpressionAttribute(string pattern)
		: base(() => "The field {0} must match the regular expression '{1}'.")
	{
		Pattern = pattern;
	}

	/// <summary>Checks whether the value entered by the user matches the regular expression pattern. </summary>
	/// <returns>true if validation is successful; otherwise, false.</returns>
	/// <param name="value">The data field value to validate.</param>
	/// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The data field value did not match the regular expression pattern.</exception>
	public override bool IsValid(object value)
	{
		SetupRegex();
		string text = Convert.ToString(value, CultureInfo.CurrentCulture);
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		Match match = Regex.Match(text);
		if (match.Success && match.Index == 0)
		{
			return match.Length == text.Length;
		}
		return false;
	}

	/// <summary>Formats the error message to display if the regular expression validation fails.</summary>
	/// <returns>The formatted error message.</returns>
	/// <param name="name">The name of the field that caused the validation failure.</param>
	public override string FormatErrorMessage(string name)
	{
		SetupRegex();
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, Pattern);
	}

	private void SetupRegex()
	{
		if (Regex == null)
		{
			if (string.IsNullOrEmpty(Pattern))
			{
				throw new InvalidOperationException("The pattern must be set to a valid regular expression.");
			}
			if (!_matchTimeoutSet)
			{
				MatchTimeoutInMilliseconds = GetDefaultTimeout();
			}
			Regex regex3;
			if (MatchTimeoutInMilliseconds != -1)
			{
				Regex regex = (Regex = new Regex(Pattern, RegexOptions.None, TimeSpan.FromMilliseconds(MatchTimeoutInMilliseconds)));
				regex3 = regex;
			}
			else
			{
				regex3 = new Regex(Pattern);
			}
			Regex = regex3;
		}
	}

	private static int GetDefaultTimeout()
	{
		return 2000;
	}
}
