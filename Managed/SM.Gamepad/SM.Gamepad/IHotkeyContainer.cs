using System;
using System.Collections.Generic;

namespace SM.Gamepad;

public interface IHotkeyContainer
{
	void Initialize(InputDisplayData displayData, IHotkeyActionInput hotkeyActionInput, HotkeyOrderConfig hotkeyOrderConfig = null);

	IHotkeySession GetSession(IHotkeySession session);

	IHotkeySession GetSession();

	internal string RequestSessionId();

	void AddOrReplaceHotkeysForObject(string requesterObjectId, Dictionary<string, Action> hotkeysData);

	void RemoveHotkeysForObject(string requesterObjectId);
}
