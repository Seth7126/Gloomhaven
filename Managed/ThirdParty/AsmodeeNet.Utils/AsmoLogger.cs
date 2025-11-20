using System;
using System.Collections;
using System.Threading;
using MiniJSON;
using UnityEngine;

namespace AsmodeeNet.Utils;

public static class AsmoLogger
{
	public enum Severity
	{
		Trace,
		Debug,
		Info,
		Notice,
		Warning,
		Error,
		Fatal
	}

	public delegate string LazyString();

	public static Severity? GlobalLogLevel;

	private static ThreadLocal<Severity> _threadLogLevel;

	public static Severity LogLevel => GlobalLogLevel ?? _threadLogLevel.Value;

	public static Severity ThreadLogLevel
	{
		get
		{
			return _threadLogLevel.Value;
		}
		set
		{
			_threadLogLevel.Value = value;
		}
	}

	public static bool IsDebugBuild { get; private set; }

	public static bool EnableTestMode { get; set; }

	static AsmoLogger()
	{
		GlobalLogLevel = null;
		_threadLogLevel = new ThreadLocal<Severity>(() => IsDebugBuild ? Severity.Debug : Severity.Warning);
		IsDebugBuild = UnityEngine.Debug.isDebugBuild;
		EnableTestMode = false;
		Info("AsmoLogger", "AsmoLogger initialized", new Hashtable
		{
			{ "IsDebugBuild", IsDebugBuild },
			{ "LogLevel", LogLevel }
		});
	}

	public static void Trace(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Trace, module, message, extraInfo);
	}

	public static void Trace(string module, LazyString lazyMessage, Hashtable extraInfo = null)
	{
		Log(Severity.Trace, module, lazyMessage, extraInfo);
	}

	public static void Debug(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Debug, module, message, extraInfo);
	}

	public static void Debug(string module, LazyString lazyMessage, Hashtable extraInfo = null)
	{
		Log(Severity.Debug, module, lazyMessage, extraInfo);
	}

	public static void Info(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Info, module, message, extraInfo);
	}

	public static void Info(string module, LazyString message, Hashtable extraInfo = null)
	{
		Log(Severity.Info, module, message, extraInfo);
	}

	public static void Notice(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Notice, module, message, extraInfo);
	}

	public static void Notice(string module, LazyString message, Hashtable extraInfo = null)
	{
		Log(Severity.Notice, module, message, extraInfo);
	}

	public static void Warning(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Warning, module, message, extraInfo);
	}

	public static void Warning(string module, LazyString message, Hashtable extraInfo = null)
	{
		Log(Severity.Warning, module, message, extraInfo);
	}

	public static void Error(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Error, module, message, extraInfo);
	}

	public static void Error(string module, LazyString message, Hashtable extraInfo = null)
	{
		Log(Severity.Error, module, message, extraInfo);
	}

	public static void Fatal(string module, string message, Hashtable extraInfo = null)
	{
		Log(Severity.Fatal, module, message, extraInfo);
	}

	public static void Fatal(string module, LazyString message, Hashtable extraInfo = null)
	{
		Log(Severity.Fatal, module, message, extraInfo);
	}

	public static bool CanLog(Severity severity)
	{
		return severity >= LogLevel;
	}

	public static void Log(Severity severity, string module, LazyString lazyMessage, Hashtable extraInfo = null)
	{
		if (CanLog(severity))
		{
			_Log(severity, module, lazyMessage(), extraInfo);
		}
	}

	public static void Log(Severity severity, string module, string message, Hashtable extraInfo = null)
	{
		if (CanLog(severity))
		{
			_Log(severity, module, message, extraInfo);
		}
	}

	private static void _Log(Severity severity, string module, string message, Hashtable extraInfo = null)
	{
		string text = string.Format("[{0}][{1}][{2}] {3}", DateTime.UtcNow.ToString("o"), severity, module, message);
		if (extraInfo != null && extraInfo.Count > 0)
		{
			string text2 = Json.Serialize(extraInfo);
			text = text + " " + text2;
		}
		switch (severity)
		{
		case Severity.Error:
		case Severity.Fatal:
			if (EnableTestMode)
			{
				UnityEngine.Debug.LogWarning(text);
			}
			else
			{
				UnityEngine.Debug.LogError(text);
			}
			break;
		case Severity.Notice:
		case Severity.Warning:
			UnityEngine.Debug.LogWarning(text);
			break;
		default:
			UnityEngine.Debug.Log(text);
			break;
		}
	}

	public static void LogException(Exception ex, string moduleName, Severity severity = Severity.Error)
	{
		string value = ((ex.InnerException != null) ? ex.InnerException.GetType().ToString() : null);
		string value2 = ((ex.InnerException != null) ? ex.InnerException.Message : null);
		Log(severity, moduleName, "Inner Exception", new Hashtable
		{
			{
				"type",
				ex.GetType().ToString()
			},
			{ "message", ex.Message },
			{ "inner_type", value },
			{ "inner_message", value2 },
			{ "stack", ex.StackTrace }
		});
	}
}
