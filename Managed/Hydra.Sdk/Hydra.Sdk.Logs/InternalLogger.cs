using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Logs;

internal class InternalLogger : IHydraSdkLogger
{
	public delegate void LogMessageDelegate(HydraLogType type, string category, string description, params object[] args);

	public delegate void LogContextDelegate(string category, string name, string value);

	public LogMessageDelegate OnLog;

	public LogContextDelegate OnLogContext;

	public void Log(HydraLogType type, string category, string description, params object[] args)
	{
		OnLog?.Invoke(type, category, description, args);
	}

	public void LogContext(string category, string name, string value)
	{
		OnLogContext?.Invoke(category, name, value);
	}
}
