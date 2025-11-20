using System;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;

namespace Manatee.Json.Serialization.Internal.Serializers;

internal class ReferencingSerializer : IChainedSerializer
{
	public static ReferencingSerializer Instance { get; } = new ReferencingSerializer();

	private ReferencingSerializer()
	{
	}

	public static bool Handles(ISerializer serializer, Type type)
	{
		Log.Serialization(() => "Determining if object references should be maintained");
		if (!type.IsValueType)
		{
			return serializer.ShouldMaintainReferences;
		}
		return false;
	}

	public JsonValue TrySerialize(ISerializer serializer, SerializationContext context)
	{
		if (context.SerializationMap.TryGetPair(context.Source, out SerializationReference pair))
		{
			Log.Serialization(() => "Object already serialized; returning reference marker");
			return new JsonObject { ["$ref"] = pair.Source.ToString() };
		}
		Log.Serialization(() => "Object not serialized yet; setting up tracking...");
		context.SerializationMap.Add(new SerializationReference(context.CurrentLocation.CleanAndClone())
		{
			Object = context.Source
		});
		return serializer.Serialize(context);
	}

	public object? TryDeserialize(ISerializer serializer, DeserializationContext context)
	{
		if (context.LocalValue.Type == JsonValueType.Object && context.LocalValue.Object.TryGetValue("$ref", out JsonValue value))
		{
			Log.Serialization(() => "Found reference marker; setting up tracking...");
			JsonPointer source = JsonPointer.Parse(value.String);
			context.SerializationMap.AddReference(source, context.CurrentLocation.CleanAndClone());
			return context.InferredType.Default();
		}
		SerializationReference serializationReference = new SerializationReference(context.CurrentLocation.CleanAndClone());
		context.SerializationMap.Add(serializationReference);
		object result = (serializationReference.Object = serializer.Deserialize(context));
		serializationReference.DeserializationIsComplete = true;
		return result;
	}
}
