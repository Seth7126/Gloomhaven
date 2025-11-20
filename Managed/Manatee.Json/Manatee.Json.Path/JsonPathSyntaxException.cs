using System;

namespace Manatee.Json.Path;

public class JsonPathSyntaxException : Exception
{
	public string? Path { get; }

	public override string Message => base.Message + " Path up to error: '" + Path + "'";

	internal JsonPathSyntaxException(JsonPath? path, string message)
		: base(message)
	{
		Path = path?.ToString();
	}
}
