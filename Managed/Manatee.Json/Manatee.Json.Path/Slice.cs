using System;
using System.Collections.Generic;
using System.Linq;

namespace Manatee.Json.Path;

public class Slice : IEquatable<Slice>
{
	private readonly int? _start;

	private readonly int? _end;

	private readonly int? _step;

	internal int? Index { get; }

	public Slice(int index)
	{
		Index = index;
	}

	public Slice(int? start, int? end, int? step = null)
	{
		_start = start;
		_end = end;
		_step = step;
	}

	public override string? ToString()
	{
		if (!Index.HasValue)
		{
			if (!_step.HasValue)
			{
				return (_start.HasValue ? _start.ToString() : string.Empty) + ":" + (_end.HasValue ? _end.ToString() : string.Empty);
			}
			return $"{(_start.HasValue ? _start.ToString() : string.Empty)}:{(_end.HasValue ? _end.ToString() : string.Empty)}:{_step}";
		}
		return Index.ToString();
	}

	public bool Equals(Slice? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Index == other.Index && _start == other._start && _end == other._end)
		{
			return _step == other._step;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Slice);
	}

	public override int GetHashCode()
	{
		return (((((Index.GetHashCode() * 397) ^ _start.GetHashCode()) * 397) ^ _end.GetHashCode()) * 397) ^ _step.GetHashCode();
	}

	public static implicit operator Slice(int i)
	{
		return new Slice(i);
	}

	internal IEnumerable<JsonValue> Find(JsonArray json, JsonValue root)
	{
		if (Index.HasValue)
		{
			int num = Index.Value;
			if (num < 0)
			{
				num += json.Count;
			}
			if (num >= 0 && json.Count > num)
			{
				return new JsonValue[1] { json[num] };
			}
			return Enumerable.Empty<JsonValue>();
		}
		int num2 = Math.Max(_ResolveIndex(_start.GetValueOrDefault(), json.Count), 0);
		int num3 = _ResolveIndex(_end ?? json.Count, json.Count);
		if (num2 == num3)
		{
			return Enumerable.Empty<JsonValue>();
		}
		int num4 = _step ?? 1;
		if (num2 == 0 && num3 == json.Count && num4 == 1)
		{
			return json;
		}
		if (num4 > 0 && num3 <= num2)
		{
			return Enumerable.Empty<JsonValue>();
		}
		if (num4 == 1)
		{
			JsonValue[] array = new JsonValue[num3 - num2];
			json.CopyTo(num2, array, 0, num3 - num2);
			return array;
		}
		return _FindSlow(json, num2, num3, num4);
	}

	private static IEnumerable<JsonValue> _FindSlow(JsonArray json, int start, int end, int step)
	{
		Func<int, int, bool> func = ((step > 0) ? ((Func<int, int, bool>)((int a, int b) => a < b)) : ((Func<int, int, bool>)((int a, int b) => a > b)));
		int num = start;
		List<JsonValue> list = new List<JsonValue>();
		for (; func(num, end); num += step)
		{
			list.Add(json[num]);
		}
		return list;
	}

	private static int _ResolveIndex(int index, int count)
	{
		if (index >= 0)
		{
			return Math.Min(index, count);
		}
		return count + index;
	}
}
