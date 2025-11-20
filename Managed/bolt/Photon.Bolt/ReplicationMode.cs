namespace Photon.Bolt;

internal enum ReplicationMode
{
	EveryoneExceptController,
	Everyone,
	OnlyOwnerAndController,
	LocalForEachPlayer
}
