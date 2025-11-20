namespace Manatee.Trello.Json;

public interface IJsonNotificationData
{
	[JsonDeserialize]
	IJsonAttachment Attachment { get; set; }

	[JsonDeserialize]
	IJsonBoard Board { get; set; }

	[JsonDeserialize]
	IJsonBoard BoardSource { get; set; }

	[JsonDeserialize]
	IJsonBoard BoardTarget { get; set; }

	[JsonDeserialize]
	IJsonCard Card { get; set; }

	[JsonDeserialize]
	IJsonCard CardSource { get; set; }

	[JsonDeserialize]
	IJsonCheckItem CheckItem { get; set; }

	[JsonDeserialize]
	IJsonCheckList CheckList { get; set; }

	[JsonDeserialize]
	IJsonList List { get; set; }

	[JsonDeserialize]
	IJsonList ListAfter { get; set; }

	[JsonDeserialize]
	IJsonList ListBefore { get; set; }

	[JsonDeserialize]
	IJsonMember Member { get; set; }

	[JsonDeserialize]
	IJsonOrganization Org { get; set; }

	[JsonDeserialize]
	IJsonNotificationOldData Old { get; set; }

	[JsonDeserialize]
	string Text { get; set; }
}
