namespace Manatee.Json.Serialization;

public interface IJsonSerializable
{
	void FromJson(JsonValue json, JsonSerializer serializer);

	JsonValue ToJson(JsonSerializer serializer);
}
