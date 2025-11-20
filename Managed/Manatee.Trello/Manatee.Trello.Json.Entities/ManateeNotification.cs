using System;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeNotification : IJsonNotification, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public bool? Unread { get; set; }

	public NotificationType? Type { get; set; }

	public DateTime? Date { get; set; }

	public IJsonNotificationData Data { get; set; }

	public IJsonMember MemberCreator { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			MemberCreator = obj.Deserialize<IJsonMember>(serializer, "idMemberCreator");
			Data = obj.Deserialize<IJsonNotificationData>(serializer, "data");
			Unread = obj.TryGetBoolean("unread");
			Type = obj.Deserialize<NotificationType?>(serializer, "type");
			Date = obj.Deserialize<DateTime?>(serializer, "date");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		Unread.Serialize(jsonObject, serializer, "unread");
		return jsonObject;
	}
}
