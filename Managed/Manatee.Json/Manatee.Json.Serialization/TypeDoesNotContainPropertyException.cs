using System;

namespace Manatee.Json.Serialization;

public class TypeDoesNotContainPropertyException : Exception
{
	public Type Type { get; }

	public JsonValue Json { get; }

	internal TypeDoesNotContainPropertyException(Type type, JsonValue json)
		: base($"Type {type} does not contain any properties within {json}.")
	{
		Type = type;
		Json = json;
	}
}
