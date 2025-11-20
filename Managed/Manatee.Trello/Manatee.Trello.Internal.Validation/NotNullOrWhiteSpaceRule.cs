namespace Manatee.Trello.Internal.Validation;

internal class NotNullOrWhiteSpaceRule : IValidationRule<string>
{
	public static NotNullOrWhiteSpaceRule Instance { get; }

	static NotNullOrWhiteSpaceRule()
	{
		Instance = new NotNullOrWhiteSpaceRule();
	}

	private NotNullOrWhiteSpaceRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if (!newValue.IsNullOrWhiteSpace())
		{
			return null;
		}
		return "Value cannot be null, empty, or whitespace.";
	}
}
