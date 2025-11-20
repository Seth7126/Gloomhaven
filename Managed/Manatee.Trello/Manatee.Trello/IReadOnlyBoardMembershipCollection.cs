using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyBoardMembershipCollection : IReadOnlyCollection<IBoardMembership>, IEnumerable<IBoardMembership>, IEnumerable, IRefreshable
{
	IBoardMembership this[string key] { get; }

	void Filter(MembershipFilter membership);

	void Filter(IEnumerable<MembershipFilter> memberships);
}
