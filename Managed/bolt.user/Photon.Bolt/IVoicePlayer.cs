using System;

namespace Photon.Bolt;

public interface IVoicePlayer : IState, IDisposable
{
	NetworkTransform Transform { get; }

	int VoicePlayerID { get; set; }
}
