using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICommentCollection : IReadOnlyActionCollection, IReadOnlyCollection<IAction>, IEnumerable<IAction>, IEnumerable, IRefreshable
{
	Task<IAction> Add(string text, CancellationToken ct = default(CancellationToken));
}
