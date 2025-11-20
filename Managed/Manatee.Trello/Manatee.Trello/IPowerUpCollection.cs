using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface IPowerUpCollection : IReadOnlyCollection<IPowerUp>, IEnumerable<IPowerUp>, IEnumerable, IRefreshable
{
	Task EnablePowerUp(IPowerUp powerUp, CancellationToken ct = default(CancellationToken));

	Task DisablePowerUp(IPowerUp powerUp, CancellationToken ct = default(CancellationToken));
}
