namespace InControl;

public readonly struct MoveActionModel
{
	public TwoAxisInputControl TwoAxisInputControl { get; }

	public MoveActionSourceType SourceType { get; }

	public MoveActionModel(TwoAxisInputControl twoAxisInputControl, MoveActionSourceType sourceType)
	{
		TwoAxisInputControl = twoAxisInputControl;
		SourceType = sourceType;
	}
}
