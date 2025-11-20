using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IBoardLabelCollection : IReadOnlyCollection<ILabel>, IEnumerable<ILabel>, IEnumerable, IRefreshable
{
	Task<ILabel> Add(string name, LabelColor? color, CancellationToken ct = default(CancellationToken));

	void Filter(LabelColor labelColor);
}
