using System;
using System.Threading.Tasks;

namespace Hydra.Sdk.Interfaces;

public interface IChannelManager
{
	void UpdateToken(string token);

	void AddEndpoint(string serviceName, Uri uri);

	bool TryGetChannel(string serviceName, out IHydraSdkChannel channel);

	Task ShutdownChannels();
}
