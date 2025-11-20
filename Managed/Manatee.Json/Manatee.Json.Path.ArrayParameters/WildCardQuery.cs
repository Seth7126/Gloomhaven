using System;
using System.Collections.Generic;

namespace Manatee.Json.Path.ArrayParameters;

internal class WildCardQuery : IJsonPathArrayQuery, IEquatable<WildCardQuery>
{
	public static WildCardQuery Instance { get; }

	static WildCardQuery()
	{
		Instance = new WildCardQuery();
	}

	private WildCardQuery()
	{
	}

	public IEnumerable<JsonValue> Find(JsonArray json, JsonValue root)
	{
		return json;
	}

	public override string ToString()
	{
		return "*";
	}

	public bool Equals(WildCardQuery? other)
	{
		return other != null;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as WildCardQuery);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
