using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Trello.Json.Entities;

internal class ManateeWebhook : IJsonWebhook, IJsonCacheable, IJsonSerializable
{
	public string Id { get; set; }

	public string Description { get; set; }

	public string IdModel { get; set; }

	public string CallbackUrl { get; set; }

	public bool? Active { get; set; }

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Object)
		{
			JsonObject obj = json.Object;
			Id = obj.TryGetString("id");
			Description = obj.TryGetString("description");
			IdModel = obj.TryGetString("idModel");
			CallbackUrl = obj.TryGetString("callbackURL");
			Active = obj.TryGetBoolean("active");
		}
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonObject jsonObject = new JsonObject();
		Id.Serialize(jsonObject, serializer, "id");
		Description.Serialize(jsonObject, serializer, "description");
		IdModel.Serialize(jsonObject, serializer, "idModel");
		CallbackUrl.Serialize(jsonObject, serializer, "callbackURL");
		Active.Serialize(jsonObject, serializer, "active");
		return jsonObject;
	}
}
