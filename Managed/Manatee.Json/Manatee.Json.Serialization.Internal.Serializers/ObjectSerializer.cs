using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using JetBrains.Annotations;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class ObjectSerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		return context.InferredType == typeof(object);
	}

	[UsedImplicitly]
	private static JsonValue _Encode(SerializationContext context)
	{
		throw new NotImplementedException();
	}

	[UsedImplicitly]
	private static object _Decode(DeserializationContext context)
	{
		switch (context.LocalValue.Type)
		{
		case JsonValueType.Number:
			return context.LocalValue.Number;
		case JsonValueType.String:
			return context.LocalValue.String;
		case JsonValueType.Boolean:
			return context.LocalValue.Boolean;
		case JsonValueType.Array:
			return context.LocalValue.Array.Select(delegate(JsonValue value, int i)
			{
				context.Push(typeof(object), i.ToString(), value);
				object? result = context.RootSerializer.Deserialize(context);
				context.Pop();
				return result;
			}).ToList();
		case JsonValueType.Object:
		{
			IDictionary<string, object> dictionary = new ExpandoObject();
			{
				foreach (KeyValuePair<string, JsonValue> item in context.LocalValue.Object)
				{
					context.Push(typeof(object), item.Key, item.Value);
					dictionary[item.Key] = context.RootSerializer.Deserialize(context);
					context.Pop();
				}
				return dictionary;
			}
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
