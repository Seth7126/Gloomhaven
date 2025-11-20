using System.Collections.Generic;
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeSticker : IJsonSticker, IJsonCacheable, IAcceptId, IJsonSerializable
{
	public string Id { get; set; }

	public double? Left { get; set; }

	public string Name { get; set; }

	public List<IJsonImagePreview> Previews { get; set; }

	public int? Rotation { get; set; }

	public double? Top { get; set; }

	public string Url { get; set; }

	public int? ZIndex { get; set; }

	public bool ValidForMerge { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		switch (json.Type)
		{
		case JsonValueType.Object:
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Left = obj.TryGetNumber("left");
			Name = obj.TryGetString("image");
			Previews = obj.Deserialize<List<IJsonImagePreview>>(serializer, "imageScaled");
			Rotation = (int?)obj.TryGetNumber("rotate");
			Top = obj.TryGetNumber("top");
			Url = obj.TryGetString("imageUrl");
			ZIndex = (int?)obj.TryGetNumber("zIndex");
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
		Left.Serialize(jsonObject, serializer, "left");
		Name.Serialize(jsonObject, serializer, "image");
		Rotation.Serialize(jsonObject, serializer, "rotate");
		Top.Serialize(jsonObject, serializer, "top");
		ZIndex.Serialize(jsonObject, serializer, "zIndex");
		return jsonObject;
	}
}
