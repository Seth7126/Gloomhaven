using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICheckItemCollection : IReadOnlyCheckItemCollection, IReadOnlyCollection<ICheckItem>, IEnumerable<ICheckItem>, IEnumerable, IRefreshable
{
	Task<ICheckItem> Add(string name, CancellationToken ct = default(CancellationToken));
}
