namespace Manatee.Json;

public static class JsonObjectExtensions
{
	public static string? TryGetString(this JsonObject obj, string key)
	{
		if (obj != null)
		{
			if (!obj.ContainsKey(key) || obj[key].Type != JsonValueType.String)
			{
				return null;
			}
			return obj[key].String;
		}
		return null;
	}

	public static double? TryGetNumber(this JsonObject obj, string key)
	{
		if (obj != null)
		{
			if (!obj.ContainsKey(key) || obj[key].Type != JsonValueType.Number)
			{
				return null;
			}
			return obj[key].Number;
		}
		return null;
	}

	public static bool? TryGetBoolean(this JsonObject obj, string key)
	{
		if (obj != null)
		{
			if (!obj.ContainsKey(key) || obj[key].Type != JsonValueType.Boolean)
			{
				return null;
			}
			return obj[key].Boolean;
		}
		return null;
	}

	public static JsonArray? TryGetArray(this JsonObject obj, string key)
	{
		if (obj != null)
		{
			if (!obj.ContainsKey(key) || obj[key].Type != JsonValueType.Array)
			{
				return null;
			}
			return obj[key].Array;
		}
		return null;
	}

	public static JsonObject? TryGetObject(this JsonObject obj, string key)
	{
		if (obj != null)
		{
			if (!obj.ContainsKey(key) || obj[key].Type != JsonValueType.Object)
			{
				return null;
			}
			return obj[key].Object;
		}
		return null;
	}
}
