using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoardBackgroundCollection : IReadOnlyCollection<IBoardBackground>, IEnumerable<IBoardBackground>, IEnumerable, IRefreshable
{
	Task<IBoardBackground> Add(byte[] data, CancellationToken ct = default(CancellationToken));
}
