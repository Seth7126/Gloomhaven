using System;
using Code.State;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

public class SignOutConfirmationBox : Singleton<SignOutConfirmationBox>
{
	[SerializeField]
	private TextLocalizedListener _titleText;

	[SerializeField]
	private TextLocalizedListener _messageText;

	[SerializeField]
	private UIWindow _uiWindow;

	[SerializeField]
	private LocalHotkeys _hotkeyContainer;

	private readonly UiNavigationBlocker _navigationBlocker = new UiNavigationBlocker("SignOutConfirmationBox");

	private bool _blockedChoreographer;

	private Action _onComplete;

	private Action _onCancel;

	private IHotkeySession _hotkeySession;

	public UIWindow Window => _uiWindow;

	protected override void Awake()
	{
		base.Awake();
		InitGamepadInput();
	}

	protected override void OnDestroy()
	{
		DisableGamepadInput();
		base.OnDestroy();
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnSubmit, null, null, isPersistent: true).AddBlocker(new UIWindowOpenKeyActionBlocker(Window)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancel, null, null, isPersistent: true).AddBlocker(new UIWindowOpenKeyActionBlocker(Window)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnSubmit);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancel);
		}
	}

	public void Activate(string titleKey, string messageKey, Action onComplete = null, Action onCancel = null)
	{
		if (_uiWindow.IsOpen)
		{
			return;
		}
		_onComplete = onComplete;
		_onCancel = onCancel;
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyAction.UI_SUBMIT, KeyAction.UI_CANCEL);
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navigationBlocker);
		_titleText.SetTextKey(titleKey);
		_messageText.SetTextKey(messageKey);
		if (InputManager.GamePadInUse)
		{
			_hotkeySession = _hotkeyContainer.GetSession();
			_hotkeySession.AddOrReplaceHotkey("ErrorOk", null);
			if (_onCancel != null)
			{
				_hotkeySession.AddOrReplaceHotkey("ErrorCancel", null);
			}
		}
		Window.Show();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.SignOutConfirmationBox);
	}

	private void OnSubmit()
	{
		Deactivate(_onComplete);
	}

	private void OnCancel()
	{
		Deactivate(_onCancel);
	}

	private void Deactivate(Action onResult)
	{
		if (_blockedChoreographer)
		{
			if (Choreographer.s_Choreographer != null)
			{
				Choreographer.s_Choreographer.RemoveUpdateBlocker();
			}
			_blockedChoreographer = false;
		}
		onResult?.Invoke();
		_hotkeySession?.Dispose();
		Window.Hide();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
		InputManager.SkipNextSubmitAction();
		IStateFilter stateFilter = new StateFilterByType(typeof(GamepadDisconnectionBoxState)).InverseFilter();
		Singleton<UINavigation>.Instance.StateMachine.ToPreviousState(stateFilter);
	}
}
