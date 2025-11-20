using SRDebugger.Gamepad;
using SRF;
using SRF.Service;

namespace SRDebugger.Services.Implementation;

[Service(typeof(GamepadShortcutsListenerService))]
public class GamepadShortcutsListenerService : SRServiceBase<GamepadShortcutsListenerService>
{
	private IGamepadButton _buttonEast;

	protected override void Awake()
	{
		base.Awake();
		base.CachedTransform.SetParent(Hierarchy.Get("SRDebugger"));
	}

	protected override void Update()
	{
		base.Update();
		UpdateInputSystem();
	}

	private void UpdateInputSystem()
	{
		IGamepad gamepad = SRDebug.Instance.Gamepad;
		if (gamepad != null && gamepad.IsValid())
		{
			if (_buttonEast == null)
			{
				_buttonEast = gamepad.GetGamepadButton("buttonEast");
			}
			if (_buttonEast.IsWasPressed() && SRDebug.Instance.IsDebugPanelVisible)
			{
				SRDebug.Instance.HideDebugPanel();
			}
		}
	}
}
