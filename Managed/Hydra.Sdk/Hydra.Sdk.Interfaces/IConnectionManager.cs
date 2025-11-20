using Hydra.Api.EndpointDispatcher;
using Hydra.Sdk.Generated;

namespace Hydra.Sdk.Interfaces;

public interface IConnectionManager
{
	T GetConnection<T>() where T : ClientBase<T>;

	IHydraSdkChannel GetChannel(string serviceName);

	void AddConnections(params EndpointInfo[] endpointInfos);

	void UpdateToken(string token);
}
