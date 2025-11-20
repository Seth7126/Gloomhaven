public interface IDiagnosticsLogger
{
	void Log(string message);

	void LogWarning(string message);

	void LogError(string message);
}
