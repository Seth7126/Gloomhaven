using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation.HotkeysBehaviour;

public class HotkeyInitializer : MonoBehaviour
{
	[SerializeField]
	private Hotkey _hotkey;

	private void Awake()
	{
		if (_hotkey == null)
		{
			_hotkey = GetComponent<Hotkey>();
		}
		if (!(_hotkey == null) && Singleton<UINavigation>.Instance.Input.GamePadInUse)
		{
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_hotkey.DisplayHotkey(active: true);
			_hotkey.UpdateHotkeyIcon();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<UINavigation>.Instance != null && Singleton<UINavigation>.Instance.Input.GamePadInUse)
		{
			_hotkey.Deinitialize();
		}
	}
}
