using System.Threading;

namespace System.Drawing;

internal class WorkerThread
{
	private EventHandler frameChangeHandler;

	private AnimateEventArgs animateEventArgs;

	private int[] delay;

	public WorkerThread(EventHandler frmChgHandler, AnimateEventArgs aniEvtArgs, int[] delay)
	{
		frameChangeHandler = frmChgHandler;
		animateEventArgs = aniEvtArgs;
		this.delay = delay;
	}

	public void LoopHandler()
	{
		try
		{
			int num = 0;
			while (true)
			{
				Thread.Sleep(delay[num++]);
				frameChangeHandler(null, animateEventArgs);
				if (num == delay.Length)
				{
					num = 0;
				}
			}
		}
		catch (ThreadAbortException)
		{
			Thread.ResetAbort();
		}
	}
}
