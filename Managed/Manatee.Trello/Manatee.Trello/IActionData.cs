using System;

namespace Manatee.Trello;

public interface IActionData
{
	IAttachment Attachment { get; }

	IBoard Board { get; }

	IBoard BoardSource { get; }

	IBoard BoardTarget { get; }

	ICard Card { get; }

	ICard CardSource { get; }

	ICheckItem CheckItem { get; }

	ICheckList CheckList { get; }

	ICustomFieldDefinition CustomField { get; }

	ILabel Label { get; }

	DateTime? LastEdited { get; }

	IList List { get; }

	IList ListAfter { get; }

	IList ListBefore { get; }

	IMember Member { get; }

	string OldDescription { get; }

	IList OldList { get; }

	Position OldPosition { get; }

	string OldText { get; }

	IOrganization Organization { get; }

	IPowerUp PowerUp { get; }

	string Text { get; set; }

	bool? WasArchived { get; }

	string Value { get; }
}
