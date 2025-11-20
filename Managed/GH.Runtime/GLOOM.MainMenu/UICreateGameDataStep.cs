using System;
using AsmodeeNet.Foundation;
using SM.Gamepad;
using Script.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameDataStep : UICreateGameStep
{
	[SerializeField]
	protected UIKeyboard _keyboard;

	[SerializeField]
	private PartyNameInputFieldController _partyNameInputFieldController;

	[SerializeField]
	private UIDifficultySelector m_DifficultySelector;

	[Header("Confirm")]
	[SerializeField]
	private GUIAnimator m_IncompletedDataWarningAnimator;

	[SerializeField]
	private GameObject m_HighlightConfirmButton;

	[SerializeField]
	private GUIAnimator m_EnableConfirmAnimator;

	[SerializeField]
	private string m_EnableConfirmAudioItem;

	[SerializeField]
	protected ConfirmNameBlock _confirmNameBlock;

	[SerializeField]
	private LocalHotkeys _hotkeys;

	private readonly SimpleKeyActionHandlerBlocker _validNameBlocker = new SimpleKeyActionHandlerBlocker();

	private readonly SimpleKeyActionHandlerBlocker _hiddenViewBlocker = new SimpleKeyActionHandlerBlocker();

	private bool m_IsConfirmEnabled;

	private IHotkeySession _hotkeySession;

	protected override void Awake()
	{
		base.Awake();
		_validNameBlocker.SetBlock(value: true);
		_partyNameInputFieldController.NameValidated += OnNameValidated;
		m_Window.onHidden.AddListener(OnHidden);
	}

	private new void OnDestroy()
	{
		_partyNameInputFieldController.PartyNameInputField.onValueChanged.RemoveAllListeners();
	}

	private void OnHidden()
	{
		if (!InputManager.GamePadInUse)
		{
			m_IncompletedDataWarningAnimator?.Stop();
			m_EnableConfirmAnimator?.Stop();
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			OnControllerUnfocused();
			OnHidden();
		}
	}

	protected override void Show(bool instant = false)
	{
		if (!InputManager.GamePadInUse)
		{
			m_IncompletedDataWarningAnimator.Stop();
			m_EnableConfirmAnimator.Stop();
		}
		base.Show(instant);
		_partyNameInputFieldController.FocusOnInputFieldIfNeed();
		ChangeHotkeySelect();
	}

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		base.Setup(service, gameData, onConfirmed, onCancelled);
		_partyNameInputFieldController.InitializeGameModeService(service);
		m_DifficultySelector.ToggleDifficulty(gameData.Difficulty);
		_partyNameInputFieldController.SetInputFieldText(gameData.GameName);
		_partyNameInputFieldController.HideHelpBox();
		_partyNameInputFieldController.ValidateName(gameData.GameName);
	}

	protected override void Validate(Action<bool> callback)
	{
		callback = (Action<bool>)Delegate.Combine(callback, (Action<bool>)delegate(bool isValid)
		{
			if (!isValid && m_IncompletedDataWarningAnimator != null)
			{
				m_IncompletedDataWarningAnimator.Play();
			}
		});
		_partyNameInputFieldController.IsValidName(callback);
	}

	protected override void OnConfirmedStep()
	{
		BuildGameModel(m_Data);
		base.OnConfirmedStep();
	}

	protected virtual GameData BuildGameModel(GameData data)
	{
		data.GameName = _partyNameInputFieldController.GetInputFieldText();
		data.Difficulty = m_DifficultySelector.SelectedDifficulty;
		return data;
	}

	protected override void OnUIWindowShown()
	{
		base.OnUIWindowShown();
		Initialize();
		_hiddenViewBlocker.SetBlock(value: false);
	}

	protected override void OnUIWindowHidden()
	{
		Deinitialize();
		_hiddenViewBlocker.SetBlock(value: true);
		base.OnUIWindowHidden();
		if (!InputManager.GamePadInUse)
		{
			m_IncompletedDataWarningAnimator.Stop();
			m_EnableConfirmAnimator.Stop();
		}
	}

	private void Initialize()
	{
		_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back");
		SubscribeOnEventsForGamepad();
		InitializeConfirmBlock();
		_partyNameInputFieldController.IsValidName(delegate(bool isValid)
		{
			if (isValid)
			{
				EnableConfirmationButton(enable: true);
			}
			if (InputManager.GamePadInUse && !_keyboard.IsActive && isValid)
			{
				_confirmNameBlock.ActivateConfirmButton();
			}
		});
	}

	private void Deinitialize()
	{
		UsubscribeOnEventsForGamepad();
		DeinitializeConfirmBlock();
		_hotkeySession?.Dispose();
		_hotkeySession = null;
	}

	private void DeinitializeConfirmBlock()
	{
		if (_confirmNameBlock != null)
		{
			_confirmNameBlock.Deinitialize();
		}
	}

	private void UsubscribeOnEventsForGamepad()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CONFIRM_ACTION_BUTTON, OnConfirmButtonPressed);
			_partyNameInputFieldController.NameInputFieldSelected -= OnNameInputFieldSelected;
			_partyNameInputFieldController.NameInputFieldDeselected -= OnNameInputFieldDeselected;
			UIKeyboard keyboard = _keyboard;
			keyboard.OnHide = (Action)Delegate.Remove(keyboard.OnHide, new Action(OnKeyboardHidden));
			UIKeyboard keyboard2 = _keyboard;
			keyboard2.OnShow = (Action)Delegate.Remove(keyboard2.OnShow, new Action(OnKeyboardShown));
		}
	}

	protected override void EnableConfirmationButton(bool enable)
	{
		if (!InputManager.GamePadInUse)
		{
			if (!enable)
			{
				m_EnableConfirmAnimator.Stop();
			}
			else if (!m_IsConfirmEnabled)
			{
				AudioControllerUtils.PlaySound(m_EnableConfirmAudioItem);
				m_EnableConfirmAnimator.Play();
			}
			m_IsConfirmEnabled = enable;
		}
		_validNameBlocker.SetBlock(value: false);
		base.EnableConfirmationButton(enable);
	}

	private void SubscribeOnEventsForGamepad()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CONFIRM_ACTION_BUTTON, OnConfirmButtonPressed).AddBlocker(_validNameBlocker).AddBlocker(_hiddenViewBlocker).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
			_partyNameInputFieldController.NameInputFieldSelected += OnNameInputFieldSelected;
			_partyNameInputFieldController.NameInputFieldDeselected += OnNameInputFieldDeselected;
			UIKeyboard keyboard = _keyboard;
			keyboard.OnHide = (Action)Delegate.Combine(keyboard.OnHide, new Action(OnKeyboardHidden));
			UIKeyboard keyboard2 = _keyboard;
			keyboard2.OnShow = (Action)Delegate.Combine(keyboard2.OnShow, new Action(OnKeyboardShown));
		}
	}

	private void OnConfirmButtonPressed()
	{
		if (!(_confirmNameBlock.LongConfirmButton == null))
		{
			KeyAction shortButtonKeyAction = KeyAction.UI_SUBMIT;
			if (_partyNameInputFieldController.PartyNameInputField.text.IsNOTNullOrEmpty() && !_keyboard.IsActive)
			{
				shortButtonKeyAction = KeyAction.UI_RENAME;
			}
			_confirmNameBlock.LongConfirmButton.SetSendSubmitEventOnShort(enable: true);
			_confirmNameBlock.LongConfirmButton.EnableSendSubmitPlayerActionOnShort(shortButtonKeyAction);
			_confirmNameBlock.LongConfirmButton.Pressed(OnLongPressConfirmed);
		}
	}

	private void InitializeConfirmBlock()
	{
		if (InputManager.GamePadInUse && _confirmNameBlock != null)
		{
			_confirmNameBlock.Initialize();
		}
	}

	private void OnLongPressConfirmed()
	{
		Confirm();
	}

	private void OnNameInputFieldSelected()
	{
		ChangeHotkeySelect();
	}

	private void OnNameInputFieldDeselected()
	{
		_partyNameInputFieldController.IsValidName(delegate(bool isValid)
		{
			if (!_keyboard.IsActive && isValid)
			{
				_confirmNameBlock.ActivateConfirmButton();
			}
			InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, ShowKeyboard);
			InputManager.UnregisterToOnPressed(KeyAction.UI_RENAME, ShowKeyboard);
		});
	}

	private void OnKeyboardShown()
	{
		_confirmNameBlock.DeactivateConfirmButton();
	}

	private void OnNameValidated(bool isValid)
	{
		EnableConfirmationButton(isValid);
	}

	private void OnKeyboardHidden()
	{
		_partyNameInputFieldController.IsValidName(delegate(bool isValid)
		{
			if (isValid)
			{
				EnableConfirmationButton(enable: true);
			}
			if (!_keyboard.IsActive && isValid)
			{
				_confirmNameBlock.ActivateConfirmButton();
			}
			ChangeHotkeySelect();
		});
	}

	protected override void OnControllerFocused()
	{
		base.OnControllerFocused();
		ChangeHotkeySelect();
	}

	protected override void OnControllerUnfocused()
	{
		base.OnControllerUnfocused();
		HideKeyboard();
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, ShowKeyboard);
		InputManager.UnregisterToOnPressed(KeyAction.UI_RENAME, ShowKeyboard);
	}

	private void ShowKeyboard()
	{
		if (_partyNameInputFieldController.InputFieldIsCurrentSelectable())
		{
			_keyboard.Show();
		}
	}

	private void HideKeyboard()
	{
		if (_keyboard.gameObject.activeSelf)
		{
			_keyboard.Hide();
		}
	}

	private void ChangeHotkeySelect()
	{
		if (InputManager.GamePadInUse)
		{
			if (_partyNameInputFieldController.PartyNameInputField.text.IsNOTNullOrEmpty())
			{
				InputManager.RegisterToOnPressed(KeyAction.UI_RENAME, ShowKeyboard);
				InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, ShowKeyboard);
			}
			else
			{
				InputManager.UnregisterToOnPressed(KeyAction.UI_RENAME, ShowKeyboard);
				InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, ShowKeyboard);
			}
		}
	}
}
