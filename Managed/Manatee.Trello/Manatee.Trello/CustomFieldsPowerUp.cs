using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public sealed class CustomFieldsPowerUp : IPowerUp, ICacheable, IRefreshable
{
	public string Id => "56d5e249a98895a9797bebb9";

	public string Name => "Custom Fields";

	public bool? IsPublic => true;

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return Task.CompletedTask;
	}
}
