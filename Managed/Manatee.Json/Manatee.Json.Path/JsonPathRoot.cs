using System;

namespace Manatee.Json.Path;

public static class JsonPathRoot
{
	public static int Length()
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static bool HasProperty(string name)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static JsonPathValue Name(string name)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}

	public static JsonPathValue ArrayIndex(int index)
	{
		throw new InvalidOperationException("This operation is reserved for JsonPath.");
	}
}
