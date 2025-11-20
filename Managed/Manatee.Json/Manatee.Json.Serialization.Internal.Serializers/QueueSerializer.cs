using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class QueueSerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		if (context.InferredType.GetTypeInfo().IsGenericType)
		{
			return context.InferredType.GetGenericTypeDefinition() == typeof(Queue<>);
		}
		return false;
	}

	[UsedImplicitly]
	private static JsonValue _Encode<T>(SerializationContext context)
	{
		Queue<T> queue = (Queue<T>)context.Source;
		JsonArray jsonArray = new JsonArray();
		for (int i = 0; i < queue.Count; i++)
		{
			T val = queue.ElementAt(i);
			context.Push(val?.GetType() ?? typeof(T), typeof(T), i.ToString(), val);
			jsonArray.Add(context.RootSerializer.Serialize(context));
			context.Pop();
		}
		return jsonArray;
	}

	[UsedImplicitly]
	private static Queue<T> _Decode<T>(DeserializationContext context)
	{
		Queue<T> queue = new Queue<T>();
		for (int i = 0; i < context.LocalValue.Array.Count; i++)
		{
			context.Push(typeof(T), i.ToString(), context.LocalValue.Array[i]);
			queue.Enqueue((T)context.RootSerializer.Deserialize(context));
			context.Pop();
		}
		return queue;
	}
}
