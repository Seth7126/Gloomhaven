using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IOrganizationCollection : IReadOnlyOrganizationCollection, IReadOnlyCollection<IOrganization>, IEnumerable<IOrganization>, IEnumerable, IRefreshable
{
	Task<IOrganization> Add(string displayName, string description = null, string name = null, CancellationToken ct = default(CancellationToken));
}
