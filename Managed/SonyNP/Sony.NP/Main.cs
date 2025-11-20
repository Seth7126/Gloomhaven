using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sony.NP;

public class Main
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OnPrxCallbackEvent();

	[StructLayout(LayoutKind.Sequential)]
	internal class ValidationChecks
	{
		internal uint expectedNumFunctionTypes;

		public void Init()
		{
			if (initResult.sceSDKVersion >= 122683392)
			{
				expectedNumFunctionTypes = 124u;
			}
			else if (initResult.sceSDKVersion >= 117440512)
			{
				expectedNumFunctionTypes = 136u;
			}
			else if (initResult.sceSDKVersion >= 105906176)
			{
				expectedNumFunctionTypes = 137u;
			}
			else if (initResult.sceSDKVersion >= 83886080)
			{
				expectedNumFunctionTypes = 135u;
			}
			else
			{
				expectedNumFunctionTypes = 118u;
			}
		}
	}

	public delegate void EventHandler(NpCallbackEvent npEvent);

	public class LaunchAppEventResponse : ResponseBase
	{
		internal string args;

		public string Args => args;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.LaunchAppEventBegin);
			byte[] data = null;
			if (memoryBuffer.ReadData(ref data) == 0)
			{
				args = "";
			}
			else
			{
				args = Encoding.UTF8.GetString(data, 0, data.Length);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.LaunchAppEventEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	internal static InitResult initResult;

	public static event EventHandler OnAsyncEvent;

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxInitialize(InitToolkit initParams, out NativeInitResult initResult, OnPrxCallbackEvent toolkitEventCallback, OnPrxCallbackEvent npRequestEventCallback, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxValidateToolkit(ValidationChecks checks, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern void PrxShutDown();

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxUpdate();

	[DllImport("UnityNpToolkit2")]
	private static extern bool PrxAbortRequest(uint npRequestId, out APIResult result);

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxGetMemoryPoolStats(out MemoryPoolStats result);

	public static InitResult Initialize(InitToolkit initParams)
	{
		initParams.CheckValid();
		OnPrxCallbackEvent toolkitEventCallback = PopulateThread.OnPrxNpToolkitEvent;
		OnPrxCallbackEvent npRequestEventCallback = NpRequestsThread.OnPrxNpRequestEvent;
		NativeInitResult nativeResult = default(NativeInitResult);
		PrxInitialize(initParams, out nativeResult, toolkitEventCallback, npRequestEventCallback, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		initResult.Initialise(nativeResult);
		ValidationChecks validationChecks = new ValidationChecks();
		validationChecks.Init();
		PrxValidateToolkit(validationChecks, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		PopulateThread.Start();
		NpRequestsThread.Start();
		return initResult;
	}

	internal static void InternalEventHandler(NpCallbackEvent npEvent)
	{
		if (npEvent.service == ServiceTypes.Notification && npEvent.apiCalled == FunctionTypes.NotificationAborted)
		{
			PendingAsyncRequestList.RequestHasBeenAborted(npEvent.npRequestId);
		}
	}

	internal static void CallOnAsyncEvent(NpCallbackEvent npEvent)
	{
		InternalEventHandler(npEvent);
		try
		{
			Main.OnAsyncEvent(npEvent);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception Occured in OnAsyncEvent handler : " + ex.Message);
			Console.WriteLine(ex.StackTrace);
			throw;
		}
	}

	public static void Update()
	{
		PrxUpdate();
	}

	private static void PumpAsyncEvents()
	{
		if (Main.OnAsyncEvent != null)
		{
			for (NpCallbackEvent npCallbackEvent = PendingCallbackQueue.PopEvent(); npCallbackEvent != null; npCallbackEvent = PendingCallbackQueue.PopEvent())
			{
				InternalEventHandler(npCallbackEvent);
				Main.OnAsyncEvent(npCallbackEvent);
			}
		}
	}

	public static int GetMemoryPoolStats(out MemoryPoolStats res)
	{
		return PrxGetMemoryPoolStats(out res);
	}

	public static void ShutDown()
	{
		PopulateThread.Stop();
		NpRequestsThread.Stop();
		PendingAsyncRequestList.Shutdown();
		PrxShutDown();
	}

	public static List<PendingRequest> GetPendingRequests()
	{
		return PendingAsyncRequestList.PendingRequests;
	}

	public static bool AbortRequest(uint npRequestId)
	{
		if (!PendingAsyncRequestList.IsPending(npRequestId))
		{
			return false;
		}
		PrxAbortRequest(npRequestId, out var result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		PendingAsyncRequestList.MarkRequestAsAborting(npRequestId);
		return true;
	}
}
