public abstract class DiagnosticsLoggerBase : IDiagnosticsLogger
{
	protected string Prefix { get; set; } = "[DIAGNOSTICS]:";

	public void Log(string message)
	{
		string message2 = ConstructLogMessage(message);
		LogInternal(message2);
	}

	public void LogWarning(string message)
	{
		string message2 = ConstructLogMessage(message);
		LogWarningInternal(message2);
	}

	public void LogError(string message)
	{
		string message2 = ConstructLogMessage(message);
		LogErrorInternal(message2);
	}

	protected abstract void LogInternal(string message);

	protected abstract void LogWarningInternal(string message);

	protected abstract void LogErrorInternal(string message);

	protected virtual string ConstructLogMessage(string message)
	{
		return Prefix + " " + message;
	}
}
