using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeCustomFieldDisplayInfo : IJsonCustomFieldDisplayInfo, IJsonSerializable
{
	public bool? CardFront { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			CardFront = obj.TryGetBoolean("cardFront");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		CardFront.Serialize(jsonObject, serializer, "cardFront");
		return jsonObject;
	}
}
