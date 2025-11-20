using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoardCollection : IReadOnlyBoardCollection, IReadOnlyCollection<IBoard>, IEnumerable<IBoard>, IEnumerable, IRefreshable
{
	Task<IBoard> Add(string name, string description = null, IBoard source = null, CancellationToken ct = default(CancellationToken));
}
