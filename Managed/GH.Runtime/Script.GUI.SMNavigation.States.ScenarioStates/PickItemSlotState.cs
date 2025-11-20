using Code.State;
using SM.Gamepad;
using Script.GUI.GameScreen;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public abstract class PickItemSlotState : PopupState
{
	private IHotkeySession _hotkeySession;

	private ItemCardSelectHotkeys _selectHotkeys = new ItemCardSelectHotkeys();

	public PickItemSlotState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Enter(stateProvider, payload);
		Singleton<FastForwardButton>.Instance.Toggle(active: false);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		_hotkeySession = Hotkeys.Instance.GetSession();
		_selectHotkeys.Enter(_hotkeySession);
		ItemCardPickerSlot currentItem = ItemCardPickerSlot.CurrentItem;
		if (currentItem != null)
		{
			_selectHotkeys.SetSelectHotkeys(currentItem);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		_selectHotkeys.Exit();
		_hotkeySession.Dispose();
		_hotkeySession = null;
	}
}
