using System;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class ArraySerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		return context.InferredType.IsArray;
	}

	protected override Type[] GetTypeArguments(Type type)
	{
		return new Type[1] { type.GetElementType() };
	}

	[UsedImplicitly]
	private static JsonValue _Encode<T>(SerializationContext context)
	{
		T[] array = (T[])context.Source;
		JsonValue[] array2 = new JsonValue[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			context.Push(array[i]?.GetType() ?? typeof(T), typeof(T), i.ToString(), array[i]);
			array2[i] = context.RootSerializer.Serialize(context);
			context.Pop();
		}
		return new JsonArray(array2);
	}

	[UsedImplicitly]
	private static T[] _Decode<T>(DeserializationContext context)
	{
		JsonArray array = context.LocalValue.Array;
		T[] array2 = new T[array.Count];
		for (int i = 0; i < array.Count; i++)
		{
			context.Push(typeof(T), i.ToString(), array[i]);
			array2[i] = (T)context.RootSerializer.Deserialize(context);
			context.Pop();
		}
		return array2;
	}
}
