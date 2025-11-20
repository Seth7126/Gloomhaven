using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies that a data field value is a  well-formed phone number using a regular expression for phone numbers.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class PhoneAttribute : DataTypeAttribute
{
	private static Regex _regex = CreateRegEx();

	private const string _additionalPhoneNumberCharacters = "-.()";

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.PhoneAttribute" /> class.</summary>
	public PhoneAttribute()
		: base(DataType.PhoneNumber)
	{
		base.DefaultErrorMessage = "The {0} field is not a valid phone number.";
	}

	/// <summary>Determines whether the specified phone number is in a valid phone number format. </summary>
	/// <returns>true if the phone number is valid; otherwise, false.</returns>
	/// <param name="value">The value to validate.</param>
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return true;
		}
		string text = value as string;
		if (_regex != null)
		{
			if (text != null)
			{
				return _regex.Match(text).Length > 0;
			}
			return false;
		}
		if (text == null)
		{
			return false;
		}
		text = text.Replace("+", string.Empty).TrimEnd();
		text = RemoveExtension(text);
		bool flag = false;
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			if (char.IsDigit(text2[i]))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		text2 = text;
		foreach (char c in text2)
		{
			if (!char.IsDigit(c) && !char.IsWhiteSpace(c) && "-.()".IndexOf(c) == -1)
			{
				return false;
			}
		}
		return true;
	}

	private static Regex CreateRegEx()
	{
		if (AppSettings.DisableRegEx)
		{
			return null;
		}
		TimeSpan matchTimeout = TimeSpan.FromSeconds(2.0);
		try
		{
			if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
			{
				return new Regex("^(\\+\\s?)?((?<!\\+.*)\\(\\+?\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+)([\\s\\-\\.]?(\\(\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+))*(\\s?(x|ext\\.?)\\s?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, matchTimeout);
			}
		}
		catch
		{
		}
		return new Regex("^(\\+\\s?)?((?<!\\+.*)\\(\\+?\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+)([\\s\\-\\.]?(\\(\\d+([\\s\\-\\.]?\\d+)?\\)|\\d+))*(\\s?(x|ext\\.?)\\s?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
	}

	private static string RemoveExtension(string potentialPhoneNumber)
	{
		int num = potentialPhoneNumber.LastIndexOf("ext.", StringComparison.InvariantCultureIgnoreCase);
		if (num >= 0 && MatchesExtension(potentialPhoneNumber.Substring(num + 4)))
		{
			return potentialPhoneNumber.Substring(0, num);
		}
		num = potentialPhoneNumber.LastIndexOf("ext", StringComparison.InvariantCultureIgnoreCase);
		if (num >= 0 && MatchesExtension(potentialPhoneNumber.Substring(num + 3)))
		{
			return potentialPhoneNumber.Substring(0, num);
		}
		num = potentialPhoneNumber.LastIndexOf("x", StringComparison.InvariantCultureIgnoreCase);
		if (num >= 0 && MatchesExtension(potentialPhoneNumber.Substring(num + 1)))
		{
			return potentialPhoneNumber.Substring(0, num);
		}
		return potentialPhoneNumber;
	}

	private static bool MatchesExtension(string potentialExtension)
	{
		potentialExtension = potentialExtension.TrimStart();
		if (potentialExtension.Length == 0)
		{
			return false;
		}
		string text = potentialExtension;
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsDigit(text[i]))
			{
				return false;
			}
		}
		return true;
	}
}
