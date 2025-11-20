using System;

namespace Manatee.Trello;

public interface IBadges
{
	int? Attachments { get; }

	int? CheckItems { get; }

	int? CheckItemsChecked { get; }

	int? Comments { get; }

	DateTime? DueDate { get; }

	string FogBugz { get; }

	bool? HasDescription { get; }

	bool? HasVoted { get; }

	bool? IsComplete { get; }

	bool? IsSubscribed { get; }

	int? Votes { get; }
}
