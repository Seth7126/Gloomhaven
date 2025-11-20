using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyCheckItemCollection : IReadOnlyCollection<ICheckItem>, IEnumerable<ICheckItem>, IEnumerable, IRefreshable
{
	ICheckItem this[string key] { get; }
}
