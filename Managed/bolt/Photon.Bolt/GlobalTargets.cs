namespace Photon.Bolt;

[Documentation]
public enum GlobalTargets : byte
{
	Everyone = 2,
	Others = 4,
	AllClients = 8,
	OnlyServer = 6,
	OnlySelf = 12
}
