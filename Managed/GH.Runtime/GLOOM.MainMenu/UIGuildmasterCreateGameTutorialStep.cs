using System;
using Assets.Script.Misc;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIGuildmasterCreateGameTutorialStep : UICreateGameStep
{
	[Header("Tutorial")]
	[SerializeField]
	private UIGuildmasterTutorialSelector m_TutorialSelector;

	[SerializeField]
	private Hotkey _confirmHotkey;

	[SerializeField]
	private Hotkey _backHotkey;

	protected override void Awake()
	{
		base.Awake();
		if (!_isConfirmedCallbackSetted)
		{
			confirmButton.onClick.AddListener(base.OnConfirmButtonClicked);
		}
		m_Window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				base.gameObject.SetActive(value: false);
			}
		});
	}

	[UsedImplicitly]
	private new void OnDestroy()
	{
		DeinitGamepadHotkeys();
		confirmButton.onClick.RemoveListener(base.OnConfirmButtonClicked);
	}

	public override ICallbackPromise Show(IGameModeService service, GameData gameData, bool instant = false)
	{
		if (!string.IsNullOrEmpty(SaveData.Instance.Global.CurrentModdedRuleset))
		{
			gameData.AddParam(typeof(EGuildmasterTutorial).ToString(), EGuildmasterTutorial.SkipIntro);
			return CallbackPromise.Resolved();
		}
		base.gameObject.SetActive(value: true);
		InitGamepadHotkeys();
		InitGamepadButtons();
		return base.Show(service, gameData, instant);
	}

	private void InitGamepadButtons()
	{
		if (InputManager.GamePadInUse)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnControllerSubmit).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		}
	}

	private void DeinitGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnControllerSubmit);
		}
	}

	private void OnControllerSubmit()
	{
		OnConfirmButtonClicked();
	}

	protected override void Setup(IGameModeService service, GameData data, Action onConfirmed, Action onCancelled = null)
	{
		m_TutorialSelector.SetMode(EGuildmasterTutorial.Full);
		base.Setup(service, data, onConfirmed, onCancelled);
		EnableConfirmationButton(enable: true);
	}

	private void InitGamepadHotkeys()
	{
		if (InputManager.GamePadInUse)
		{
			_confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_backHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void DeinitGamepadHotkeys()
	{
		if (InputManager.GamePadInUse)
		{
			_confirmHotkey.Deinitialize();
			_backHotkey.Deinitialize();
		}
	}

	public override void Hide(bool instant = false)
	{
		if (base.gameObject.activeSelf)
		{
			DeinitGamepadHotkeys();
			DeinitGamepadInput();
			base.Hide(instant);
		}
	}

	protected override void OnConfirmedStep()
	{
		m_Data.AddParam(typeof(EGuildmasterTutorial).ToString(), m_TutorialSelector.GetSelectedMode());
		base.OnConfirmedStep();
	}
}
