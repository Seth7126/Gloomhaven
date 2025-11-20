using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonCard : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonAttachment> Attachments { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	IJsonBadges Badges { get; set; }

	[JsonDeserialize]
	List<IJsonCheckList> CheckLists { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Closed { get; set; }

	[JsonDeserialize]
	List<IJsonAction> Comments { get; set; }

	[JsonDeserialize]
	List<IJsonCustomField> CustomFields { get; set; }

	[JsonDeserialize]
	DateTime? DateLastActivity { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Desc { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	DateTime? Due { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? DueComplete { get; set; }

	[JsonDeserialize]
	int? IdShort { get; set; }

	[JsonDeserialize]
	string IdAttachmentCover { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	List<IJsonLabel> Labels { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonList List { get; set; }

	[JsonDeserialize]
	bool? ManualCoverAttachment { get; set; }

	[JsonDeserialize]
	List<IJsonMember> Members { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonPosition Pos { get; set; }

	[JsonDeserialize]
	List<IJsonPowerUpData> PowerUpData { get; set; }

	[JsonDeserialize]
	string Url { get; set; }

	[JsonDeserialize]
	string ShortUrl { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Subscribed { get; set; }

	[JsonSerialize]
	IJsonCard CardSource { get; set; }

	[JsonSerialize]
	object UrlSource { get; set; }

	bool ForceDueDate { get; set; }

	[JsonSerialize]
	string IdMembers { get; set; }

	[JsonSerialize]
	string IdLabels { get; set; }

	[JsonDeserialize]
	List<IJsonSticker> Stickers { get; set; }

	[JsonDeserialize]
	List<IJsonMember> MembersVoted { get; set; }

	[JsonSerialize]
	CardCopyKeepFromSourceOptions KeepFromSource { get; set; }
}
