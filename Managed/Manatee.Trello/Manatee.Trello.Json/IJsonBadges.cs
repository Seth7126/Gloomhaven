using System;

namespace Manatee.Trello.Json;

public interface IJsonBadges
{
	[JsonDeserialize]
	int? Votes { get; set; }

	[JsonDeserialize]
	bool? ViewingMemberVoted { get; set; }

	[JsonDeserialize]
	bool? Subscribed { get; set; }

	[JsonDeserialize]
	string Fogbugz { get; set; }

	[JsonDeserialize]
	bool? DueComplete { get; set; }

	[JsonDeserialize]
	DateTime? Due { get; set; }

	[JsonDeserialize]
	bool? Description { get; set; }

	[JsonDeserialize]
	int? Comments { get; set; }

	[JsonDeserialize]
	int? CheckItemsChecked { get; set; }

	[JsonDeserialize]
	int? CheckItems { get; set; }

	[JsonDeserialize]
	int? Attachments { get; set; }
}
