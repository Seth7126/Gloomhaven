using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Manatee.Json.Internal;

namespace Manatee.Json.Serialization.Internal.Serializers;

[UsedImplicitly]
internal class DictionarySerializer : GenericTypeSerializerBase
{
	public override bool Handles(SerializationContextBase context)
	{
		if (context.InferredType.GetTypeInfo().IsGenericType)
		{
			return context.InferredType.GetGenericTypeDefinition().InheritsFrom(typeof(Dictionary<, >));
		}
		return false;
	}

	[UsedImplicitly]
	private static JsonValue _Encode<TKey, TValue>(SerializationContext context) where TKey : notnull
	{
		Dictionary<TKey, TValue> dictionary = (Dictionary<TKey, TValue>)context.Source;
		bool encodeDefaultValues = context.RootSerializer.Options.EncodeDefaultValues;
		bool useDefaultValue = encodeDefaultValues || typeof(TValue).GetTypeInfo().IsValueType;
		if (typeof(TKey) == typeof(string))
		{
			Dictionary<string, JsonValue> dictionary2 = new Dictionary<string, JsonValue>();
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				TValue value = item.Value;
				context.Push(((value != null) ? value.GetType() : null) ?? typeof(TValue), typeof(TValue), item.Key.ToString(), item.Value);
				JsonValue value2 = _SerializeDefaultValue(context, useDefaultValue, encodeDefaultValues);
				dictionary2.Add((string)(object)item.Key, value2);
			}
			return dictionary2.ToJson();
		}
		if (typeof(Enum).GetTypeInfo().IsAssignableFrom(typeof(TKey).GetTypeInfo()))
		{
			return _EncodeEnumKeyDictionary(context, dictionary, useDefaultValue, encodeDefaultValues);
		}
		return _EncodeDictionary(context, dictionary, context.RootSerializer, useDefaultValue, encodeDefaultValues);
	}

	private static JsonValue _EncodeDictionary<TKey, TValue>(SerializationContext context, Dictionary<TKey, TValue> dict, JsonSerializer serializer, bool useDefaultValue, bool existingOption) where TKey : notnull
	{
		JsonValue[] array = new JsonValue[dict.Count];
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			string propertyName = num.ToString();
			context.Push(item.Key.GetType(), typeof(TKey), propertyName, item.Key);
			context.Push(item.Key.GetType(), typeof(TKey), "Key", item.Key);
			JsonValue value = serializer.Serialize(context);
			context.Pop();
			context.Pop();
			TValue value2 = item.Value;
			context.Push(((value2 != null) ? value2.GetType() : null) ?? typeof(TValue), typeof(TValue), propertyName, item.Value);
			value2 = item.Value;
			context.Push(((value2 != null) ? value2.GetType() : null) ?? typeof(TValue), typeof(TValue), "Value", item.Value);
			JsonValue value3 = _SerializeDefaultValue(context, useDefaultValue, existingOption);
			context.Pop();
			context.Pop();
			array[num] = new JsonObject
			{
				{ "Key", value },
				{ "Value", value3 }
			};
			num++;
		}
		return new JsonArray(array);
	}

	private static JsonValue _EncodeEnumKeyDictionary<TKey, TValue>(SerializationContext context, Dictionary<TKey, TValue> dict, bool useDefaultValue, bool existingOption) where TKey : notnull
	{
		JsonSerializer rootSerializer = context.RootSerializer;
		EnumSerializationFormat enumSerializationFormat = rootSerializer.Options.EnumSerializationFormat;
		rootSerializer.Options.EnumSerializationFormat = EnumSerializationFormat.AsName;
		JsonObject jsonObject = new JsonObject();
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			string propertyName = num.ToString();
			context.Push(item.Key.GetType(), typeof(TKey), propertyName, item.Key);
			context.Push(item.Key.GetType(), typeof(TKey), "Key", item.Key);
			string text = rootSerializer.Options.SerializationNameTransform(_SerializeDefaultValue(context, useDefaultValue: true, existingOption).String);
			context.Pop();
			context.Pop();
			TValue value = item.Value;
			context.Push(((value != null) ? value.GetType() : null) ?? typeof(TValue), typeof(TValue), text, item.Value);
			JsonValue value2 = _SerializeDefaultValue(context, useDefaultValue, existingOption);
			context.Pop();
			jsonObject.Add(text, value2);
			num++;
		}
		rootSerializer.Options.EnumSerializationFormat = enumSerializationFormat;
		return jsonObject;
	}

	[UsedImplicitly]
	private static Dictionary<TKey, TValue> _Decode<TKey, TValue>(DeserializationContext context) where TKey : notnull
	{
		JsonValue localValue = context.LocalValue;
		if (typeof(TKey) == typeof(string))
		{
			return localValue.Object.ToDictionary<KeyValuePair<string, JsonValue>, TKey, TValue>((KeyValuePair<string, JsonValue> kvp) => (TKey)(object)kvp.Key, delegate(KeyValuePair<string, JsonValue> kvp)
			{
				context.Push(typeof(TValue), kvp.Key.ToString(), kvp.Value);
				TValue result = (TValue)context.RootSerializer.Deserialize(context);
				context.Pop();
				return result;
			});
		}
		if (typeof(Enum).GetTypeInfo().IsAssignableFrom(typeof(TKey).GetTypeInfo()))
		{
			return _DecodeEnumDictionary<TKey, TValue>(context);
		}
		return localValue.Array.Select((JsonValue jv, int i) => new
		{
			Value = jv,
			Index = i
		}).ToDictionary(jv =>
		{
			JsonValue localValue2 = jv.Value.Object["Key"];
			context.Push(typeof(TKey), jv.Index.ToString(), localValue2);
			context.Push(typeof(TKey), "Key", localValue2);
			TKey result = (TKey)context.RootSerializer.Deserialize(context);
			context.Pop();
			context.Pop();
			return result;
		}, jv =>
		{
			JsonValue localValue2 = jv.Value.Object["Value"];
			context.Push(typeof(TValue), jv.Index.ToString(), localValue2);
			context.Push(typeof(TValue), "Value", localValue2);
			TValue result = (TValue)context.RootSerializer.Deserialize(context);
			context.Pop();
			context.Pop();
			return result;
		});
	}

	private static Dictionary<TKey, TValue> _DecodeEnumDictionary<TKey, TValue>(DeserializationContext context) where TKey : notnull
	{
		JsonSerializer rootSerializer = context.RootSerializer;
		bool encodeDefaultValues = rootSerializer.Options.EncodeDefaultValues;
		rootSerializer.Options.EncodeDefaultValues = true;
		EnumSerializationFormat enumSerializationFormat = rootSerializer.Options.EnumSerializationFormat;
		rootSerializer.Options.EnumSerializationFormat = EnumSerializationFormat.AsName;
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		int num = 0;
		foreach (KeyValuePair<string, JsonValue> item in context.LocalValue.Object)
		{
			string text = rootSerializer.Options.DeserializationNameTransform(item.Key);
			context.Push(typeof(TKey), num.ToString(), text);
			context.Push(typeof(TKey), "Key", text);
			TKey key = (TKey)rootSerializer.Deserialize(context);
			context.Pop();
			context.Pop();
			context.Push(typeof(TValue), item.Key, item.Value);
			dictionary.Add(key, (TValue)rootSerializer.Deserialize(context));
			context.Pop();
			num++;
		}
		rootSerializer.Options.EnumSerializationFormat = enumSerializationFormat;
		rootSerializer.Options.EncodeDefaultValues = encodeDefaultValues;
		return dictionary;
	}

	private static JsonValue _SerializeDefaultValue(SerializationContext context, bool useDefaultValue, bool existingOption)
	{
		context.RootSerializer.Options.EncodeDefaultValues = useDefaultValue;
		JsonValue result = context.RootSerializer.Serialize(context);
		context.RootSerializer.Options.EncodeDefaultValues = existingOption;
		return result;
	}
}
