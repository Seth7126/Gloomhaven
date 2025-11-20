using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hydra.Api.Facts;

namespace Hydra.Sdk.Components.Facts.Core;

public class FactGlobalContext
{
	private readonly ConcurrentDictionary<string, string> _context;

	public List<FactsContext> Get()
	{
		return _context.Select((KeyValuePair<string, string> c) => new FactsContext
		{
			PropertyName = c.Key,
			PropertyValue = c.Value
		}).ToList();
	}

	public FactGlobalContext()
	{
		_context = new ConcurrentDictionary<string, string>();
	}

	public bool TryAdd(string name, string value)
	{
		if (!_context.TryGetValue(name, out var value2))
		{
			_context.TryAdd(name, value);
			return true;
		}
		if (value2 != value)
		{
			_context[name] = value;
			return true;
		}
		return false;
	}
}
