using System;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameHouseRulesStep : UICreateGameStep
{
	[Header("House Rules")]
	[SerializeField]
	private UIHouseRulesSelector _houseRulesSelector;

	[Header("DLC Keyboard/mouse")]
	[SerializeField]
	private UIDLCSelector _DLCSelector;

	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	[SerializeField]
	private Hotkey _longConfirmHotkey;

	[SerializeField]
	private LocalHotkeys _hotkeys;

	[SerializeField]
	private ControllerInputAreaLocal _inputArea;

	private readonly SimpleKeyActionHandlerBlocker _windowShownBlocker = new SimpleKeyActionHandlerBlocker();

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private IHotkeySession _hotkeySession;

	protected override void Awake()
	{
		base.Awake();
		_windowShownBlocker.SetBlock(value: true);
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this);
		if (_inputArea != null)
		{
			_inputArea.OnFocusedArea.AddListener(OnFocus);
		}
		SubscribeGamepadEvents();
		if (InputManager.GamePadInUse)
		{
			TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
		}
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			if (Singleton<KeyActionHandlerController>.Instance != null)
			{
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked);
				Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CONFIRM_ACTION_BUTTON, OnConfirmButtonPressed);
			}
			TooltipsVisibilityHelper.Instance.RemoveTooltipRequest(this);
			DeinitializeHotKeys();
			base.OnDestroy();
		}
	}

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		if (_houseRulesSelector != null)
		{
			_houseRulesSelector.SetValue(gameData.HouseRules);
		}
		if ((bool)_DLCSelector)
		{
			_DLCSelector.SetValue(DLCRegistry.EDLCKey.None);
		}
		base.Setup(service, gameData, onConfirmed, onCancelled);
		EnableConfirmationButton(enable: true);
	}

	protected override void OnConfirmedStep()
	{
		m_Data.HouseRules = _houseRulesSelector.GetValue();
		if ((bool)_DLCSelector)
		{
			m_Data.DLCEnabled = _DLCSelector.GetValue();
		}
		base.OnConfirmedStep();
	}

	protected override void OnUIWindowHidden()
	{
		base.OnUIWindowHidden();
		_windowShownBlocker.SetBlock(value: true);
		DeinitializeHotKeys();
	}

	protected override void OnUIWindowShown()
	{
		base.OnUIWindowShown();
		_windowShownBlocker.SetBlock(value: false);
		InitializeHotKeys();
	}

	private void OnFocus()
	{
		_skipFrameKeyActionHandlerBlocker.Run();
	}

	private void InitializeHotKeys()
	{
		if (InputManager.GamePadInUse)
		{
			_longConfirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_hotkeySession = _hotkeys.GetSessionOrEmpty().AddOrReplaceHotkey("Back", null).AddOrReplaceHotkey("Tips", delegate
			{
				TooltipsVisibilityHelper.Instance.ToggleTooltips(this);
			});
		}
	}

	private void DeinitializeHotKeys()
	{
		if (InputManager.GamePadInUse)
		{
			if (_longConfirmHotkey != null)
			{
				_longConfirmHotkey.Deinitialize();
			}
			if (_hotkeySession != null)
			{
				_hotkeySession.Dispose();
			}
		}
	}

	private void SubscribeGamepadEvents()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelButtonClicked).AddBlocker(_windowShownBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_inputArea)));
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CONFIRM_ACTION_BUTTON, OnConfirmButtonPressed).AddBlocker(_windowShownBlocker).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(_inputArea)));
		}
	}

	private void OnLongButtonConfirmed()
	{
		Confirm();
	}

	private void OnConfirmButtonPressed()
	{
		_longConfirmHandler.Pressed(OnLongButtonConfirmed);
	}

	private void OnCancelButtonClicked()
	{
		Cancel();
	}
}
