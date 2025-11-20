namespace Manatee.Trello;

public interface INotificationData
{
	IAttachment Attachment { get; }

	IBoard Board { get; }

	IBoard BoardSource { get; }

	IBoard BoardTarget { get; }

	ICard Card { get; }

	ICard CardSource { get; }

	ICheckItem CheckItem { get; }

	ICheckList CheckList { get; }

	IList List { get; }

	IList ListAfter { get; }

	IList ListBefore { get; }

	IMember Member { get; }

	string OldDescription { get; }

	IList OldList { get; }

	Position OldPosition { get; }

	string OldText { get; }

	IOrganization Organization { get; }

	string Text { get; set; }

	bool? WasArchived { get; }
}
