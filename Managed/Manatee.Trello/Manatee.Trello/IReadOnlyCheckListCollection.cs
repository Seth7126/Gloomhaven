using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyCheckListCollection : IReadOnlyCollection<ICheckList>, IEnumerable<ICheckList>, IEnumerable, IRefreshable
{
	ICheckList this[string key] { get; }
}
