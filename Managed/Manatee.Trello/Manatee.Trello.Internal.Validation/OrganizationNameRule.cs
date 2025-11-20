using System.Linq;
using System.Text.RegularExpressions;

namespace Manatee.Trello.Internal.Validation;

internal class OrganizationNameRule : IValidationRule<string>
{
	private static readonly Regex Regex;

	public static OrganizationNameRule Instance { get; }

	static OrganizationNameRule()
	{
		Regex = new Regex("^[a-z0-9_]{3,}$");
		Instance = new OrganizationNameRule();
	}

	private OrganizationNameRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		bool flag = Regex.IsMatch(newValue);
		if (flag)
		{
			Search search = new Search(newValue, 10, SearchModelType.Organizations);
			flag &= search.Organizations == null || search.Organizations.All((IOrganization o) => o.Name != newValue);
		}
		if (flag)
		{
			return null;
		}
		return "Value must consist of at least three lowercase letters, number, or underscores and must be unique on Trello.";
	}
}
