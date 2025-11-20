using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI;

public class LocalHotkeys : HotkeyContainer
{
	[SerializeField]
	private InputDisplayData _inputDisplayData;

	[SerializeField]
	private HotkeyOrderConfig _hotkeyOrderConfig;

	private void Awake()
	{
		Initialize(_inputDisplayData, Singleton<UINavigation>.Instance.Input, _hotkeyOrderConfig);
	}
}
