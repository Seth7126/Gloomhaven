using System;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace Sony.NP;

internal static class PopulateThread
{
	private static Thread populateThread;

	private static bool stopThread = false;

	private static Semaphore workLoad = new Semaphore(0, 1000);

	[DllImport("UnityNpToolkit2")]
	private static extern bool PrxPopFirstResponse(out int service, out int apiCalled, out uint npRequestId, out int userId, out int customReturnCode);

	public static void Start()
	{
		stopThread = false;
		populateThread = new Thread(RunProc);
		populateThread.Name = "Sony Np";
		populateThread.Start();
	}

	private static void RunProc()
	{
		workLoad.WaitOne();
		Core.UserServiceUserId userId = default(Core.UserServiceUserId);
		while (!stopThread)
		{
			ServiceTypes serviceTypes = ServiceTypes.Invalid;
			FunctionTypes functionTypes = FunctionTypes.Invalid;
			if (PrxPopFirstResponse(out var service, out var apiCalled, out var npRequestId, out userId.id, out var customReturnCode))
			{
				RequestBase requestBase = null;
				NpCallbackEvent npCallbackEvent = null;
				try
				{
					serviceTypes = Compatibility.ConvertServiceToEnum(service);
					functionTypes = Compatibility.ConvertFunctionToEnum(apiCalled);
					npCallbackEvent = new NpCallbackEvent();
					if (serviceTypes == ServiceTypes.Notification)
					{
						npCallbackEvent.response = Notifications.CreateNotificationResponse(functionTypes);
					}
					else
					{
						requestBase = PendingAsyncRequestList.RemoveRequest(npRequestId);
						npCallbackEvent.response = PendingAsyncResponseList.FindAndRemoveResponse(npRequestId);
						if (npCallbackEvent.response == null)
						{
							Console.WriteLine("Error : PopulateThread.RunProc : Can't find response object for Request " + npRequestId);
						}
					}
					if (npCallbackEvent.response != null)
					{
						npCallbackEvent.response.PopulateFromNative(npRequestId, functionTypes, requestBase);
						if (customReturnCode != 0)
						{
							npCallbackEvent.response.returnCode = customReturnCode;
						}
					}
					npCallbackEvent.service = serviceTypes;
					npCallbackEvent.apiCalled = functionTypes;
					npCallbackEvent.npRequestId = npRequestId;
					npCallbackEvent.userId = userId;
					npCallbackEvent.request = requestBase;
					Main.CallOnAsyncEvent(npCallbackEvent);
				}
				catch (NpToolkitException ex)
				{
					Console.WriteLine("Toolkit Exception - PopulateThread.RunProc : " + ex.ExtendedMessage);
					Console.WriteLine(ex.StackTrace);
					Console.WriteLine(string.Concat("Toolkit Exception : service = ", serviceTypes, " : apiCalled = ", functionTypes, "(", (int)functionTypes, ") : npRequestId = ", npRequestId, " : userId = ", userId.id));
					if (requestBase != null)
					{
						Console.WriteLine("Toolkit Exception - Caused by Request : " + requestBase.FunctionType);
					}
					if (npCallbackEvent != null && npCallbackEvent.response != null)
					{
						Console.WriteLine("Toolkit Exception - Response Type = " + npCallbackEvent.response.GetType().ToString());
					}
				}
				catch (Exception ex2)
				{
					Console.WriteLine("Exception - PopulateThread.RunProc : " + ex2.Message);
					Console.WriteLine(ex2.StackTrace);
					Console.WriteLine(string.Concat("Toolkit Exception : service = ", serviceTypes, " : apiCalled = ", functionTypes, "(", (int)functionTypes, ") : npRequestId = ", npRequestId, " : userId = ", userId.id));
					if (requestBase != null)
					{
						Console.WriteLine("Toolkit Exception - Caused by Request : " + requestBase.FunctionType);
					}
					else
					{
						Console.WriteLine("Toolkit Exception - No request data available");
					}
				}
			}
			workLoad.WaitOne();
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
	public static void OnPrxNpToolkitEvent()
	{
		Execute();
	}
}
