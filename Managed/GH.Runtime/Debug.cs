#define ENABLE_LOGS
using System;
using System.Diagnostics;
using SM.Utils;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.Internal;

public class Debug
{
	private const string TAG_GUI_FORMAT = "<color=#00FFF5>[GUI]</color> {0}";

	private const string TAG_CONTROLLER_FORMAT = "<color=#A3DA77>[CONTROLLER]</color> {0}";

	[Conditional("ENABLE_LOGS")]
	public static void LogGUI(object message)
	{
		LogFormat("<color=#00FFF5>[GUI]</color> {0}", message);
	}

	public static void LogErrorGUI(object message)
	{
		LogErrorFormat("<color=#00FFF5>[GUI]</color> {0}", message);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarningGUI(object message)
	{
		LogWarningFormat("<color=#00FFF5>[GUI]</color> {0}", message);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogController(object message)
	{
		LogFormat("<color=#A3DA77>[CONTROLLER]</color> {0}", message);
	}

	public static void LogErrorController(object message)
	{
		LogErrorFormat("<color=#A3DA77>[CONTROLLER]</color> {0}", message);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarningController(object message)
	{
		LogWarningFormat("<color=#A3DA77>[CONTROLLER]</color> {0}", message);
	}

	[Conditional("ENABLE_LOGS")]
	public static void Log(object message)
	{
		LogUtils.Log(message.ToString());
	}

	[Conditional("ENABLE_LOGS")]
	public static void Log(object message, UnityEngine.Object context)
	{
		LogUtils.Log(message.ToString(), context);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(object message)
	{
		LogUtils.LogWarning(message.ToString());
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarning(object message, UnityEngine.Object context)
	{
		LogUtils.LogWarning(message.ToString(), context);
	}

	public static void LogError(object message)
	{
		SimpleLog.AddToSimpleLog((string)message, sendToClientLog: false);
		LogUtils.LogError(message.ToString());
	}

	public static void LogError(object message, UnityEngine.Object context)
	{
		SimpleLog.AddToSimpleLog((string)message, sendToClientLog: false);
		LogUtils.LogError(message.ToString(), context);
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogFormat(string format, params object[] args)
	{
		LogUtils.Log(string.Format(format, args));
	}

	[Conditional("ENABLE_LOGS")]
	public static void LogWarningFormat(string format, params object[] args)
	{
		LogUtils.LogWarning(string.Format(format, args));
	}

	public static void LogErrorFormat(string format, params object[] args)
	{
		LogUtils.LogError(string.Format(format, args));
	}

	[Conditional("ENABLE_LOGS")]
	public static void Assert(bool condition)
	{
		LogUtils.Assert(condition, "Fail");
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		UnityEngine.Debug.DrawLine(start, end, color);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawLine(Vector3 start, Vector3 end)
	{
		UnityEngine.Debug.DrawLine(start, end);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawLine(Vector3 start, Vector3 end, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
		UnityEngine.Debug.DrawRay(start, dir, color);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
	{
		UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
	}

	[Conditional("ENABLE_LOGS")]
	public static void DrawRay(Vector3 start, Vector3 dir)
	{
		UnityEngine.Debug.DrawRay(start, dir);
	}

	public static string GetExceptionText(Exception ex)
	{
		string text = string.Empty;
		if (ex != null)
		{
			text = "\n" + ex.Message + "\n" + ex.StackTrace;
			if (ex.InnerException != null)
			{
				text += GetExceptionText(ex.InnerException);
			}
		}
		return text;
	}
}
