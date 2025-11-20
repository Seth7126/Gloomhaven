using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class NullableSerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		if (context.InferredType.GetTypeInfo().IsGenericType)
		{
			return context.InferredType.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	[UsedImplicitly]
	private static JsonValue _Encode<T>(SerializationContext context) where T : struct
	{
		T? val = (T?)context.Source;
		if (!val.HasValue)
		{
			return JsonValue.Null;
		}
		bool encodeDefaultValues = context.RootSerializer.Options.EncodeDefaultValues;
		context.RootSerializer.Options.EncodeDefaultValues = object.Equals(val.Value, default(T));
		context.Push(typeof(T), typeof(T), null, val.Value);
		JsonValue result = context.RootSerializer.Serialize(context);
		context.Pop();
		context.RootSerializer.Options.EncodeDefaultValues = encodeDefaultValues;
		return result;
	}

	[UsedImplicitly]
	private static T? _Decode<T>(DeserializationContext context) where T : struct
	{
		if (context.LocalValue == JsonValue.Null)
		{
			return null;
		}
		context.Push(typeof(T), null, context.LocalValue);
		T value = (T)context.RootSerializer.Deserialize(context);
		context.Pop();
		return value;
	}
}
