using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class JsonSerializableSerializer : IPrioritizedSerializer, ISerializer
{
	public int Priority => 2;

	public bool ShouldMaintainReferences => true;

	public bool Handles(SerializationContextBase context)
	{
		return typeof(IJsonSerializable).GetTypeInfo().IsAssignableFrom(context.InferredType.GetTypeInfo());
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return ((IJsonSerializable)context.Source).ToJson(context.RootSerializer);
	}

	public object Deserialize(DeserializationContext context)
	{
		IJsonSerializable obj = (IJsonSerializable)context.RootSerializer.AbstractionMap.CreateInstance(context);
		obj.FromJson(context.LocalValue, context.RootSerializer);
		return obj;
	}
}
