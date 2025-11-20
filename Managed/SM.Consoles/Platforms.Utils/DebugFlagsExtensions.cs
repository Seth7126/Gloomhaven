#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SM.Utils;
using UnityEngine;

namespace Platforms.Utils;

public static class DebugFlagsExtensions
{
	[Conditional("ENABLE_LOGS")]
	public static void Log(this DebugFlags flags, string message, UnityEngine.Object context = null)
	{
		if (flags.HasFlag(DebugFlags.Log))
		{
			if (context == null)
			{
				LogUtils.Log(message);
			}
			else
			{
				LogUtils.Log(message, context);
			}
		}
	}

	[Conditional("ENABLE_LOGS")]
	public static void Log(this DebugFlags flags, IEnumerable<string> messages, UnityEngine.Object context = null)
	{
		if (!flags.HasFlag(DebugFlags.Log))
		{
			return;
		}
		if (context == null)
		{
			foreach (string message in messages)
			{
				LogUtils.Log(message);
			}
			return;
		}
		foreach (string message2 in messages)
		{
			LogUtils.Log(message2, context);
		}
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(this DebugFlags flags, string message, UnityEngine.Object context = null)
	{
		if (flags.HasFlag(DebugFlags.Warning))
		{
			if (context == null)
			{
				LogUtils.LogWarning(message);
			}
			else
			{
				LogUtils.LogWarning(message, context);
			}
		}
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(this DebugFlags flags, IEnumerable<string> messages, UnityEngine.Object context = null)
	{
		if (!flags.HasFlag(DebugFlags.Warning))
		{
			return;
		}
		if (context == null)
		{
			foreach (string message in messages)
			{
				LogUtils.LogWarning(message);
			}
			return;
		}
		foreach (string message2 in messages)
		{
			LogUtils.LogWarning(message2, context);
		}
	}

	public static void LogError(this DebugFlags flags, string message, UnityEngine.Object context = null)
	{
		if (flags.HasFlag(DebugFlags.Error))
		{
			if (context == null)
			{
				LogUtils.LogError(message);
			}
			else
			{
				LogUtils.LogError(message, context);
			}
		}
	}

	public static void LogError(this DebugFlags flags, IEnumerable<string> messages, UnityEngine.Object context = null)
	{
		if (!flags.HasFlag(DebugFlags.Error))
		{
			return;
		}
		if (context == null)
		{
			foreach (string message in messages)
			{
				LogUtils.LogError(message);
			}
			return;
		}
		foreach (string message2 in messages)
		{
			LogUtils.LogError(message2, context);
		}
	}

	public static void LogException(this DebugFlags flags, Exception exception)
	{
		LogUtils.LogException(exception);
	}
}
