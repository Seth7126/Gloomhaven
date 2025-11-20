using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IRefreshable
{
	Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken));
}
