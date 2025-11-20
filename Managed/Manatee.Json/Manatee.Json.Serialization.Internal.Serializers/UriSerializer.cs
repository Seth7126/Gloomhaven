using System;

namespace Manatee.Json.Serialization.Internal.Serializers;

internal class UriSerializer : IPrioritizedSerializer, ISerializer
{
	public int Priority => 2;

	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(Uri);
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return ((Uri)context.Source).OriginalString;
	}

	public object Deserialize(DeserializationContext context)
	{
		return new Uri(context.LocalValue.String);
	}
}
