using System.Collections.Generic;

namespace Manatee.Trello;

public interface IMemberSearch : IRefreshable
{
	IEnumerable<MemberSearchResult> Results { get; }
}
