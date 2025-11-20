using System;
using System.Collections;
using Code.State;
using FFSNet;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SelectInputDeviceBox : Singleton<SelectInputDeviceBox>
{
	public enum ESelectInputReason
	{
		DisconnectGamepad,
		Options
	}

	[SerializeField]
	private UIButtonExtended _mouseKeyboard;

	[SerializeField]
	private UIButtonExtended _gamepad;

	[SerializeField]
	private TextLocalizedListener _header;

	[SerializeField]
	private TextLocalizedListener _info;

	[SerializeField]
	private TextLocalizedListener _warning;

	[SerializeField]
	private GameObject _pressEnter;

	private UiNavigationBlocker _navigationBlocker = new UiNavigationBlocker("SelectInputDeviceBox");

	private ESelectInputReason _reason;

	private UIWindow _window;

	private const string headerPCGamepadConnected = "Consoles/GUI_PC_Gamepad_Header_Connected";

	private const string headerPCGamepadDisconnected = "Consoles/GUI_PC_Gamepad_Header_Disconnected";

	private const string infoPCGamepadConnected = "Consoles/GUI_PC_Gamepad_Info_Connected";

	private const string infoPCGamepadDisconnected = "Consoles/GUI_PC_Gamepad_Info_Disconnected";

	private const string warningClient = "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_CLIENT";

	private const string warningHost = "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_HOST";

	private IStateFilter _filter = new StateFilterByType(typeof(SelectInputDeviceBoxState));

	private UINavigationSelectable GamepadSelectable => _gamepad.GetComponent<UINavigationSelectable>();

	protected override void Awake()
	{
		_window = GetComponent<UIWindow>();
		base.Awake();
		_mouseKeyboard.onClick.AddListener(OnMouseKeyboardClicked);
		_gamepad.onClick.AddListener(OnGamepadClicked);
		_window.onHidden.AddListener(OnHide);
	}

	protected override void OnDestroy()
	{
		_mouseKeyboard.onClick.RemoveListener(OnMouseKeyboardClicked);
		_gamepad.onClick.RemoveListener(OnGamepadClicked);
		base.OnDestroy();
	}

	public void Active(ESelectInputReason reason)
	{
		StartCoroutine(ActivateWindowCoroutine(reason));
	}

	private IEnumerator ActivateWindowCoroutine(ESelectInputReason reason)
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		if (!Singleton<InputManager>.Instance.IsPCVersion())
		{
			Disable();
			yield break;
		}
		_reason = reason;
		base.gameObject.SetActive(value: true);
		InputManager.RequestDisableInput(this, EKeyActionTag.UI);
		ControllerInputScroll.SetGlobalEnabled(enabled: false);
		_navigationBlocker.Block();
		if (FFSNetwork.IsOnline)
		{
			_warning.gameObject.SetActive(value: true);
			_warning.SetTextKey(FFSNetwork.IsHost ? "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_HOST" : "Consoles/GUI_PC_GAMEPAD_ADDED_WARNING_CLIENT");
		}
		else
		{
			_warning.gameObject.SetActive(value: false);
		}
		Singleton<UINavigation>.Instance.StateMachine.SetFilter(_filter);
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.SelectInputDeviceBox);
		switch (_reason)
		{
		case ESelectInputReason.DisconnectGamepad:
			SetState("Consoles/GUI_PC_Gamepad_Header_Disconnected", "Consoles/GUI_PC_Gamepad_Info_Disconnected", hasEnter: true, hasGamepadButton: false);
			InputManager.RegisterToOnPressed(KeyAction.MENU_ENTER, OnEnterClick);
			break;
		case ESelectInputReason.Options:
			_navigationBlocker.Unblock();
			InputManager.EnableInput();
			Singleton<UINavigation>.Instance.NavigationManager.TrySelect(GamepadSelectable);
			SetState("Consoles/GUI_PC_Gamepad_Header_Connected", "Consoles/GUI_PC_Gamepad_Info_Disconnected", hasEnter: false, hasGamepadButton: true);
			break;
		}
		_window.Show(instant: true);
	}

	public void Disable()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.UI);
		if (_reason == ESelectInputReason.DisconnectGamepad)
		{
			InputManager.UnregisterToOnPressed(KeyAction.MENU_ENTER, OnEnterClick);
		}
		_navigationBlocker.Unblock();
		_window.Hide();
	}

	private void OnHide()
	{
		ControllerInputScroll.SetGlobalEnabled(enabled: true);
		Singleton<UINavigation>.Instance.StateMachine.RemoveFilter();
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState is SelectInputDeviceBoxState)
		{
			Singleton<UINavigation>.Instance.StateMachine.ToPreviousState();
		}
		Singleton<UINavigation>.Instance.StateMachine.RemoveLast(Singleton<UINavigation>.Instance.StateMachine.GetState(PopupStateTag.SelectInputDeviceBox));
	}

	private void OnEnterClick()
	{
		OnMouseKeyboardClicked();
	}

	private void OnMouseKeyboardClicked()
	{
		SetInputDevice(isUseGamepad: false);
	}

	private void OnGamepadClicked()
	{
		SetInputDevice(isUseGamepad: true);
	}

	private void SetState(string header, string info, bool hasEnter, bool hasGamepadButton)
	{
		_header.SetTextKey(header);
		_info.SetTextKey(info);
		_pressEnter.SetActive(hasEnter);
		_gamepad.gameObject.SetActive(hasGamepadButton);
	}

	private void SetInputDevice(bool isUseGamepad)
	{
		bool gamePadInUse = InputManager.GamePadInUse;
		Disable();
		switch (_reason)
		{
		case ESelectInputReason.DisconnectGamepad:
			if (gamePadInUse == isUseGamepad)
			{
				break;
			}
			if (FFSNetwork.IsOnline)
			{
				UIManager.LoadMainMenu(null, delegate
				{
					ChangeDevice(isUseGamepad);
					PersistentData.s_Instance.ClearCardPools();
				});
			}
			else
			{
				ReloadSceneWithChangeDevice(isUseGamepad);
			}
			break;
		case ESelectInputReason.Options:
			Singleton<UIOptionsWindow>.Instance.Hide();
			if (gamePadInUse == isUseGamepad)
			{
				break;
			}
			if (FFSNetwork.IsOnline)
			{
				UIManager.LoadMainMenu(null, delegate
				{
					ChangeDevice(isUseGamepad);
					PersistentData.s_Instance.ClearCardPools();
				});
			}
			else
			{
				ReloadSceneWithChangeDevice(isUseGamepad);
			}
			break;
		}
	}

	public static void ChangeDevice(bool isUseGamepad)
	{
		Singleton<InputManager>.Instance.SetGamepadInputDevice(isUseGamepad);
		InputManager.UpdateMouseInputEnabled(!isUseGamepad);
		Singleton<InputManager>.Instance.IsPlayerSelectInputDevice = true;
		if (isUseGamepad)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public static void ReloadSceneWithChangeDevice(bool isUseGamepad, Action onLoaded = null)
	{
		bool isClient = FFSNetwork.IsClient;
		Singleton<UIConfirmationBoxManager>.Instance?.Hide();
		UIConfirmationBoxManager.MainMenuInstance?.Hide();
		switch (SaveData.Instance.Global.GameMode)
		{
		case EGameMode.Guildmaster:
			PlayerRegistry.LoadingInFromJoiningClient = isClient;
			SaveData.Instance.LoadGuildmasterMode(SaveData.Instance.Global.ResumeAdventure, isClient, loadMenuOnCancel: true, delegate
			{
				PersistentData.s_Instance.ClearCardPools();
				if (SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState != null)
				{
					ScenarioRuleClient.Stop();
				}
				ChangeDevice(isUseGamepad);
			}, null, refreshMapState: true);
			break;
		case EGameMode.Campaign:
			PlayerRegistry.LoadingInFromJoiningClient = isClient;
			SaveData.Instance.LoadCampaignMode(SaveData.Instance.Global.ResumeCampaign, isClient, loadMenuOnCancel: true, delegate
			{
				PersistentData.s_Instance.ClearCardPools();
				if (SaveData.Instance.Global.CurrentAdventureData?.AdventureMapState != null)
				{
					ScenarioRuleClient.Stop();
				}
				ChangeDevice(isUseGamepad);
			}, null, refreshMapState: true);
			break;
		case EGameMode.MainMenu:
			UIManager.LoadMainMenu(onLoaded, delegate
			{
				ChangeDevice(isUseGamepad);
				PersistentData.s_Instance.ClearCardPools();
			});
			break;
		case EGameMode.FrontEndTutorial:
		{
			CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(SaveData.Instance.Global.CurrentFrontEndTutorialFilename);
			SaveData.Instance.LoadCustomLevelFromData(customLevel, LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
			SceneController.Instance.LoadCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData, isFrontEndTutorial: true, delegate
			{
				ScenarioRuleClient.Stop();
				ChangeDevice(isUseGamepad);
			});
			break;
		}
		case EGameMode.LevelEditor:
		case EGameMode.SingleScenario:
		case EGameMode.Autotest:
			break;
		}
	}
}
