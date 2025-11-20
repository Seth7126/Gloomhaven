using Hydra.Api.Push.Presence;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class SessionUpdateWrapper : IHydraSdkStateWrapper
{
	public PresenceSessionUpdateVersion Status { get; set; }
}
