using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable, IRefreshable
{
	int? Limit { get; set; }

	T this[int index] { get; }
}
