using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeLabel : IJsonLabel, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public IJsonBoard Board { get; set; }

	public LabelColor? Color { get; set; }

	public bool ForceNullColor { get; set; }

	public string Id { get; set; }

	public string Name { get; set; }

	public int? Uses { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Board = obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			Color = obj.Deserialize<LabelColor?>(serializer, "color");
			Id = obj.TryGetString("id");
			Name = obj.TryGetString("name");
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
		if (Color.HasValue)
		{
			Color.Serialize(jsonObject, serializer, "color");
		}
		else if (ForceNullColor)
		{
			jsonObject["color"] = JsonValue.Null;
		}
		Name.Serialize(jsonObject, serializer, "name");
		return jsonObject;
	}
}
