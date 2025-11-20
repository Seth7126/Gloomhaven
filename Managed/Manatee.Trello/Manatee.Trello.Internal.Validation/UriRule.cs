using System;

namespace Manatee.Trello.Internal.Validation;

internal class UriRule : IValidationRule<string>
{
	public static UriRule Instance { get; }

	static UriRule()
	{
		Instance = new UriRule();
	}

	private UriRule()
	{
	}

	public string Validate(string oldValue, string newValue)
	{
		if ((newValue.BeginsWith("http://") || newValue.BeginsWith("https://")) && Uri.IsWellFormedUriString(newValue, UriKind.Absolute))
		{
			return null;
		}
		return "Value must begin with \"http://\" or \"https://\" and be a valid URI.";
	}
}
