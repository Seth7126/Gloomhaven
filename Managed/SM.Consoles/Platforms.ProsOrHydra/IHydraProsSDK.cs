using System;
using System.Threading.Tasks;

namespace Platforms.ProsOrHydra;

public interface IHydraProsSDK
{
	bool IsOnlineState { get; }

	double ConnectionCheckCooldown { get; }

	Task RegisterComponents();

	Task<IGetEnvironmentInfoResponse> GetEnvironmentInfo();

	void DisableNetworkInBackground(bool value);

	Task DisposeAsync();

	void LogException(Exception exception);

	Task Tick();
}
