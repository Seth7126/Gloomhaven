using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IListCollection : IReadOnlyListCollection, IReadOnlyCollection<IList>, IEnumerable<IList>, IEnumerable, IRefreshable
{
	Task<IList> Add(string name, Position position = null, CancellationToken ct = default(CancellationToken));
}
