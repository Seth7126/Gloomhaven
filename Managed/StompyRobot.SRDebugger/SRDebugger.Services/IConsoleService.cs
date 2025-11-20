using System.Collections.Generic;

namespace SRDebugger.Services;

public interface IConsoleService
{
	int ErrorCount { get; }

	int WarningCount { get; }

	int InfoCount { get; }

	IReadOnlyList<ConsoleEntry> Entries { get; }

	IReadOnlyList<ConsoleEntry> AllEntries { get; }

	bool LoggingEnabled { get; set; }

	bool LogHandlerIsOverriden { get; }

	event ConsoleUpdatedEventHandler Updated;

	event ConsoleUpdatedEventHandler Error;

	void Clear();
}
