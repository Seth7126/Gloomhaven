using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCustomFieldDefinition : IJsonCustomFieldDefinition, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public IJsonBoard Board { get; set; }

	public string FieldGroup { get; set; }

	public string Name { get; set; }

	public IJsonPosition Pos { get; set; }

	public CustomFieldType? Type { get; set; }

	public List<IJsonCustomDropDownOption> Options { get; set; }

	public IJsonCustomFieldDisplayInfo Display { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Board = obj.Deserialize<IJsonBoard>(serializer, "idBoard");
			FieldGroup = obj.TryGetString("fieldGroup");
			Name = obj.TryGetString("name");
			Pos = obj.Deserialize<IJsonPosition>(serializer, "pos");
			Type = obj.Deserialize<CustomFieldType?>(serializer, "type");
			Options = obj.Deserialize<List<IJsonCustomDropDownOption>>(serializer, "options");
			Display = obj.Deserialize<IJsonCustomFieldDisplayInfo>(serializer, "display");
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
		if (Board != null)
		{
			jsonObject["idModel"] = Board.Id;
			jsonObject["modelType"] = "board";
		}
		Name.Serialize(jsonObject, serializer, "name");
		Pos.Serialize(jsonObject, serializer, "pos");
		Type.Serialize(jsonObject, serializer, "type");
		if (Type == CustomFieldType.DropDown)
		{
			Options.Serialize(jsonObject, serializer, "options");
		}
		Display?.CardFront.Serialize(jsonObject, serializer, "display/cardFront");
		return jsonObject;
	}
}
