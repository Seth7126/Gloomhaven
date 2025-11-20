using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Path;

namespace Manatee.Json.Transform;

public static class JsonTransformer
{
	public static JsonValue Transform(this JsonValue source, JsonValue template)
	{
		return source._Transform(source, template, -1);
	}

	private static JsonValue _Transform(this JsonValue source, JsonValue localSource, JsonValue template, int index)
	{
		switch (template.Type)
		{
		case JsonValueType.Number:
		case JsonValueType.Boolean:
		case JsonValueType.Null:
			return template;
		case JsonValueType.String:
			return _TransformString(source, localSource, template.String, index);
		case JsonValueType.Object:
			return _TransformObject(source, localSource, template.Object, index);
		case JsonValueType.Array:
			return _TransformArray(source, localSource, template.Array, index);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static JsonValue _TransformObject(JsonValue source, JsonValue localSource, JsonObject template, int index)
	{
		JsonObject jsonObject = new JsonObject();
		foreach (KeyValuePair<string, JsonValue> item in template)
		{
			jsonObject[item.Key] = source._Transform(localSource, item.Value, index);
		}
		return jsonObject;
	}

	private static JsonValue _TransformArray(JsonValue source, JsonValue localSource, JsonArray template, int index)
	{
		if (template.Count != 2 || template[0].Type != JsonValueType.String)
		{
			return _TransformArrayElements(source, localSource, template, index);
		}
		JsonPath jsonPath = _TryGetPath(template[0].String);
		if (jsonPath == null)
		{
			return _TransformArrayElements(source, localSource, template, index);
		}
		return new JsonArray(jsonPath.Evaluate(source).Select((JsonValue item, int i) => source._Transform(item, template[1], i)));
	}

	private static JsonValue _TransformArrayElements(JsonValue source, JsonValue localSource, JsonArray array, int index)
	{
		return new JsonArray(array.Select((JsonValue jv) => source._Transform(localSource, jv, index)));
	}

	private static JsonValue _TransformString(JsonValue source, JsonValue localSource, string template, int index)
	{
		JsonPath jsonPath = _TryGetPath(template);
		if (jsonPath == null)
		{
			return template;
		}
		bool flag = template[0] == '@';
		JsonArray jsonArray = jsonPath.Evaluate(flag ? localSource : source);
		if (jsonArray.Count != 1)
		{
			if (index != -1)
			{
				return jsonArray[index];
			}
			return jsonArray;
		}
		return jsonArray[0];
	}

	private static JsonPath? _TryGetPath(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (text[0] == '@')
		{
			text = "$" + text.Substring(1);
		}
		try
		{
			return JsonPath.Parse(text);
		}
		catch
		{
			return null;
		}
	}
}
