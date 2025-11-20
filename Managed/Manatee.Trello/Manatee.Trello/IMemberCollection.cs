using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IMemberCollection : IReadOnlyMemberCollection, IReadOnlyCollection<IMember>, IEnumerable<IMember>, IEnumerable, IRefreshable
{
	Task Add(IMember member, CancellationToken ct = default(CancellationToken));

	Task Remove(IMember member, CancellationToken ct = default(CancellationToken));
}
