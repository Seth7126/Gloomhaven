using System;
using System.Collections.Generic;
using System.Linq;

namespace Manatee.Json.Path.SearchParameters;

internal class NameSearchParameter : IJsonPathSearchParameter, IEquatable<NameSearchParameter>
{
	private readonly string _name;

	public NameSearchParameter(string name)
	{
		_name = name;
	}

	public IEnumerable<JsonValue> Find(IEnumerable<JsonValue> json, JsonValue root)
	{
		JsonArray jsonArray = new JsonArray();
		foreach (JsonValue item in json)
		{
			_Find(item, jsonArray);
		}
		return jsonArray;
	}

	public override string? ToString()
	{
		if (!_name.Any((char c) => !char.IsLetterOrDigit(c)) && !string.IsNullOrWhiteSpace(_name))
		{
			return _name;
		}
		return "'" + _name + "'";
	}

	public bool Equals(NameSearchParameter? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return string.Equals(_name, other._name);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as NameSearchParameter);
	}

	public override int GetHashCode()
	{
		return _name?.GetHashCode() ?? 0;
	}

	private void _Find(JsonValue value, JsonArray results)
	{
		switch (value.Type)
		{
		case JsonValueType.Object:
		{
			if (value.Object.TryGetValue(_name, out JsonValue value2))
			{
				results.Add(value2);
			}
			{
				foreach (JsonValue value3 in value.Object.Values)
				{
					_Find(value3, results);
				}
				break;
			}
		}
		case JsonValueType.Array:
		{
			foreach (JsonValue item in value.Array)
			{
				_Find(item, results);
			}
			break;
		}
		}
	}
}
