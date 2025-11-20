using System;
using GLOOM;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIEnhancementConfirmationBox : Singleton<UIEnhancementConfirmationBox>
{
	[SerializeField]
	private Image enhancementIcon;

	[SerializeField]
	private TMP_Text enhancementName;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private ExtendedButton cancelButton;

	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text informationText;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private UIWindow _confirmationBox;

	private Action _onConfirmCallback;

	private Action _onCancelCallback;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	protected override void Awake()
	{
		base.Awake();
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
		_confirmationBox = GetComponent<UIWindow>();
		UIWindow confirmationBox = _confirmationBox;
		confirmationBox.OnHide = (Action)Delegate.Combine(confirmationBox.OnHide, new Action(ToPreviousState));
		if (InputManager.GamePadInUse)
		{
			InitGamepadInput();
		}
		ResetConfirmationBox();
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse)
		{
			DisableGamepadInput();
		}
		UIWindow confirmationBox = _confirmationBox;
		confirmationBox.OnHide = (Action)Delegate.Remove(confirmationBox.OnHide, new Action(ToPreviousState));
		if (!InputManager.GamePadInUse)
		{
			confirmButton.onClick.RemoveAllListeners();
		}
		cancelButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	public void ShowConfirmation(string title, string information, EEnhancement enhancement, Action onActionConfirmed, string confirmActionKey = null, string cancelActionKey = null, Action onCancelled = null)
	{
		ShowConfirmation(title, information, UIInfoTools.Instance.GetEnhancementIcon(enhancement), LocalizationManager.GetTranslation($"ENHANCEMENT_{enhancement}"), onActionConfirmed, confirmActionKey, cancelActionKey, onCancelled);
	}

	public void ShowConfirmation(string title, string information, Sprite elementIcon, string elementName, Action onActionConfirmed, string confirmActionKey = null, string cancelActionKey = null, Action onCancelled = null)
	{
		_skipFrameKeyActionHandlerBlocker.Run();
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.EnhancmentConfirmation);
		_ = Time.frameCount;
		ResetConfirmationBox();
		enhancementIcon.sprite = elementIcon;
		enhancementName.text = elementName;
		titleText.text = title;
		informationText.text = information;
		_onConfirmCallback = onActionConfirmed;
		if (confirmActionKey != null)
		{
			confirmButton.TextLanguageKey = confirmActionKey;
		}
		if (cancelActionKey != null)
		{
			cancelButton.TextLanguageKey = cancelActionKey;
		}
		_confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				controllerArea.Destroy();
				onCancelled?.Invoke();
			}
		});
		if (!InputManager.GamePadInUse)
		{
			confirmButton.onClick.AddListener(OnConfirm);
		}
		cancelButton.onClick.AddListener(Hide);
		_confirmationBox.Show();
		controllerArea.Enable();
	}

	public void Hide()
	{
		_confirmationBox.Hide();
	}

	private void ToPreviousState()
	{
		Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
	}

	private void ResetConfirmationBox()
	{
		confirmButton.TextLanguageKey = "YES";
		cancelButton.TextLanguageKey = "GUI_CANCEL";
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		_confirmationBox.onHidden.RemoveAllListeners();
		_confirmationBox.onTransitionComplete.RemoveAllListeners();
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnConfirm).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancel).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
	}

	private void DisableGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnConfirm);
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancel);
		}
	}

	private void OnConfirm()
	{
		_confirmationBox.onTransitionComplete.RemoveAllListeners();
		_confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				controllerArea.Destroy();
				_onConfirmCallback?.Invoke();
			}
		});
		Hide();
	}

	private void OnCancel()
	{
		if (_onCancelCallback != null)
		{
			_confirmationBox.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
			{
				if (state == UIWindow.VisualState.Hidden)
				{
					controllerArea.Destroy();
					_onCancelCallback();
				}
			});
		}
		Hide();
	}
}
