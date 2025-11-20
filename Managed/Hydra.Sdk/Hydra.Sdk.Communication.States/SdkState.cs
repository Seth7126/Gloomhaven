using Hydra.Sdk.Enums;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class SdkState : IHydraSdkStateWrapper
{
	public OnlineState State { get; private set; }

	public bool Suspended { get; private set; }

	public SdkState(OnlineState state = OnlineState.Offline, bool suspended = false)
	{
		State = state;
		Suspended = suspended;
	}
}
