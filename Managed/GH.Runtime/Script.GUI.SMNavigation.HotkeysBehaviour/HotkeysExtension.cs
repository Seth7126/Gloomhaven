using System.Collections.Generic;
using SM.Gamepad;
using Script.GUI.SMNavigation.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.HotkeysBehaviour;

public static class HotkeysExtension
{
	public static void TrySetHotkeyEvent(this GameObject target, KeyAction keyAction)
	{
		target.GetComponent<Hotkey>().TrySetHotkeyEvent(keyAction);
	}

	public static void TrySetHotkeyEvent(this Hotkey hotkey, KeyAction keyAction)
	{
		if (!(hotkey == null))
		{
			hotkey.ExpectedEvent = keyAction.ConvertToExceptedEvent();
			hotkey.TrySetEnabledHotkey(enable: true);
		}
	}

	public static void TryEnableHotkeys(this IEnumerable<Hotkey> hotkeys)
	{
		foreach (Hotkey hotkey in hotkeys)
		{
			hotkey.TryEnableHotkey();
		}
	}

	public static void RebuildHotkeysLayout(this IEnumerable<Hotkey> hotkeys)
	{
		foreach (Hotkey hotkey in hotkeys)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(hotkey.transform as RectTransform);
		}
	}

	public static void DisableHotkeys(this IEnumerable<Hotkey> hotkeys)
	{
		foreach (Hotkey hotkey in hotkeys)
		{
			hotkey.DisableHotkey();
		}
	}

	public static void TryEnableHotkey(this Hotkey hotkey)
	{
		hotkey.TrySetEnabledHotkey(enable: true);
	}

	public static void DisableHotkey(this Hotkey hotkey)
	{
		hotkey.SetEnabledHotkey(enable: false);
	}

	public static void TrySetEnabledHotkey(this Hotkey hotkey, bool enable)
	{
		hotkey.SetEnabledHotkey(InputManager.GamePadInUse && enable);
	}

	public static void SetEnabledHotkey(this Hotkey hotkey, bool enable)
	{
		if (!(hotkey == null))
		{
			if (enable)
			{
				hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
				return;
			}
			hotkey.DisplayHotkey(active: false);
			hotkey.Deinitialize();
		}
	}
}
