using System.Collections.Generic;

namespace Script.GUI.SMNavigation.Utils;

public static class KeyEventsExtension
{
	private static readonly Dictionary<KeyAction, string> _exceptedEventsMap = new Dictionary<KeyAction, string>
	{
		{
			KeyAction.CONFIRM_ACTION_BUTTON,
			"Select"
		},
		{
			KeyAction.UI_SUBMIT,
			"Select"
		},
		{
			KeyAction.UI_CANCEL,
			"Back"
		}
	};

	public static string ConvertToExceptedEvent(this KeyAction keyAction)
	{
		if (_exceptedEventsMap.TryGetValue(keyAction, out var value))
		{
			return value;
		}
		return "INVALID_KEY_ACTION";
	}
}
