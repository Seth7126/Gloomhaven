using System;
using System.Linq;

namespace Manatee.Json;

public static class JsonArrayExtensions
{
	public static JsonArray OfType(this JsonArray? arr, JsonValueType type)
	{
		if (arr == null)
		{
			throw new ArgumentNullException("arr");
		}
		JsonArray jsonArray = new JsonArray();
		jsonArray.AddRange(arr.Where((JsonValue j) => j.Type == type));
		return jsonArray;
	}
}
