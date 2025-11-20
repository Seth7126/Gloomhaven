using System;

namespace Manatee.Json.Path;

public static class PathExpressionExtensions
{
	public static int Length(this JsonPathArray json)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static JsonPathArray Name(this JsonPathArray json, string name)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static int Length(this JsonPathValue json)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static bool HasProperty(this JsonPathValue json, string name)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static JsonPathValue Name(this JsonPathValue json, string name)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static JsonPathValue ArrayIndex(this JsonPathValue json, int index)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static int IndexOf(this JsonPathValue json, JsonValue value)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}
}
