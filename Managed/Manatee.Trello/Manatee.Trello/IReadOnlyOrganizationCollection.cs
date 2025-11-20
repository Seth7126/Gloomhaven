using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyOrganizationCollection : IReadOnlyCollection<IOrganization>, IEnumerable<IOrganization>, IEnumerable, IRefreshable
{
	IOrganization this[string key] { get; }

	void Filter(OrganizationFilter filter);
}
