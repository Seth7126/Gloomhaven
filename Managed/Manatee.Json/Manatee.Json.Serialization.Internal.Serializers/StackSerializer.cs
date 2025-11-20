using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class StackSerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		if (context.InferredType.GetTypeInfo().IsGenericType)
		{
			return context.InferredType.GetGenericTypeDefinition() == typeof(Stack<>);
		}
		return false;
	}

	[UsedImplicitly]
	private static JsonValue _Encode<T>(SerializationContext context)
	{
		Stack<T> stack = (Stack<T>)context.Source;
		JsonValue[] array = new JsonValue[stack.Count];
		for (int i = 0; i < array.Length; i++)
		{
			T val = stack.ElementAt(i);
			context.Push(val?.GetType() ?? typeof(T), typeof(T), i.ToString(), val);
			array[i] = context.RootSerializer.Serialize(context);
			context.Pop();
		}
		return new JsonArray(array);
	}

	[UsedImplicitly]
	private static Stack<T> _Decode<T>(DeserializationContext context)
	{
		JsonArray array = context.LocalValue.Array;
		T[] array2 = new T[array.Count];
		for (int i = 0; i < array2.Length; i++)
		{
			context.Push(typeof(T), i.ToString(), array[i]);
			array2[i] = (T)context.RootSerializer.Deserialize(context);
			context.Pop();
		}
		return new Stack<T>(array2);
	}
}
