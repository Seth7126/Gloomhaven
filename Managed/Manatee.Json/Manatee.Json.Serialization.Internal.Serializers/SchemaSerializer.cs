using JetBrains.Annotations;
using Manatee.Json.Schema;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class SchemaSerializer : IPrioritizedSerializer, ISerializer
{
	public bool ShouldMaintainReferences => false;

	public int Priority => 1;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(JsonSchema);
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return ((JsonSchema)context.Source).ToJson(context.RootSerializer);
	}

	public object Deserialize(DeserializationContext context)
	{
		JsonSchema jsonSchema = new JsonSchema();
		jsonSchema.FromJson(context.LocalValue, context.RootSerializer);
		return jsonSchema;
	}
}
