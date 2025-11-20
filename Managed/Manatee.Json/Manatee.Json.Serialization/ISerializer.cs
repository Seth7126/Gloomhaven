namespace Manatee.Json.Serialization;

public interface ISerializer
{
	bool ShouldMaintainReferences { get; }

	bool Handles(SerializationContextBase context);

	JsonValue Serialize(SerializationContext context);

	object Deserialize(DeserializationContext context);
}
