using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class StandaloneServerWrapper : IHydraSdkStateWrapper
{
	public string ServerToken { get; private set; }

	public StandaloneServerWrapper(string serverToken)
	{
		ServerToken = serverToken;
	}
}
