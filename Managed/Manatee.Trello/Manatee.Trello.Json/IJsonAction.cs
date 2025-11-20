using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonAction : IJsonCacheable
{
	[JsonDeserialize]
	IJsonMember MemberCreator { get; set; }

	[JsonDeserialize]
	IJsonActionData Data { get; set; }

	[JsonDeserialize]
	ActionType? Type { get; set; }

	[JsonDeserialize]
	DateTime? Date { get; set; }

	[JsonSerialize]
	string Text { get; set; }

	[JsonDeserialize]
	List<IJsonCommentReaction> Reactions { get; set; }
}
