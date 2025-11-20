using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCard : IJsonCard, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public List<IJsonAction> Actions { get; set; }

	public List<IJsonAttachment> Attachments { get; set; }

	public IJsonBadges Badges { get; set; }

	public List<IJsonCheckList> CheckLists { get; set; }

	public bool? Closed { get; set; }

	public List<IJsonAction> Comments { get; set; }

	public List<IJsonCustomField> CustomFields { get; set; }

	public DateTime? DateLastActivity { get; set; }

	public string Desc { get; set; }

	public DateTime? Due { get; set; }

	public bool? DueComplete { get; set; }

	public IJsonBoard Board { get; set; }

	public IJsonList List { get; set; }

	public int? IdShort { get; set; }

	public string IdAttachmentCover { get; set; }

	public List<IJsonLabel> Labels { get; set; }

	public bool? ManualCoverAttachment { get; set; }

	public List<IJsonMember> Members { get; set; }

	public string Name { get; set; }

	public IJsonPosition Pos { get; set; }

	public List<IJsonPowerUpData> PowerUpData { get; set; }

	public string Url { get; set; }

	public string ShortUrl { get; set; }

	public bool? Subscribed { get; set; }

	public IJsonCard CardSource { get; set; }

	public object UrlSource { get; set; }

	public bool ForceDueDate { get; set; }

	public string IdMembers { get; set; }

	public string IdLabels { get; set; }

	public List<IJsonSticker> Stickers { get; set; }

	public List<IJsonMember> MembersVoted { get; set; }

	public CardCopyKeepFromSourceOptions KeepFromSource { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Attachments = obj.Deserialize<List<IJsonAttachment>>(serializer, "attachments");
			Badges = obj.Deserialize<IJsonBadges>(serializer, "badges");
			CheckLists = obj.Deserialize<List<IJsonCheckList>>(serializer, "checklists");
			Closed = obj.TryGetBoolean("closed");
			if (Card.DownloadedFields.HasFlag(Card.Fields.Comments))
			{
				JsonArray jsonArray = obj.TryGetArray("actions")?.Where((JsonValue jv) => jv.Type == JsonValueType.Object && jv.Object.TryGetString("type") == "commentCard").ToJson();
				if (jsonArray != null)
				{
					Comments = serializer.Deserialize<List<IJsonAction>>(jsonArray);
				}
			}
			else
			{
				Actions = obj.Deserialize<List<IJsonAction>>(serializer, "actions");
			}
			CustomFields = obj.Deserialize<List<IJsonCustomField>>(serializer, "customFieldItems");
			DateLastActivity = obj.Deserialize<DateTime?>(serializer, "dateLastActivity");
			Due = obj.Deserialize<DateTime?>(serializer, "due");
			DueComplete = obj.TryGetBoolean("dueComplete");
			Desc = obj.TryGetString("desc");
			Board = obj.Deserialize<IJsonBoard>(serializer, "board") ?? obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			List = obj.Deserialize<IJsonList>(serializer, "list") ?? obj.Deserialize<IJsonList>(serializer, "idList");
			IdShort = (int?)obj.TryGetNumber("idShort");
			IdAttachmentCover = obj.TryGetString("idAttachmentCover");
			Labels = obj.Deserialize<List<IJsonLabel>>(serializer, "labels");
			ManualCoverAttachment = obj.TryGetBoolean("manualAttachmentCover");
			Members = obj.Deserialize<List<IJsonMember>>(serializer, "members");
			Name = obj.TryGetString("name");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
			Url = obj.TryGetString("url");
			ShortUrl = obj.TryGetString("shortUrl") ?? ("https://trello.com/c/" + obj.TryGetString("shortLink"));
			Subscribed = obj.TryGetBoolean("subscribed");
			Stickers = obj.Deserialize<List<IJsonSticker>>(serializer, "stickers");
			MembersVoted = obj.Deserialize<List<IJsonMember>>(serializer, "membersVoted");
			ValidForMerge = true;
			break;
		}
		case JsonValueType.String:
			Id = json.String;
			break;
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		Board.SerializeId(jsonObject, "idBoard");
		Closed.Serialize(jsonObject, serializer, "closed");
		Desc.Serialize(jsonObject, serializer, "desc");
		Due.Serialize(jsonObject, serializer, "due", ForceDueDate);
		DueComplete.Serialize(jsonObject, serializer, "dueComplete");
		List.SerializeId(jsonObject, "idList");
		Name.Serialize(jsonObject, serializer, "name");
		Pos.Serialize(jsonObject, serializer, "pos");
		Subscribed.Serialize(jsonObject, serializer, "subscribed");
		CardSource.SerializeId(jsonObject, "idCardSource");
		UrlSource.Serialize(jsonObject, serializer, "urlSource");
		IdMembers.Serialize(jsonObject, serializer, "idMembers");
		IdLabels.Serialize(jsonObject, serializer, "idLabels");
		if (KeepFromSource != CardCopyKeepFromSourceOptions.None)
		{
			KeepFromSource.Serialize(jsonObject, serializer, "keepFromSource");
		}
		return jsonObject;
	}
}
