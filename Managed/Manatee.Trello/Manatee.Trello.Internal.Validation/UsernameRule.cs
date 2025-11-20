using System.Linq;
using System.Text.RegularExpressions;

namespace Manatee.Trello.Internal.Validation;

internal class UsernameRule : IValidationRule<string>
{
	private static readonly Regex Regex;

	public static UsernameRule Instance { get; }

	static UsernameRule()
	{
		Regex = new Regex("^[a-z0-9_]{3,}$");
		Instance = new UsernameRule();
	}

	private UsernameRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		bool flag = Regex.IsMatch(newValue);
		if (flag)
		{
			MemberSearch memberSearch = new MemberSearch(newValue);
			flag &= memberSearch.Results == null || memberSearch.Results.All((MemberSearchResult o) => o.Member.UserName != newValue);
		}
		if (flag)
		{
			return null;
		}
		return "Value must consist of at least three lowercase letters, number, or underscores and must be unique on Trello.";
	}
}
