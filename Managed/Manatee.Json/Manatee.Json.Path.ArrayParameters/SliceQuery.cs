using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;

namespace Manatee.Json.Path.ArrayParameters;

internal class SliceQuery : IJsonPathArrayQuery, IEquatable<SliceQuery>
{
	internal IEnumerable<Slice> Slices { get; }

	public SliceQuery(params Slice[] slices)
	{
		Slices = slices.ToList();
	}

	public IEnumerable<JsonValue> Find(JsonArray json, JsonValue root)
	{
		HashSet<JsonValue> hashSet = new HashSet<JsonValue>();
		foreach (Slice slice in Slices)
		{
			hashSet.UnionWith(slice.Find(json, root));
		}
		return hashSet;
	}

	public override string ToString()
	{
		return string.Join(",", Slices);
	}

	public bool Equals(SliceQuery? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Slices.ContentsEqual(other.Slices);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SliceQuery);
	}

	public override int GetHashCode()
	{
		return Slices?.GetCollectionHashCode() ?? 0;
	}
}
