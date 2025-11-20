using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameDLCStep : UICreateGameStep
{
	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	[SerializeField]
	private Hotkey _continueHotkey;

	[Header("DLC")]
	[SerializeField]
	private UIDLCSelector m_DLCSelector;

	private bool _isLoadGameDlcState;

	private IGameSaveData _saveData;

	private Action<IGameSaveData, DLCRegistry.EDLCKey> _onDLCEnabled;

	private INavigationOperation _navigationOperation;

	private UIDLCSelectorOption _currentSelectedOption;

	private bool _longConfirmEnabled = true;

	private IHotkeySession _hotkeySession;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _unselectHotkey;

	private SessionHotkey _promoteHotkey;

	private DLCRegistry.EDLCKey _initiallyEnabledDlcs;

	private DLCRegistry.EDLCKey _enabledDlcs;

	private bool IsLoadGameDlcState
	{
		get
		{
			return _isLoadGameDlcState;
		}
		set
		{
			_isLoadGameDlcState = value;
			DlcSelector.IsActivateInLoadState = value;
		}
	}

	public UIDLCSelector DlcSelector => m_DLCSelector;

	protected override void Awake()
	{
		base.Awake();
		_navigationOperation = new AnonymousOperation(OnHideNavigationOperation);
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			DeinitializeHotKeys();
			UnsubscribeOnGamepadEvents();
		}
	}

	public void ShowDLCForLoadGame(IGameSaveData saveData, Action<IGameSaveData, DLCRegistry.EDLCKey> onDLCEnabled)
	{
		IsLoadGameDlcState = true;
		_saveData = saveData;
		_onDLCEnabled = onDLCEnabled;
		PartyGameSaveData partyGameSaveData = saveData as PartyGameSaveData;
		_initiallyEnabledDlcs = partyGameSaveData.PartyAdventureData.DLCEnabled;
		DlcSelector.SetValue(partyGameSaveData.PartyAdventureData.DLCEnabled);
		DlcSelector.SetInteractable(partyGameSaveData.PartyAdventureData.DLCEnabled);
		Show();
	}

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		DlcSelector.SetInteractable(DLCRegistry.EDLCKey.None);
		DlcSelector.SetValue(DLCRegistry.EDLCKey.None);
		_initiallyEnabledDlcs = DLCRegistry.EDLCKey.None;
		_enabledDlcs = DLCRegistry.EDLCKey.None;
		base.Setup(service, gameData, onConfirmed, onCancelled);
		EnableConfirmationButton(enable: true);
	}

	protected override void OnConfirmedStep()
	{
		if (Singleton<UIConfirmationBoxManager>.Instance.IsOpen)
		{
			return;
		}
		if (IsLoadGameDlcState)
		{
			if (DlcSelector.IsAnyDlcSelected)
			{
				UnityAction onEnableDLCConfirmed = delegate
				{
					_onDLCEnabled(_saveData, DlcSelector.GetValue());
					IsLoadGameDlcState = false;
					Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.LoadGame);
					Hide();
				};
				StartCoroutine(SkipAFrameBeforeMessage(onEnableDLCConfirmed));
			}
		}
		else
		{
			if (m_Data != null)
			{
				m_Data.DLCEnabled = DlcSelector.GetValue();
			}
			base.OnConfirmedStep();
		}
	}

	protected override void Show(bool instant = false)
	{
		InitializeHotKeys();
		SubscribeGamepadEvents();
		base.Show(instant);
	}

	public override void Hide(bool instant = false)
	{
		UnsubscribeOnGamepadEvents();
		DeinitializeHotKeys();
		base.Hide(instant);
	}

	protected override void OnControllerUnfocused()
	{
		base.OnControllerUnfocused();
		UnsubscribeFromCurrentOption();
	}

	private IEnumerator SkipAFrameBeforeMessage(UnityAction onEnableDLCConfirmed)
	{
		yield return new WaitForEndOfFrame();
		UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
		string translation = LocalizationManager.GetTranslation("GUI_ENABLE_DLC");
		string translation2 = LocalizationManager.GetTranslation("GUI_ENABLE_DLC_CONFIRMATION");
		INavigationOperation navigationOperation = _navigationOperation;
		instance.ShowGenericConfirmation(translation, translation2, onEnableDLCConfirmed, null, "GUI_ENABLE", null, null, showHeader: true, enableSoftlockReport: false, navigationOperation);
	}

	private void OnHideNavigationOperation(NavigationStateMachine navigationStateMachine)
	{
		IsLoadGameDlcState = true;
		navigationStateMachine.Enter(MainStateTag.SelectDLC);
	}

	private void SubscribeGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_DLC_PROMOTION, OnPromoteDLCButtonClicked));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
			UIDLCSelectorOption.UIDLCOptionSelected += HandleUidlcSelectorOptionSelected;
		}
	}

	private void UnsubscribeOnGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			if (Singleton<KeyActionHandlerController>.IsInitialized)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked);
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_DLC_PROMOTION, OnPromoteDLCButtonClicked);
			}
			UIDLCSelectorOption.UIDLCOptionSelected -= HandleUidlcSelectorOptionSelected;
			UnsubscribeFromCurrentOption();
		}
	}

	private void InitializeHotKeys()
	{
		if (InputManager.GamePadInUse)
		{
			_hotkeySession = Hotkeys.Instance.GetSession();
			_hotkeySession.AddOrReplaceHotkeys("Back");
			_hotkeySession.GetHotkey(out _selectHotkey, "Select").GetHotkey(out _unselectHotkey, "Unselect").GetHotkey(out _promoteHotkey, "DLC Promotion");
			_continueHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			if (_longConfirmHandler != null)
			{
				InputManager.RegisterToOnPressed(KeyAction.CONFIRM_ACTION_BUTTON, OnSubmitPressed);
			}
		}
	}

	private void OnSubmitPressed()
	{
		if (_longConfirmEnabled)
		{
			_longConfirmHandler.Pressed(OnLongButtonConfirmed, ShortPressedCallback);
		}
		else
		{
			ShortPressedCallback();
		}
	}

	private void ShortPressedCallback()
	{
		if (_currentSelectedOption != null)
		{
			_currentSelectedOption.OnSubmitPressed();
		}
	}

	private void HandleUidlcSelectorOptionSelected(UIDLCSelectorOption option)
	{
		UnsubscribeFromCurrentOption();
		_currentSelectedOption = option;
		if (_currentSelectedOption.Interactable)
		{
			_currentSelectedOption.OnDLCStateChanged += UpdateHotkeyForCurrentDLCOption;
			_currentSelectedOption.OnToggledDLC.AddListener(OnDLCOptionToggled);
		}
		UpdateHotkeyForCurrentDLCOption();
	}

	private void UnsubscribeFromCurrentOption()
	{
		if (_currentSelectedOption != null)
		{
			_currentSelectedOption.OnDLCStateChanged -= UpdateHotkeyForCurrentDLCOption;
			_currentSelectedOption.OnToggledDLC.RemoveListener(OnDLCOptionToggled);
		}
	}

	private void OnDLCOptionToggled(DLCRegistry.EDLCKey dlc, bool toggled)
	{
		if (toggled)
		{
			_enabledDlcs |= dlc;
		}
		else
		{
			_enabledDlcs &= ~dlc;
		}
		UpdateHotkeyForCurrentDLCOption();
	}

	private void UpdateHotkeyForCurrentDLCOption()
	{
		if (_isLoadGameDlcState)
		{
			List<DLCRegistry.EDLCKey> source = DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey it) => it != DLCRegistry.EDLCKey.None && PlatformLayer.DLC.UserInstalledDLC(it)).ToList();
			DLCRegistry.EDLCKey newlyEnabledDlcs = _enabledDlcs ^ (_enabledDlcs & _initiallyEnabledDlcs);
			bool flag = source.Any((DLCRegistry.EDLCKey x) => newlyEnabledDlcs.HasFlag(x));
			_longConfirmHandler.gameObject.SetActive(flag);
			_longConfirmEnabled = flag;
		}
		else
		{
			_longConfirmHandler.gameObject.SetActive(value: true);
		}
		if (_currentSelectedOption.DLCAvailable)
		{
			if (!_currentSelectedOption.Interactable)
			{
				HideDLCOptionHotkeys();
			}
			else
			{
				SetHotkeyForDLCOption(_currentSelectedOption.ToggledOn ? _unselectHotkey : _selectHotkey);
			}
		}
		else
		{
			SetHotkeyForDLCOption(_promoteHotkey);
		}
	}

	private void SetHotkeyForDLCOption(SessionHotkey hotkey)
	{
		HideDLCOptionHotkeys();
		hotkey.Show();
	}

	private void HideDLCOptionHotkeys()
	{
		_selectHotkey.Hide();
		_unselectHotkey.Hide();
		_promoteHotkey.Hide();
	}

	private void DeinitializeHotKeys()
	{
		if (InputManager.GamePadInUse)
		{
			if (_hotkeySession != null)
			{
				_selectHotkey.Dispose();
				_unselectHotkey.Dispose();
				_promoteHotkey.Dispose();
				_hotkeySession.Dispose();
				_hotkeySession = null;
			}
			_continueHotkey.Deinitialize();
			if (InputManager.GamePadInUse && _longConfirmHandler != null)
			{
				InputManager.UnregisterToOnPressed(KeyAction.CONFIRM_ACTION_BUTTON, OnSubmitPressed);
			}
		}
	}

	public void EnableLongConfirmInput()
	{
		_longConfirmEnabled = true;
	}

	public void DisableLongConfirmInput()
	{
		_longConfirmEnabled = false;
	}

	private void OnLongButtonConfirmed()
	{
		if (_longConfirmEnabled)
		{
			Confirm();
		}
	}

	private void OnPromoteDLCButtonClicked()
	{
		if (!(_currentSelectedOption == null) && !_currentSelectedOption.DLCAvailable)
		{
			PlatformLayer.DLC.OpenPlatformStoreDLCOverlay(_currentSelectedOption.DLC);
		}
	}

	private void OnCancelButtonClicked()
	{
		IsLoadGameDlcState = false;
		Cancel();
	}
}
