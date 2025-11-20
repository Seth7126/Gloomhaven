using System.Collections.Generic;

namespace SRDebugger.Services;

public interface IPinEntryService
{
	bool IsShowingKeypad { get; }

	void ShowPinEntry(IReadOnlyList<int> requiredPin, string message, PinEntryCompleteCallback callback, bool allowCancel = true);
}
