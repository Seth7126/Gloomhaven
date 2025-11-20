using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeNotificationData : IJsonNotificationData, IJsonSerializable
{
	public IJsonAttachment Attachment { get; set; }

	public IJsonBoard Board { get; set; }

	public IJsonBoard BoardSource { get; set; }

	public IJsonBoard BoardTarget { get; set; }

	public IJsonCard Card { get; set; }

	public IJsonCard CardSource { get; set; }

	public IJsonCheckItem CheckItem { get; set; }

	public IJsonCheckList CheckList { get; set; }

	public IJsonList List { get; set; }

	public IJsonList ListAfter { get; set; }

	public IJsonList ListBefore { get; set; }

	public IJsonMember Member { get; set; }

	public IJsonOrganization Org { get; set; }

	public IJsonNotificationOldData Old { get; set; }

	public string Text { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject jsonObject = json.Object;
			Attachment = jsonObject.Deserialize<IJsonAttachment>(serializer, "attachment");
			Board = jsonObject.Deserialize<IJsonBoard>(serializer, "board");
			BoardSource = jsonObject.Deserialize<IJsonBoard>(serializer, "boardSource");
			BoardTarget = jsonObject.Deserialize<IJsonBoard>(serializer, "boardTarget");
			Card = jsonObject.Deserialize<IJsonCard>(serializer, "card");
			CheckItem = jsonObject.Deserialize<IJsonCheckItem>(serializer, "checkItem");
			CheckList = jsonObject.Deserialize<IJsonCheckList>(serializer, "checklist");
			List = jsonObject.Deserialize<IJsonList>(serializer, "list");
			ListAfter = jsonObject.Deserialize<IJsonList>(serializer, "listAfter");
			ListBefore = jsonObject.Deserialize<IJsonList>(serializer, "listBefore");
			Member = jsonObject.Deserialize<IJsonMember>(serializer, jsonObject.ContainsKey("member") ? "member" : "idMember");
			Old = jsonObject.Deserialize<IJsonNotificationOldData>(serializer, "old");
			Org = jsonObject.Deserialize<IJsonOrganization>(serializer, "org");
			Text = jsonObject.TryGetString("text");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return null;
	}
}
