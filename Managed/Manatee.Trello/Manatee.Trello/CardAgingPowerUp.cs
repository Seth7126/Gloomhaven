using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public sealed class CardAgingPowerUp : IPowerUp, ICacheable, IRefreshable
{
	public string Id => "55a5d917446f517774210012";

	public string Name => "Card Aging";

	public bool? IsPublic => true;

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return Task.CompletedTask;
	}
}
