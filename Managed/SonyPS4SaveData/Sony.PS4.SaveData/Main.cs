using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sony.PS4.SaveData;

public class Main
{
	public delegate void EventHandler(SaveDataCallbackEvent npEvent);

	internal static InitResult initResult;

	public static event EventHandler OnAsyncEvent;

	[DllImport("SaveData")]
	private static extern void PrxSaveDataInitialize(out NativeInitResult nativeResult, out APIResult result);

	[DllImport("SaveData")]
	private static extern void PrxSaveDataTerminate(out APIResult result);

	public static InitResult Initialize(InitSettings initSettings)
	{
		NativeInitResult nativeResult = default(NativeInitResult);
		PrxSaveDataInitialize(out nativeResult, out var result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
		initResult.Initialise(nativeResult);
		ProcessQueueThread.Start(initSettings.affinity);
		DispatchQueueThread.Start(initSettings.affinity);
		Notifications.Start(initSettings.affinity);
		return initResult;
	}

	public static void Terminate()
	{
		PrxSaveDataTerminate(out var result);
		if (result.RaiseException)
		{
			throw new SaveDataException(result);
		}
		ProcessQueueThread.Stop();
		DispatchQueueThread.Stop();
		Notifications.Stop();
	}

	internal static void CallOnAsyncEvent(SaveDataCallbackEvent sdEvent)
	{
		try
		{
			Main.OnAsyncEvent(sdEvent);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception Occured in OnAsyncEvent handler : " + ex.Message);
			Console.WriteLine(ex.StackTrace);
			throw;
		}
	}

	internal static void ProcessInternalResponses(RequestBase request, ResponseBase response)
	{
		if (request == null)
		{
			return;
		}
		if (request.functionType == FunctionTypes.Mount)
		{
			if (response.ReturnCode == ReturnCodes.SUCCESS)
			{
				Mounting.MountResponse mountResponse = response as Mounting.MountResponse;
				Mounting.AddMountPoint(mountResponse.MountPoint);
			}
		}
		else if (request.functionType == FunctionTypes.Unmount && response.ReturnCode == ReturnCodes.SUCCESS)
		{
			Mounting.UnmountRequest unmountRequest = request as Mounting.UnmountRequest;
			Mounting.RemoveMountPoint(unmountRequest.MountPointName);
		}
	}

	public static bool AbortRequest(int requestId)
	{
		return ProcessQueueThread.AbortRequest(requestId);
	}

	public static List<PendingRequest> GetPendingRequests()
	{
		return ProcessQueueThread.PendingRequests;
	}
}
