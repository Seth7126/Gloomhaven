using System;
using System.Collections.Generic;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class MainOptionExtras : MainOptionOpenSuboptions
{
	[SerializeField]
	private MenuOptionIcon compendiumIcon;

	[SerializeField]
	private MenuOptionIcon levelEditorIcon;

	[SerializeField]
	private UIMainMenuModeSelection levelEditor;

	[SerializeField]
	private MenuOptionIcon moddingIcon;

	[SerializeField]
	private GameObject moddingUI;

	[SerializeField]
	private MenuOptionIcon creditsIcon;

	[SerializeField]
	private UISubmenuGOWindow creditsWindow;

	private UIWindow compendiumWindow;

	private MenuSuboption compendiumOpt;

	protected override void Awake()
	{
		base.Awake();
		compendiumOpt = new MenuSuboption("GUI_HOW_TO_PLAY", compendiumIcon, delegate
		{
			compendiumWindow = Singleton<SpecialUIProvider>.Instance.CompendiumUIObject.GetComponent<UIWindow>();
			MainMenuUIManager.Instance.mainMenu.Hide();
			compendiumWindow.Show();
			compendiumWindow.onHidden.AddListener(OnCloseCompendium);
		});
	}

	private void Start()
	{
		creditsWindow.OnHidden.AddListener(OpenExtras);
	}

	private void OpenExtras()
	{
		MainMenuUIManager.Instance.mainMenu.OpenExtras();
	}

	private void OnCloseCompendium()
	{
		MainMenuUIManager.Instance.mainMenu.OpenExtras();
		compendiumWindow.onHidden.RemoveListener(OnCloseCompendium);
	}

	public override List<MenuSuboption> BuildOptions()
	{
		compendiumOpt.Reset();
		List<MenuSuboption> list = new List<MenuSuboption> { compendiumOpt };
		if (PlatformLayer.Modding.LevelEditorSupported)
		{
			MenuSuboption levelEditorOpt = null;
			levelEditorOpt = new MenuSuboption("GUI_LEVEL_EDITOR", levelEditorIcon, delegate
			{
				OpenLevelEditor(levelEditorOpt.Deselect);
			}, null, interactable: true, LocalizationManager.GetTranslation("Consoles/GUI_MAIN_MENU_LEVEL_EDITOR_TOOLTIP"));
			list.Add(levelEditorOpt);
		}
		if (PlatformLayer.Modding.ModdingSupported)
		{
			MenuSuboption moddingOpt = null;
			moddingOpt = new MenuSuboption("GUI_MODDING", moddingIcon, delegate
			{
				OpenModding(moddingOpt.Deselect);
			}, null, interactable: true, LocalizationManager.GetTranslation("Consoles/GUI_MAIN_MENU_MODDING_TOOLTIP"));
			list.Add(moddingOpt);
		}
		list.Add(new MenuSuboption("GUI_CREDITS", creditsIcon, OpenCredits));
		return list;
	}

	private void OpenModding(Action cancelAction)
	{
		if (Singleton<InputManager>.Instance.IsPCAndGamepadVersion() && InputManager.GamePadInUse)
		{
			UIConfirmationBoxManager.MainMenuInstance.ShowGenericConfirmation(LocalizationManager.GetTranslation("Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY"), null, delegate
			{
				SelectInputDeviceBox.ReloadSceneWithChangeDevice(isUseGamepad: false, delegate
				{
					MainMenuUIManager.Instance.mainMenu.OpenExtras();
				});
			}, delegate
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.SubMenuOptionsWithSelected);
				cancelAction?.Invoke();
			}, "Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY_SWITCH_INPUT", "GUI_CANCEL", null, showHeader: true, enableSoftlockReport: true, null, resetAfterAction: false, "Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY_HEADER");
		}
		else
		{
			moddingUI.SetActive(value: true);
			MainMenuUIManager.Instance.mainMenu.Hide();
		}
	}

	private void OpenLevelEditor(Action cancelAction)
	{
		if (Singleton<InputManager>.Instance.IsPCAndGamepadVersion() && InputManager.GamePadInUse)
		{
			UIConfirmationBoxManager.MainMenuInstance.ShowGenericConfirmation(LocalizationManager.GetTranslation("Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY"), null, delegate
			{
				SelectInputDeviceBox.ReloadSceneWithChangeDevice(isUseGamepad: false, delegate
				{
					MainMenuUIManager.Instance.mainMenu.OpenExtras();
				});
			}, delegate
			{
				Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.SubMenuOptionsWithSelected);
				cancelAction?.Invoke();
			}, "Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY_SWITCH_INPUT", "GUI_CANCEL", null, showHeader: true, enableSoftlockReport: true, null, resetAfterAction: false, "Consoles/GUI_CONFIRMATION_KEYBOARD_MOUSE_ONLY_HEADER");
		}
		else
		{
			MainMenuUIManager.Instance.mainMenu.Hide();
			levelEditor.ShowForLevelEditor();
		}
	}

	private void OpenCredits()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		creditsWindow.Show();
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.Credits);
	}
}
