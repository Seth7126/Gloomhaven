using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface ICache : IEnumerable<ICacheable>, IEnumerable
{
	void Add(ICacheable obj);

	T Find<T>(string id) where T : class, ICacheable;

	void Remove(ICacheable obj);

	void Clear();
}
