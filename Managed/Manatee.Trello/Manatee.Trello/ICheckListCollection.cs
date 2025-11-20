using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ICheckListCollection : IReadOnlyCheckListCollection, IReadOnlyCollection<ICheckList>, IEnumerable<ICheckList>, IEnumerable, IRefreshable
{
	Task<ICheckList> Add(string name, ICheckList source = null, CancellationToken ct = default(CancellationToken));
}
