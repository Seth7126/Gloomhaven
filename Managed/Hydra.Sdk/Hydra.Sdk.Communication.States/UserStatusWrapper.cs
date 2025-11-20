using Hydra.Api.Push.Presence;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class UserStatusWrapper : IHydraSdkStateWrapper
{
	public PresenceUserUpdateVersion Status { get; set; }
}
