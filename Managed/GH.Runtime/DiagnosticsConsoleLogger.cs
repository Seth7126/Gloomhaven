#define ENABLE_LOGS
using SM.Utils;

public sealed class DiagnosticsConsoleLogger : DiagnosticsLoggerBase
{
	protected override void LogInternal(string message)
	{
		LogUtils.Log(message);
	}

	protected override void LogWarningInternal(string message)
	{
		LogUtils.LogWarning(message);
	}

	protected override void LogErrorInternal(string message)
	{
		LogUtils.LogError(message);
	}
}
