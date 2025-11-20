using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IStarredBoardCollection : IReadOnlyCollection<IStarredBoard>, IEnumerable<IStarredBoard>, IEnumerable, IRefreshable
{
	Task<IStarredBoard> Add(IBoard board, Position position = null, CancellationToken ct = default(CancellationToken));
}
