namespace Apparance.Net;

public interface ILogging
{
	void LogMessage(string message);

	void LogWarning(string message);

	void LogError(string message);
}
