namespace Manatee.Trello.Internal.Validation;

internal class OldValueNotNullOrWhiteSpaceRule : IValidationRule<string>
{
	public static OldValueNotNullOrWhiteSpaceRule Instance { get; }

	static OldValueNotNullOrWhiteSpaceRule()
	{
		Instance = new OldValueNotNullOrWhiteSpaceRule();
	}

	private OldValueNotNullOrWhiteSpaceRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if (!oldValue.IsNullOrWhiteSpace())
		{
			return null;
		}
		return "Value cannot be set unless it already has a value.";
	}
}
