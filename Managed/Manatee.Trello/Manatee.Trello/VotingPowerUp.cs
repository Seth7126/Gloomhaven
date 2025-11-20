using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public sealed class VotingPowerUp : IPowerUp, ICacheable, IRefreshable
{
	public string Id => "55a5d917446f517774210013";

	public string Name => "Voting";

	public bool? IsPublic => true;

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return Task.CompletedTask;
	}
}
