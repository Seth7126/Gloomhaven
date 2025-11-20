using System;
using Script.GUI.Popups;
using UnityEngine.EventSystems;

public class LongConfirmHandler : LongPressHandlerBase
{
	private bool _canTriggerSubmitEventOnShort;

	private bool _canTriggerSubmitPlayerActionOnShort;

	private KeyAction _shortButtonKeyAction = KeyAction.UI_SUBMIT;

	protected override KeyAction KeyActionToWait => KeyAction.CONFIRM_ACTION_BUTTON;

	public bool SendSendSubmitEventOnShort { get; private set; }

	public bool SendSubmitPlayerActionOnShort { get; private set; }

	public void SetSendSubmitEventOnShort(bool enable)
	{
		SendSendSubmitEventOnShort = enable;
		_canTriggerSubmitEventOnShort = false;
	}

	public void SetSendSubmitPlayerActionOnShort(bool enable)
	{
		SendSubmitPlayerActionOnShort = enable;
		_canTriggerSubmitPlayerActionOnShort = false;
		_shortButtonKeyAction = KeyAction.UI_SUBMIT;
	}

	public void EnableSendSubmitPlayerActionOnShort(KeyAction shortButtonKeyAction)
	{
		_shortButtonKeyAction = shortButtonKeyAction;
		SendSubmitPlayerActionOnShort = true;
		_canTriggerSubmitPlayerActionOnShort = false;
	}

	public override void Pressed(Action longPressedCallback, Action shortPressedCallback = null)
	{
		_canTriggerSubmitEventOnShort = SendSendSubmitEventOnShort;
		_canTriggerSubmitPlayerActionOnShort = SendSubmitPlayerActionOnShort;
		base.Pressed(longPressedCallback, shortPressedCallback);
	}

	protected override void ShortPressed()
	{
		base.ShortPressed();
		if (_canTriggerSubmitEventOnShort)
		{
			EventSystem current = EventSystem.current;
			ExecuteEvents.Execute(current.currentSelectedGameObject, new BaseEventData(current), ExecuteEvents.submitHandler);
		}
		if (_canTriggerSubmitPlayerActionOnShort)
		{
			((GHControls.GHAction)Singleton<InputManager>.Instance.PlayerControl.GetPlayerActionForKeyAction(_shortButtonKeyAction)).SimulateOnPress();
		}
		_canTriggerSubmitPlayerActionOnShort = false;
		_canTriggerSubmitEventOnShort = false;
	}
}
