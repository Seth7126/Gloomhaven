using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Serialization;

namespace Manatee.Json;

public static class LinqExtensions
{
	public static JsonArray ToJson(this IEnumerable<JsonValue> results)
	{
		if (results is JsonValue[] collection)
		{
			return new JsonArray(collection);
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(results);
		return jsonArray;
	}

	public static JsonObject ToJson(this IDictionary<string, JsonValue?> results)
	{
		return new JsonObject(results);
	}

	public static JsonObject ToJson(this IEnumerable<KeyValuePair<string, JsonValue?>> results)
	{
		JsonObject jsonObject = new JsonObject();
		foreach (KeyValuePair<string, JsonValue> result in results)
		{
			jsonObject.Add(result.Key, result.Value);
		}
		return jsonObject;
	}

	public static JsonValue ToJson(this IEnumerable<string?>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((string j) => (j != null) ? new JsonValue(j) : JsonValue.Null));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<bool>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((bool j) => new JsonValue(j)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<bool?>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((bool? b) => (!b.HasValue) ? JsonValue.Null : new JsonValue(b.Value)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<JsonArray>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((JsonArray j) => new JsonValue(j)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<JsonObject>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((JsonObject j) => new JsonValue(j)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<double>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((double j) => new JsonValue(j)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<double?>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((double? j) => (!j.HasValue) ? JsonValue.Null : new JsonValue(j.Value)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<int>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((int j) => new JsonValue(j)));
		return jsonArray;
	}

	public static JsonValue ToJson(this IEnumerable<int?>? list)
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select((int? j) => (!j.HasValue) ? JsonValue.Null : new JsonValue(j.Value)));
		return jsonArray;
	}

	public static JsonValue ToJson<T>(this IEnumerable<T> list, JsonSerializer serializer) where T : IJsonSerializable?
	{
		if (list == null)
		{
			return JsonValue.Null;
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(list.Select<T, JsonValue>((T j) => (j != null) ? j.ToJson(serializer) : JsonValue.Null));
		return jsonArray;
	}

	public static JsonObject ToJson<T>(this IEnumerable<KeyValuePair<string, T>> results, JsonSerializer serializer) where T : IJsonSerializable?
	{
		JsonObject jsonObject = new JsonObject();
		foreach (KeyValuePair<string, T> result in results)
		{
			jsonObject.Add(result.Key, (result.Value == null) ? JsonValue.Null : result.Value.ToJson(serializer));
		}
		return jsonObject;
	}

	public static IEnumerable<T> FromJson<T>(this IEnumerable<JsonValue> json, JsonSerializer serializer) where T : IJsonSerializable, new()
	{
		if (json == null)
		{
			throw new ArgumentNullException("json");
		}
		foreach (JsonValue item in json)
		{
			T val = new T();
			val.FromJson(item, serializer);
			yield return val;
		}
	}

	public static T FromJson<T>(this JsonObject? json, JsonSerializer serializer) where T : IJsonSerializable, new()
	{
		if (json == null)
		{
			return default(T);
		}
		T result = new T();
		result.FromJson(json, serializer);
		return result;
	}
}
