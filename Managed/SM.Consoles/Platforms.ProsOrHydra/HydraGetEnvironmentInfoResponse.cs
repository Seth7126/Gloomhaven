using Hydra.Api.EndpointDispatcher;

namespace Platforms.ProsOrHydra;

public class HydraGetEnvironmentInfoResponse : IGetEnvironmentInfoResponse
{
	private readonly GetEnvironmentInfoResponse _getEnvironmentInfoResponse;

	public bool IsReadyStatus => _getEnvironmentInfoResponse.Environment.Status == EnvironmentStatus.Ready;

	public HydraGetEnvironmentInfoResponse(GetEnvironmentInfoResponse getEnvironmentInfoResponse)
	{
		_getEnvironmentInfoResponse = getEnvironmentInfoResponse;
	}
}
