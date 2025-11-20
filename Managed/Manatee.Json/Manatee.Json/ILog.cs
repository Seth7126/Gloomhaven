namespace Manatee.Json;

public interface ILog
{
	void Verbose(string message, LogCategory category = LogCategory.General);
}
