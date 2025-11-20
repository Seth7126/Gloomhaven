#define ENABLE_LOGS
using Photon.Bolt.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace FFSNet;

public static class Console
{
	public static UnityAction<string, bool> OnLogDebug { get; set; }

	public static UnityAction<string, bool> OnLog { get; set; }

	public static UnityAction<string, bool> OnLogInfo { get; set; }

	public static UnityAction<string, bool> OnLogCoreInfo { get; set; }

	public static UnityAction<string, bool> OnLogWarning { get; set; }

	public static UnityAction<string, string, bool> OnLogError { get; set; }

	public static void LogDebug(string message, bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message, Color.magenta);
		}
		Debug.Log(message);
		OnLogDebug?.Invoke(message, customFlag);
	}

	public static void Log(string message, bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message, Color.white);
		}
		Debug.Log(message);
		OnLog?.Invoke(message, customFlag);
	}

	public static void LogInfo(string message, bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message, Color.green);
		}
		Debug.Log(message);
		OnLogInfo?.Invoke(message, customFlag);
	}

	public static void LogCoreInfo(string message, bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message, Color.cyan);
		}
		Debug.Log(message);
		OnLogCoreInfo?.Invoke(message, customFlag);
	}

	public static void LogWarning(string message, bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message, Color.yellow);
		}
		Debug.LogWarning(message);
		OnLogWarning?.Invoke(message, customFlag);
	}

	public static void LogError(string errorCode, string message, string stackTrace = "", bool customFlag = false)
	{
		if (FFSNetwork.IsOnline)
		{
			BoltConsole.Write(message + "\n" + stackTrace, Color.red);
		}
		Debug.LogError(message + "\n" + stackTrace);
		if (errorCode != string.Empty)
		{
			OnLogError?.Invoke(errorCode, message, customFlag);
		}
	}
}
