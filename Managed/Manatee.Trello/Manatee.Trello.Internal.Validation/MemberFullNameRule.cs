using System.Text.RegularExpressions;

namespace Manatee.Trello.Internal.Validation;

internal class MemberFullNameRule : IValidationRule<string>
{
	private static readonly Regex Regex;

	public static MemberFullNameRule Instance { get; private set; }

	static MemberFullNameRule()
	{
		Regex = new Regex("^(\\S.{2,}\\S)|\\S$");
		Instance = new MemberFullNameRule();
	}

	private MemberFullNameRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if (Regex.IsMatch(newValue))
		{
			return null;
		}
		return "Value must consist of at least four characters and cannot begin or end with whitespace.";
	}
}
