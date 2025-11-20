using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeStarredBoard : IJsonStarredBoard, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonBoard Board { get; set; }

	public IJsonPosition Pos { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Board = obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Board.Id.Serialize(jsonObject, serializer, "idBoard");
		Pos.Serialize(jsonObject, serializer, "pos");
		return jsonObject;
	}
}
