namespace Photon.Bolt;

[Documentation]
public enum EntityTargets : byte
{
	Everyone = 1,
	EveryoneExceptController = 5,
	EveryoneExceptOwner = 3,
	EveryoneExceptOwnerAndController = 13,
	OnlyController = 7,
	OnlyControllerAndOwner = 15,
	OnlyOwner = 9,
	OnlySelf = 11
}
