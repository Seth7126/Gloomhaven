using System;
using MapRuleLibrary.Party;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIAdventureCharacterConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private UICharacterInformation information;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private Hotkey confirmHotkey;

	[SerializeField]
	private Hotkey cancelHotkey;

	private Action confirmAction;

	private ControllerInputAreaLocal controllerArea;

	private UIWindow window;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		confirmButton.onClick.AddListener(OnConfirmClick);
		cancelButton.onClick.AddListener(OnBackClick);
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		controllerArea.OnFocusedArea.AddListener(OnFocused);
		ControllerAreaLocalFocusKeyActionHandlerBlocker keyActionHandlerBlocker = new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnConfirmClick, ShowConfirmHotkey, HideConfirmHotkey).AddBlocker(keyActionHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnBackClick, ShowCancelHotkey, HideCancelHotkey).AddBlocker(keyActionHandlerBlocker));
	}

	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnConfirmClick);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnBackClick);
		}
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		if (InputManager.GamePadInUse)
		{
			cancelHotkey.Deinitialize();
			confirmHotkey.Deinitialize();
		}
	}

	private void OnTransitionComplete(UIWindow window, UIWindow.VisualState state)
	{
		if (state == UIWindow.VisualState.Hidden)
		{
			window.onTransitionComplete.RemoveListener(OnTransitionComplete);
			confirmAction?.Invoke();
		}
	}

	private void OnConfirmClick()
	{
		window.onTransitionComplete.AddListener(OnTransitionComplete);
		window.Hide();
	}

	private void OnBackClick()
	{
		window.Hide();
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.CharacterConfirmationBox);
	}

	public virtual void ShowConfirmationBox(CMapCharacter character, Action onActionConfirmed)
	{
		confirmAction = onActionConfirmed;
		information.Display(character);
		window.Show();
	}

	public void Hide()
	{
		window.Hide();
	}

	private void ShowConfirmHotkey()
	{
		if (InputManager.GamePadInUse)
		{
			confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void HideConfirmHotkey()
	{
		if (InputManager.GamePadInUse)
		{
			confirmHotkey.Deinitialize();
			confirmHotkey.DisplayHotkey(active: false);
		}
	}

	private void ShowCancelHotkey()
	{
		if (InputManager.GamePadInUse)
		{
			cancelHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void HideCancelHotkey()
	{
		if (InputManager.GamePadInUse)
		{
			cancelHotkey.Deinitialize();
			cancelHotkey.DisplayHotkey(active: false);
		}
	}
}
