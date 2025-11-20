namespace Manatee.Trello.Internal.Validation;

internal class RequireCommentRule : IValidationRule<IAction>
{
	public static RequireCommentRule Instance { get; }

	static RequireCommentRule()
	{
		Instance = new RequireCommentRule();
	}

	private RequireCommentRule()
	{
	}

	public string Validate(IAction oldValue, IAction newValue)
	{
		if (!(newValue.Type != ActionType.CommentCard))
		{
			return null;
		}
		return "Action must be a comment";
	}
}
