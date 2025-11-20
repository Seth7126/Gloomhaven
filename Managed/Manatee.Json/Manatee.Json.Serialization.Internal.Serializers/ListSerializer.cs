using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class ListSerializer : GenericTypeSerializerBase
{
	public override int Priority => 3;

	public override bool Handles(SerializationContextBase context)
	{
		if (context.InferredType.GetTypeInfo().IsGenericType)
		{
			return context.InferredType.InheritsFrom(typeof(IEnumerable));
		}
		return false;
	}

	[UsedImplicitly]
	private static JsonValue _Encode<T>(SerializationContext context)
	{
		List<T> list = (List<T>)context.Source;
		JsonValue[] array = new JsonValue[list.Count];
		for (int i = 0; i < array.Length; i++)
		{
			T val = list[i];
			context.Push(((val != null) ? val.GetType() : null) ?? typeof(T), typeof(T), i.ToString(), list[i]);
			array[i] = context.RootSerializer.Serialize(context);
			context.Pop();
		}
		return new JsonArray(array);
	}

	[UsedImplicitly]
	private static List<T> _Decode<T>(DeserializationContext context)
	{
		JsonArray array = context.LocalValue.Array;
		List<T> list = new List<T>(array.Count);
		for (int i = 0; i < array.Count; i++)
		{
			context.Push(typeof(T), i.ToString(), array[i]);
			list.Add((T)context.RootSerializer.Deserialize(context));
			context.Pop();
		}
		return list;
	}

	protected override object PrepSource(SerializationContext context)
	{
		Type type = context.Source.GetType();
		if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition().InheritsFrom(typeof(List<>)))
		{
			return context.Source;
		}
		return ((IEnumerable)context.Source).Cast<object>().ToList();
	}
}
