using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace Sony.NP;

internal static class NpRequestsThread
{
	private static Thread requestsThread;

	private static bool stopThread = false;

	private static Semaphore workLoad = new Semaphore(0, 1000);

	[DllImport("UnityNpToolkit2")]
	private static extern bool PrxPollFirstRequest();

	public static void Start()
	{
		stopThread = false;
		requestsThread = new Thread(RunProc);
		requestsThread.Name = "Requests Thread";
		requestsThread.Start();
	}

	private static void RunProc()
	{
		workLoad.WaitOne();
		while (!stopThread)
		{
			if (PrxPollFirstRequest())
			{
				workLoad.WaitOne();
			}
			else
			{
				Thread.Sleep(1000);
			}
		}
	}

	public static void Execute()
	{
		workLoad.Release();
	}

	public static void Stop()
	{
		stopThread = true;
		workLoad.Release();
	}

	[MonoPInvokeCallback(typeof(Main.OnPrxCallbackEvent))]
	public static void OnPrxNpRequestEvent()
	{
		Execute();
	}
}
