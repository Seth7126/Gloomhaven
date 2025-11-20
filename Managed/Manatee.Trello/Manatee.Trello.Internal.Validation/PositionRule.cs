namespace Manatee.Trello.Internal.Validation;

internal class PositionRule : IValidationRule<Position>
{
	public static PositionRule Instance { get; }

	static PositionRule()
	{
		Instance = new PositionRule();
	}

	private PositionRule()
	{
	}

	public string Validate(Position oldValue, Position newValue)
	{
		if (!(newValue == null) && newValue.IsValid)
		{
			return null;
		}
		return "Value must be non-null and positive.";
	}
}
