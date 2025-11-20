using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IDropDownOptionCollection : IReadOnlyCollection<IDropDownOption>, IEnumerable<IDropDownOption>, IEnumerable, IRefreshable
{
	Task<IDropDownOption> Add(string text, Position position, LabelColor? color = null, CancellationToken ct = default(CancellationToken));
}
