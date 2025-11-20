using System.Threading;

namespace Photon.Bolt.Internal;

internal abstract class ControlCommand
{
	public int PendingFrames;

	public int FinishedFrames;

	public ControlState State;

	public ManualResetEvent FinishedEvent;

	public ControlCommand()
	{
		State = ControlState.Pending;
		FinishedEvent = new ManualResetEvent(initialState: false);
		PendingFrames = 2;
		FinishedFrames = 2;
	}

	public abstract void Run();

	public abstract void Done();
}
