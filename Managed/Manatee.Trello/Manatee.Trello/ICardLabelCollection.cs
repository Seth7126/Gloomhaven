using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICardLabelCollection : IReadOnlyCollection<ILabel>, IEnumerable<ILabel>, IEnumerable, IRefreshable
{
	Task Add(ILabel label, CancellationToken ct = default(CancellationToken));

	Task Remove(ILabel label, CancellationToken ct = default(CancellationToken));
}
