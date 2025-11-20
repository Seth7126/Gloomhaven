using System;

namespace Manatee.Trello.Json;

public interface IJsonActionData
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
	IJsonCustomFieldDefinition CustomField { get; set; }

	[JsonDeserialize]
	IJsonLabel Label { get; set; }

	[JsonDeserialize]
	DateTime? DateLastEdited { get; set; }

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
	IJsonActionOldData Old { get; set; }

	[JsonDeserialize]
	IJsonPowerUp Plugin { get; set; }

	[JsonDeserialize]
	string Text { get; set; }

	[JsonDeserialize]
	string Value { get; set; }
}
