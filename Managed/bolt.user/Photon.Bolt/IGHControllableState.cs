using System;

namespace Photon.Bolt;

public interface IGHControllableState : IControllableState, IState, IDisposable
{
	IProtocolToken Level { get; set; }

	IProtocolToken PerkPoints { get; set; }

	IProtocolToken ActivePerks { get; set; }

	IProtocolToken CardInventory { get; set; }

	IProtocolToken ItemInventory { get; set; }

	IProtocolToken StartingTile { get; set; }

	IProtocolToken StartRoundCards { get; set; }
}
