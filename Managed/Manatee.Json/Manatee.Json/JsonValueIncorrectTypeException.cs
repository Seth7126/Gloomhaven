using System;

namespace Manatee.Json;

public class JsonValueIncorrectTypeException : InvalidOperationException
{
	public JsonValueType ValidType { get; }

	public JsonValueType RequestedType { get; }

	internal JsonValueIncorrectTypeException(JsonValueType valid, JsonValueType requested)
		: base($"Cannot access value of type {valid} as type {requested}.")
	{
		ValidType = valid;
		RequestedType = requested;
	}
}
