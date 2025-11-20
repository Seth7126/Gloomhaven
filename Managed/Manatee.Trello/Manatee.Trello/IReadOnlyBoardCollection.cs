using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyBoardCollection : IReadOnlyCollection<IBoard>, IEnumerable<IBoard>, IEnumerable, IRefreshable
{
	IBoard this[string key] { get; }

	void Filter(BoardFilter filter);
}
