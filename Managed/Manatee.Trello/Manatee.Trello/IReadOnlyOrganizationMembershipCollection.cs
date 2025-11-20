using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyOrganizationMembershipCollection : IReadOnlyCollection<IOrganizationMembership>, IEnumerable<IOrganizationMembership>, IEnumerable, IRefreshable
{
	IOrganizationMembership this[string key] { get; }

	void Filter(MembershipFilter filter);

	void Filter(IEnumerable<MembershipFilter> filters);
}
