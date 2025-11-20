using System.Text.RegularExpressions;

namespace Manatee.Trello.Internal.Validation;

internal class MemberInitialsRule : IValidationRule<string>
{
	private static readonly Regex Regex;

	public static MemberInitialsRule Instance { get; private set; }

	static MemberInitialsRule()
	{
		Regex = new Regex("^(\\S.{0,2}\\S)|\\S$");
		Instance = new MemberInitialsRule();
	}

	private MemberInitialsRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if (Regex.IsMatch(newValue))
		{
			return null;
		}
		return "Value must consist of between one and three characters and cannot begin or end with whitespace.";
	}
}
