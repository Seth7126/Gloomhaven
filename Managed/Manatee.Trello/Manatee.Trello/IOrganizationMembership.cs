using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IOrganizationMembership : ICacheable, IRefreshable
{
	DateTime CreationDate { get; }

	bool? IsUnconfirmed { get; }

	IMember Member { get; }

	OrganizationMembershipType? MemberType { get; set; }

	event Action<IOrganizationMembership, IEnumerable<string>> Updated;
}
