using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

public class HotkeysActivationHandler : MonoBehaviour
{
	[SerializeField]
	private Hotkey[] _hotkeys;

	[UsedImplicitly]
	private void OnDestroy()
	{
		Hotkey[] hotkeys = _hotkeys;
		for (int i = 0; i < hotkeys.Length; i++)
		{
			hotkeys[i].Deinitialize();
		}
	}

	public void SetHotkeysActive(bool value)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (value)
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj in hotkeys)
			{
				obj.Initialize(Singleton<UINavigation>.Instance.Input);
				obj.DisplayHotkey(active: true);
			}
		}
		else
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj2 in hotkeys)
			{
				obj2.Deinitialize();
				obj2.DisplayHotkey(active: false);
			}
		}
	}
}
