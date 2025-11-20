using SM.Gamepad;
using Script.GUI;
using UnityEngine;

namespace GLOOM.MainMenu;

public class UIMultiplayerHowToHostWindow : UISubmenuGOWindow
{
	[SerializeField]
	private LocalHotkeys _hotkeys;

	[SerializeField]
	private ControllerInputScroll _controllerInputScroll;

	private IHotkeySession _hotkeySession;

	protected void ShowHotkeys()
	{
		if (!(_hotkeys == null))
		{
			if (_hotkeySession == null)
			{
				_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back");
			}
			if (_controllerInputScroll != null)
			{
				_controllerInputScroll.enabled = true;
			}
		}
	}

	protected void HideHotkeys()
	{
		if (_hotkeySession != null)
		{
			_hotkeySession.Dispose();
			_hotkeySession = null;
		}
		if (_controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = false;
		}
	}

	protected override void OnControllerFocused()
	{
		base.OnControllerFocused();
		ShowHotkeys();
	}

	protected override void OnControllerUnfocused()
	{
		base.OnControllerUnfocused();
		HideHotkeys();
	}
}
