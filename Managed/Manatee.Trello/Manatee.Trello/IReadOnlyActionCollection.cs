using System;
using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyActionCollection : IReadOnlyCollection<IAction>, IEnumerable<IAction>, IEnumerable, IRefreshable
{
	void Filter(ActionType actionType);

	void Filter(IEnumerable<ActionType> actionTypes);

	void Filter(DateTime? start, DateTime? end);
}
