using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeList : IJsonList, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public string Name { get; set; }

	public bool? Closed { get; set; }

	public IJsonBoard Board { get; set; }

	public IJsonPosition Pos { get; set; }

	public bool? Subscribed { get; set; }

	public List<IJsonAction> Actions { get; set; }

	public List<IJsonCard> Cards { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Name = obj.TryGetString("name");
			Closed = obj.TryGetBoolean("closed");
			Board = obj.Deserialize<IJsonBoard>(serializer, "board") ?? obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
			Subscribed = obj.TryGetBoolean("subscribed");
			Actions = obj.Deserialize<List<IJsonAction>>(serializer, "actions");
			Cards = obj.Deserialize<List<IJsonCard>>(serializer, "cards");
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
		Name.Serialize(jsonObject, serializer, "name");
		Pos.Serialize(jsonObject, serializer, "pos");
		Subscribed.Serialize(jsonObject, serializer, "subscribed");
		return jsonObject;
	}
}
