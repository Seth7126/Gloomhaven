using Hydra.Api.Push;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class PushTokenWrapper : IHydraSdkStateWrapper
{
	public PushToken Token { get; private set; }

	public PushTokenWrapper(PushToken token)
	{
		Token = token;
	}
}
