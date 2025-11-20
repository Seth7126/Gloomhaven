using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GLOOM.MainMenu;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITutorialSelectorWindow : UISubmenuGOWindow
{
	[SerializeField]
	private List<UITutorialSlot> slotPool;

	[SerializeField]
	private ExtendedScrollRect scroll;

	[Header("Hotkeys")]
	[SerializeField]
	private Hotkey _scrollHotkey;

	private ITutorialService service;

	private const int IncreasedStackSize = 1048576;

	protected override void Awake()
	{
		base.Awake();
		service = GetComponent<ITutorialService>();
		CreateTutorialOptions(service.GetTutorials());
		SubscribeOnGamepadEvents();
		if (InputManager.GamePadInUse)
		{
			_scrollHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			_scrollHotkey.DisplayHotkey(active: true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnsubscribeOnGamepadEvents();
	}

	private void UnsubscribeOnGamepadEvents()
	{
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_CANCEL, OnCancelButtonPressed);
	}

	private void SubscribeOnGamepadEvents()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_CANCEL, OnCancelButtonPressed).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)));
		}
	}

	private void OnCancelButtonPressed()
	{
		Hide();
	}

	private void CreateTutorialOptions(List<ITutorial> tutorials)
	{
		HelperTools.NormalizePool(ref slotPool, slotPool[0].gameObject, scroll.content, tutorials.Count);
		for (int i = 0; i < tutorials.Count; i++)
		{
			ITutorial tutorial = tutorials[i];
			slotPool[i].SetTutorial(tutorial, service.IsTutorialComplete(tutorial), delegate
			{
				StartTutorial(tutorial);
			}, OnHoveredSlot);
		}
	}

	private void OnHoveredSlot(UITutorialSlot slot, bool hovered)
	{
		if (InputManager.GamePadInUse && hovered)
		{
			scroll.ScrollToFit(slot.transform as RectTransform);
		}
	}

	private void OnEnable()
	{
		scroll.ScrollToTop();
	}

	private IEnumerator StartTutorialCoroutine(ITutorial tutorial)
	{
		SceneController.Instance.ShowLoadingScreen();
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		if (ScenarioRuleClient.SRLYML.YMLMode == CSRLYML.EYMLMode.Global)
		{
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
			yield return null;
			Thread loadYML = new Thread((ThreadStart)delegate
			{
				SceneController.Instance.YML.LoadGuildMaster(DLCRegistry.EDLCKey.None);
			}, 1048576);
			loadYML.Start();
			while (loadYML.IsAlive)
			{
				yield return null;
			}
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: false, this);
			if (!YMLLoading.LastLoadResult)
			{
				window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
				Debug.LogError("Unable to load Guildmaster YML");
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
				yield break;
			}
		}
		try
		{
			SceneController.Instance.DisableLoadingScreen();
			service.StartTutorial(tutorial);
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load Tutorial Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	private void StartTutorial(ITutorial tutorial)
	{
		if (InputManager.GetWasPressed(KeyAction.UI_SUBMIT) || InputManager.GetWasReleased(KeyAction.UI_SUBMIT) || !InputManager.GetIsPressed(KeyAction.UI_SUBMIT))
		{
			StartCoroutine(StartTutorialCoroutine(tutorial));
		}
	}

	protected override void OnControllerFocused()
	{
		base.OnControllerFocused();
		for (int i = 0; i < slotPool.Count && slotPool[i].gameObject.activeSelf; i++)
		{
			slotPool[i].EnableNavigation();
		}
		EventSystem.current.SetSelectedGameObject(slotPool.FirstOrDefault((UITutorialSlot it) => it.gameObject.activeSelf)?.gameObject);
	}

	protected override void OnControllerUnfocused()
	{
		base.OnControllerUnfocused();
		for (int i = 0; i < slotPool.Count && slotPool[i].gameObject.activeSelf; i++)
		{
			slotPool[i].DisableNavigation();
		}
	}
}
