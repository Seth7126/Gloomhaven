using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IBoardMembership : ICacheable, IRefreshable
{
	DateTime CreationDate { get; }

	bool? IsDeactivated { get; }

	IMember Member { get; }

	BoardMembershipType? MemberType { get; set; }

	event Action<IBoardMembership, IEnumerable<string>> Updated;
}
