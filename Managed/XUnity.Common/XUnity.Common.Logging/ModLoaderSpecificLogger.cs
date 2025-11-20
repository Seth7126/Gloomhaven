using System;
using System.Reflection;
using XUnity.Common.Utilities;

namespace XUnity.Common.Logging;

internal class ModLoaderSpecificLogger : XuaLogger
{
	public static class BepInExLogLevel
	{
		public const int None = 0;

		public const int Fatal = 1;

		public const int Error = 2;

		public const int Warning = 4;

		public const int Message = 8;

		public const int Info = 16;

		public const int Debug = 32;

		public const int All = 63;
	}

	private static Action<LogLevel, string> _logMethod;

	public ModLoaderSpecificLogger(string source)
		: base(source)
	{
		if (_logMethod != null)
		{
			return;
		}
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public;
		BindingFlags bindingAttr2 = BindingFlags.Instance | BindingFlags.Public;
		Type type = Type.GetType("BepInEx.Logging.LogLevel, BepInEx", throwOnError: false) ?? Type.GetType("BepInEx.Logging.LogLevel, BepInEx.Core", throwOnError: false);
		if (type != null)
		{
			if ((Type.GetType("BepInEx.Logging.ManualLogSource, BepInEx", throwOnError: false) ?? Type.GetType("BepInEx.Logging.ManualLogSource, BepInEx.Core", throwOnError: false)) != null)
			{
				MethodInfo method = (Type.GetType("BepInEx.Logging.Logger, BepInEx", throwOnError: false) ?? Type.GetType("BepInEx.Logging.Logger, BepInEx.Core", throwOnError: false)).GetMethod("CreateLogSource", bindingAttr, null, new Type[1] { typeof(string) }, null);
				object logInstance = method.Invoke(null, new object[1] { base.Source });
				MethodInfo method2 = logInstance.GetType().GetMethod("Log", bindingAttr2, null, new Type[2]
				{
					type,
					typeof(object)
				}, null);
				FastReflectionDelegate log = method2.CreateFastDelegate();
				_logMethod = delegate(LogLevel level, string msg)
				{
					int num = Convert(level);
					log(logInstance, num, msg);
				};
			}
			else
			{
				Type type2 = Type.GetType("BepInEx.Logger, BepInEx", throwOnError: false);
				object logInstance2 = type2.GetProperty("CurrentLogger", bindingAttr).GetValue(null, null);
				MethodInfo method3 = logInstance2.GetType().GetMethod("Log", bindingAttr2, null, new Type[2]
				{
					type,
					typeof(object)
				}, null);
				FastReflectionDelegate log2 = method3.CreateFastDelegate();
				_logMethod = delegate(LogLevel level, string msg)
				{
					int num = Convert(level);
					log2(logInstance2, num, msg);
				};
			}
		}
		else
		{
			Type type3 = Type.GetType("MelonLoader.MelonLogger, MelonLoader.ModHandler", throwOnError: false);
			if (type3 != null)
			{
				MethodInfo method4 = type3.GetMethod("Log", bindingAttr, null, new Type[2]
				{
					typeof(ConsoleColor),
					typeof(string)
				}, null);
				MethodInfo method5 = type3.GetMethod("Log", bindingAttr, null, new Type[1] { typeof(string) }, null);
				MethodInfo method6 = type3.GetMethod("LogWarning", bindingAttr, null, new Type[1] { typeof(string) }, null);
				MethodInfo method7 = type3.GetMethod("LogError", bindingAttr, null, new Type[1] { typeof(string) }, null);
				FastReflectionDelegate logDebug = method4.CreateFastDelegate();
				FastReflectionDelegate logInfo = method5.CreateFastDelegate();
				FastReflectionDelegate logWarning = method6.CreateFastDelegate();
				FastReflectionDelegate logError = method7.CreateFastDelegate();
				_logMethod = delegate(LogLevel level, string msg)
				{
					switch (level)
					{
					case LogLevel.Debug:
						logDebug(null, ConsoleColor.Gray, msg);
						break;
					case LogLevel.Info:
						logInfo(null, msg);
						break;
					case LogLevel.Warn:
						logWarning(null, msg);
						break;
					case LogLevel.Error:
						logError(null, msg);
						break;
					default:
						throw new ArgumentException("level");
					}
				};
			}
		}
		if (_logMethod != null)
		{
			return;
		}
		throw new Exception("Did not recognize any mod loader!");
	}

	protected override void Log(LogLevel level, string message)
	{
		_logMethod(level, message);
	}

	public static int Convert(LogLevel level)
	{
		return level switch
		{
			LogLevel.Debug => 32, 
			LogLevel.Info => 16, 
			LogLevel.Warn => 4, 
			LogLevel.Error => 2, 
			_ => 0, 
		};
	}
}
