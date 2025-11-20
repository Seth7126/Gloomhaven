using System;
using System.Collections.Generic;
using System.Linq;

namespace Manatee.Json.Schema;

internal static class JsonSchemaTypeExtensions
{
	public static JsonValue ToJson(this JsonSchemaType type)
	{
		List<string> list = (from JsonSchemaType v in Enum.GetValues(typeof(JsonSchemaType))
			where v != JsonSchemaType.NotDefined
			where type.HasFlag(v)
			select v).Select(_TranslateSingleType).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		JsonValue jsonValue = list.ToJson();
		jsonValue.Array.EqualityStandard = ArrayEquality.ContentsEqual;
		return jsonValue;
	}

	public static JsonSchemaType ToSchemaType(this JsonValue json)
	{
		return json.Type switch
		{
			JsonValueType.Array => json.Array.Aggregate(JsonSchemaType.NotDefined, (JsonSchemaType c, JsonValue jv) => c | _TranslateSingleType(jv.String)), 
			JsonValueType.String => _TranslateSingleType(json.String), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static JsonSchemaType _TranslateSingleType(string type)
	{
		return type switch
		{
			"array" => JsonSchemaType.Array, 
			"boolean" => JsonSchemaType.Boolean, 
			"integer" => JsonSchemaType.Integer, 
			"null" => JsonSchemaType.Null, 
			"number" => JsonSchemaType.Number, 
			"object" => JsonSchemaType.Object, 
			"string" => JsonSchemaType.String, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static string? _TranslateSingleType(JsonSchemaType type)
	{
		return type switch
		{
			JsonSchemaType.NotDefined => null, 
			JsonSchemaType.Array => "array", 
			JsonSchemaType.Boolean => "boolean", 
			JsonSchemaType.Integer => "integer", 
			JsonSchemaType.Null => "null", 
			JsonSchemaType.Number => "number", 
			JsonSchemaType.Object => "object", 
			JsonSchemaType.String => "string", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
