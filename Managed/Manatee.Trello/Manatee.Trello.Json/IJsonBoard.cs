using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonBoard : IJsonCacheable, IAcceptId
{
	[JsonDeserialize]
	[JsonSerialize]
	string Name { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Desc { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Closed { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	IJsonOrganization Organization { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Pinned { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	IJsonBoardPreferences Prefs { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Url { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Subscribed { get; set; }

	[JsonSerialize]
	IJsonBoard BoardSource { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	bool? Starred { get; set; }

	[JsonDeserialize]
	DateTime? DateLastActivity { get; set; }

	[JsonDeserialize]
	DateTime? DateLastView { get; set; }

	[JsonDeserialize]
	string ShortLink { get; set; }

	[JsonDeserialize]
	string ShortUrl { get; set; }

	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonCard> Cards { get; set; }

	[JsonDeserialize]
	List<IJsonCustomFieldDefinition> CustomFields { get; set; }

	[JsonDeserialize]
	List<IJsonLabel> Labels { get; set; }

	[JsonDeserialize]
	List<IJsonList> Lists { get; set; }

	[JsonDeserialize]
	List<IJsonMember> Members { get; set; }

	[JsonDeserialize]
	List<IJsonBoardMembership> Memberships { get; set; }

	[JsonDeserialize]
	List<IJsonPowerUp> PowerUps { get; set; }

	[JsonDeserialize]
	List<IJsonPowerUpData> PowerUpData { get; set; }
}
