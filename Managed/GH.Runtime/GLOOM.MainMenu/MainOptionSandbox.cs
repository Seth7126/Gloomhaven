using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionSandbox : MainOptionOpenSuboptions
{
	[SerializeField]
	private SandboxUI sandboxUI;

	[SerializeField]
	private MenuOptionIcon icon;

	public override List<MenuSuboption> BuildOptions()
	{
		return new List<MenuSuboption>
		{
			new MenuSuboption("GUI_ADVENTURE2", icon, delegate
			{
				StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Guildmaster));
			}),
			new MenuSuboption("GUI_CAMPAIGN", icon, delegate
			{
				StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign));
			})
		};
	}

	private IEnumerator LoadYMLAndSetMenu(ScenarioManager.EDLLMode mode)
	{
		SceneController.Instance.ShowLoadingScreen();
		yield return null;
		MainMenuUIManager.Instance.mainMenu.Hide();
		Thread loadYML = new Thread((ThreadStart)delegate
		{
			SceneController.Instance.YML.LoadSingleScenarios(mode);
		});
		loadYML.Start();
		while (loadYML.IsAlive)
		{
			yield return null;
		}
		if (YMLLoading.LastLoadResult)
		{
			SceneController.Instance.DisableLoadingScreen();
			ShowSandbox();
		}
		else
		{
			Debug.LogError("Unable to load YML for DLL Mode: " + mode);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
		}
	}

	private void ShowSandbox()
	{
		sandboxUI.gameObject.SetActive(value: true);
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.SandboxOptions);
	}
}
