using System;
using System.Diagnostics;
using UnityEngine;

namespace SM.Utils;

public static class LogUtils
{
	[Conditional("ENABLE_LOGS")]
	public static void Log(string logMsg)
	{
		Debug.Log(logMsg);
	}

	[Conditional("ENABLE_LOGS")]
	public static void Log(string logMsg, UnityEngine.Object context)
	{
		Debug.Log(logMsg, context);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(string logMsg)
	{
		Debug.LogWarning(logMsg);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(string logMsg, UnityEngine.Object context)
	{
		Debug.LogWarning(logMsg, context);
	}

	[Conditional("ENABLE_LOGS")]
	public static void Assert(bool condition, string logMsg)
	{
	}

	[Conditional("ENABLE_LOGS")]
	public static void Assert(bool condition, string logMsg, UnityEngine.Object context)
	{
	}

	public static void LogError(string logMsg)
	{
		Debug.LogError(logMsg);
	}

	public static void LogError(string logMsg, UnityEngine.Object context)
	{
		Debug.LogError(logMsg, context);
	}

	public static void LogException(Exception exception)
	{
		Debug.LogException(exception);
	}

	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		Debug.LogException(exception, context);
	}
}
