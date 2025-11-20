using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hydra.Api.Facts;

namespace Hydra.Sdk.Components.Facts.Core;

public class FactContext
{
	private readonly ConcurrentDictionary<string, FactsContext> _context;

	public FactContext()
	{
		_context = new ConcurrentDictionary<string, FactsContext>();
	}

	public FactsContext Get(string category)
	{
		if (!_context.ContainsKey(category))
		{
			return null;
		}
		return _context[category];
	}

	public IEnumerable<FactsContext> GetAll()
	{
		return _context.Select((KeyValuePair<string, FactsContext> s) => s.Value);
	}

	public void Remove(string category)
	{
		if (_context.ContainsKey(category))
		{
			_context.TryRemove(category, out var _);
		}
	}

	public void RemoveAll()
	{
		_context.Clear();
	}

	public bool Set(string category, FactsContext value)
	{
		if (!_context.ContainsKey(category))
		{
			_context.TryAdd(category, value);
			return true;
		}
		if (!_context[category].Equals(value))
		{
			_context[category] = value;
			return true;
		}
		return false;
	}
}
