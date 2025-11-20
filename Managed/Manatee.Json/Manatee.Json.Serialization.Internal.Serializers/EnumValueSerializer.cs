using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class EnumValueSerializer : IPrioritizedSerializer, ISerializer
{
	public int Priority => 2;

	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		DeserializationContext deserializationContext = context as DeserializationContext;
		if (context.InferredType.GetTypeInfo().IsEnum)
		{
			if (deserializationContext != null || context.RootSerializer.Options.EnumSerializationFormat != EnumSerializationFormat.AsInteger)
			{
				if (deserializationContext != null)
				{
					JsonValue localValue = deserializationContext.LocalValue;
					if ((object)localValue == null)
					{
						return false;
					}
					return localValue.Type == JsonValueType.Number;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return Convert.ToInt32(context.Source);
	}

	public object Deserialize(DeserializationContext context)
	{
		int value = (int)context.LocalValue.Number;
		return Enum.ToObject(context.InferredType, value);
	}
}
