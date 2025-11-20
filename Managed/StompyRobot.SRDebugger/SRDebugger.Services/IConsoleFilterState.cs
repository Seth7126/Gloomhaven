using UnityEngine;

namespace SRDebugger.Services;

public interface IConsoleFilterState
{
	event ConsoleStateChangedEventHandler FilterStateChange;

	void SetConsoleFilterState(LogType logType, bool enabled);

	bool GetConsoleFilterState(LogType logType);
}
