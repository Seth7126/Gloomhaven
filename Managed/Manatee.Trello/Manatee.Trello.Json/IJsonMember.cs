using System;
using System.Collections.Generic;

namespace Manatee.Trello.Json;

public interface IJsonMember : IJsonCacheable
{
	[JsonDeserialize]
	[Obsolete("Trello has deprecated this property.")]
	string AvatarHash { get; set; }

	[JsonDeserialize]
	string AvatarUrl { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Bio { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string FullName { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Initials { get; set; }

	[JsonDeserialize]
	string MemberType { get; set; }

	[JsonDeserialize]
	MemberStatus? Status { get; set; }

	[JsonDeserialize]
	string Url { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Username { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[Obsolete("Trello has deprecated this property.")]
	AvatarSource? AvatarSource { get; set; }

	[JsonDeserialize]
	bool? Confirmed { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	string Email { get; set; }

	[JsonDeserialize]
	[Obsolete("Trello has deprecated this property.")]
	string GravatarHash { get; set; }

	[JsonDeserialize]
	List<string> LoginTypes { get; set; }

	[JsonDeserialize]
	List<string> Trophies { get; set; }

	[JsonDeserialize]
	[Obsolete("Trello has deprecated this property.")]
	string UploadedAvatarHash { get; set; }

	[JsonDeserialize]
	List<string> OneTimeMessagesDismissed { get; set; }

	[JsonDeserialize]
	int? Similarity { get; set; }

	[JsonDeserialize]
	[JsonSerialize]
	[JsonSpecialSerialization]
	IJsonMemberPreferences Prefs { get; set; }

	[JsonDeserialize]
	List<IJsonAction> Actions { get; set; }

	[JsonDeserialize]
	List<IJsonBoard> Boards { get; set; }

	[JsonDeserialize]
	List<IJsonCard> Cards { get; set; }

	[JsonDeserialize]
	List<IJsonNotification> Notifications { get; set; }

	[JsonDeserialize]
	List<IJsonOrganization> Organizations { get; set; }

	[JsonDeserialize]
	List<IJsonStarredBoard> StarredBoards { get; set; }
}
