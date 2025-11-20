using System;

namespace Photon.Bolt;

public interface IControllableState : IState, IDisposable
{
	int ControllableID { get; set; }

	int ControllerID { get; set; }
}
