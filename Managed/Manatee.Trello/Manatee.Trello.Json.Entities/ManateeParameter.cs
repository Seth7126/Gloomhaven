using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeParameter : IJsonParameter, IJsonSerializable
{
	public string String { get; set; }

	public bool? Boolean { get; set; }

	public object Object { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			String = json.Object.TryGetString("value");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		if (Boolean.HasValue)
		{
			bool? boolean = Boolean;
			jsonObject.Add("value", boolean.HasValue ? ((JsonValue)(boolean == true)) : null);
		}
		else if (Object != null)
		{
			jsonObject.Add("value", serializer.Serialize(Object));
		}
		else
		{
			jsonObject.Add("value", String);
		}
		return jsonObject;
	}
}
