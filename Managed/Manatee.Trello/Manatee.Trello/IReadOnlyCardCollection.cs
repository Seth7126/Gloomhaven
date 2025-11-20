using System;
using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyCardCollection : IReadOnlyCollection<ICard>, IEnumerable<ICard>, IEnumerable, IRefreshable
{
	ICard this[string key] { get; }

	void Filter(CardFilter filter);

	void Filter(DateTime? start, DateTime? end);
}
