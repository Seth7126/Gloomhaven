using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyListCollection : IReadOnlyCollection<IList>, IEnumerable<IList>, IEnumerable, IRefreshable
{
	IList this[string key] { get; }

	void Filter(ListFilter filter);
}
