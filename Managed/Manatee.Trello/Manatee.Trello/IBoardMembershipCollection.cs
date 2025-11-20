using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoardMembershipCollection : IReadOnlyBoardMembershipCollection, IReadOnlyCollection<IBoardMembership>, IEnumerable<IBoardMembership>, IEnumerable, IRefreshable
{
	Task<IBoardMembership> Add(IMember member, BoardMembershipType membership, CancellationToken ct = default(CancellationToken));

	Task Remove(IMember member, CancellationToken ct = default(CancellationToken));
}
