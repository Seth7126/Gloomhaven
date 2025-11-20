using System;
using JetBrains.Annotations;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class NumericSerializer : IPrioritizedSerializer, ISerializer
{
	public int Priority => 2;

	public bool ShouldMaintainReferences => false;

	public bool Handles(SerializationContextBase context)
	{
		return context.InferredType.IsNumericType();
	}

	public JsonValue Serialize(SerializationContext context)
	{
		return Convert.ToDouble(context.Source);
	}

	public object Deserialize(DeserializationContext context)
	{
		return Convert.ChangeType(context.LocalValue.Number, context.InferredType);
	}
}
