using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IOrganizationMembershipCollection : IReadOnlyOrganizationMembershipCollection, IReadOnlyCollection<IOrganizationMembership>, IEnumerable<IOrganizationMembership>, IEnumerable, IRefreshable
{
	Task<IOrganizationMembership> Add(IMember member, OrganizationMembershipType membership, CancellationToken ct = default(CancellationToken));

	Task Remove(IMember member, CancellationToken ct = default(CancellationToken));
}
