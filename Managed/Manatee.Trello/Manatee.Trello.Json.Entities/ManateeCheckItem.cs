using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCheckItem : IJsonCheckItem, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonCheckList CheckList { get; set; }

	public CheckItemState? State { get; set; }

	public string Name { get; set; }

	public IJsonPosition Pos { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			CheckList = obj.Deserialize<IJsonCheckList>(serializer, "checklist") ?? obj.Deserialize<IJsonCheckList>(serializer, "idChecklist");
			State = obj.Deserialize<CheckItemState?>(serializer, "state");
			Name = obj.TryGetString("name");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		CheckList?.Id.Serialize(jsonObject, serializer, "idChecklist");
		Id.Serialize(jsonObject, serializer, "id");
		Name.Serialize(jsonObject, serializer, "name");
		Pos.Serialize(jsonObject, serializer, "pos");
		State.Serialize(jsonObject, serializer, "state");
		return jsonObject;
	}
}
