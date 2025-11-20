using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyMemberCollection : IReadOnlyCollection<IMember>, IEnumerable<IMember>, IEnumerable, IRefreshable
{
	IMember this[string key] { get; }

	void Filter(MemberFilter filter);

	void Filter(IEnumerable<MemberFilter> filters);
}
