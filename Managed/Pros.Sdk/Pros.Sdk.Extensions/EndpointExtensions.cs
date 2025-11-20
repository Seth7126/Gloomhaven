using Hydra.Api.EndpointDispatcher;

namespace Pros.Sdk.Extensions;

public static class EndpointExtensions
{
	public static EndpointInfo ToHydraEndpointInfo(this EndpointInfo hydraEndpointInfo, string name)
	{
		return new EndpointInfo
		{
			Ip = hydraEndpointInfo.Ip,
			Name = name,
			Port = hydraEndpointInfo.Port,
			Scheme = ((hydraEndpointInfo.Scheme != EndpointScheme.Secured) ? EndpointScheme.Unsecured : EndpointScheme.Secured),
			Version = hydraEndpointInfo.Version
		};
	}
}
