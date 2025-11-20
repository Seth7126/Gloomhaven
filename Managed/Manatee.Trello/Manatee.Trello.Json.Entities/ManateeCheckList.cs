using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCheckList : IJsonCheckList, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public string Name { get; set; }

	public IJsonBoard Board { get; set; }

	public IJsonCard Card { get; set; }

	public List<IJsonCheckItem> CheckItems { get; set; }

	public IJsonPosition Pos { get; set; }

	public IJsonCheckList CheckListSource { get; set; }

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
			Board = obj.Deserialize<IJsonBoard>(serializer, "board") ?? obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			Card = obj.Deserialize<IJsonCard>(serializer, "card") ?? obj.Deserialize<IJsonCard>(serializer, "idCard");
			CheckItems = obj.Deserialize<List<IJsonCheckItem>>(serializer, "checkItems");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
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
		Card.SerializeId(jsonObject, "idCard");
		Name.Serialize(jsonObject, serializer, "name");
		Pos.Serialize(jsonObject, serializer, "pos");
		CheckListSource.SerializeId(jsonObject, "idChecklistSource");
		return jsonObject;
	}
}
