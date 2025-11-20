using System;

namespace Photon.Bolt;

public interface IPlayerState : IState, IDisposable
{
	int LatestProcessedActionID { get; set; }
}
