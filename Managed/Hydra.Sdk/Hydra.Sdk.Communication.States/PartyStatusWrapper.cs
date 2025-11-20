using Hydra.Api.Push.Presence;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class PartyStatusWrapper : IHydraSdkStateWrapper
{
	public PresencePartyUpdateVersion Status { get; set; }
}
