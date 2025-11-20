using System;

namespace Manatee.Trello.Json;

public interface IJsonNotification : IJsonCacheable
{
	[JsonDeserialize]
	[JsonSerialize]
	bool? Unread { get; set; }

	[JsonDeserialize]
	NotificationType? Type { get; set; }

	[JsonDeserialize]
	DateTime? Date { get; set; }

	[JsonDeserialize]
	IJsonNotificationData Data { get; set; }

	[JsonDeserialize]
	IJsonMember MemberCreator { get; set; }
}
