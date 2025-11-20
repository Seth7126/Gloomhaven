using System.Text.RegularExpressions;

namespace Manatee.Trello.Internal.Validation;

internal class IdRule : IValidationRule<string>
{
	private static readonly Regex Regex;

	public static IdRule Instance { get; }

	static IdRule()
	{
		Regex = new Regex("^[a-z0-9]{24}$", RegexOptions.IgnoreCase);
		Instance = new IdRule();
	}

	private IdRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if (oldValue == null || !Regex.IsMatch(oldValue))
		{
			return string.Empty;
		}
		return null;
	}
}
