using System;

namespace Photon.Bolt;

[Flags]
public enum BoltNetworkModes
{
	None = 0,
	Server = 1,
	Client = 2,
	Shutdown = 3
}
