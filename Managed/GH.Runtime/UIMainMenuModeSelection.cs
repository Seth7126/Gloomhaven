#define ENABLE_LOGS
using System;
using System.Collections;
using System.Threading;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;

public class UIMainMenuModeSelection : MonoBehaviour, IShowActivity
{
	private enum EModeSelection
	{
		None,
		Sandbox,
		LevelEditor
	}

	[SerializeField]
	private GameObject CampaignButton;

	[SerializeField]
	private GameObject TitleSandbox;

	[SerializeField]
	private GameObject TitleLevelEditor;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[ReadOnlyField]
	public bool BusyLoading;

	private EModeSelection m_Mode;

	private Action m_OnModeLoadSuccessCallback;

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	public bool IsActive => base.gameObject.activeSelf;

	private void OnDestroy()
	{
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
	}

	private void OnEnable()
	{
		CampaignButton.SetActive(value: true);
		controllerArea.Enable();
		OnShow?.Invoke();
	}

	private void OnDisable()
	{
		OnHide?.Invoke();
	}

	public void SetModeSuccessfullyLoadedCallback(Action callbackToSet)
	{
		m_OnModeLoadSuccessCallback = callbackToSet;
	}

	public void ShowForSandbox()
	{
		MainMenuUIManager.Instance.mainMenu.Hide();
		m_Mode = EModeSelection.Sandbox;
		base.gameObject.SetActive(value: true);
	}

	public void ShowForLevelEditor()
	{
		ShowForLevelEditor(autoSelectRuleset: false, ScenarioManager.EDLLMode.None);
	}

	public void ShowForLevelEditor(bool autoSelectRuleset, ScenarioManager.EDLLMode rulesetToLoad)
	{
		m_Mode = EModeSelection.LevelEditor;
		MainMenuUIManager.Instance.mainMenu.Hide();
		base.gameObject.SetActive(value: true);
		if (SceneController.Instance.Modding != null)
		{
			SceneController.Instance.Modding.LevelEditorRuleset = null;
		}
		if (autoSelectRuleset)
		{
			switch (rulesetToLoad)
			{
			case ScenarioManager.EDLLMode.Campaign:
				OnCampaignRulesetClick();
				break;
			case ScenarioManager.EDLLMode.Guildmaster:
				OnGuildmasterRulesetClick();
				break;
			default:
				Debug.LogFormat("Unable to autoload into {0} ruleset, it is currently unsupported", rulesetToLoad.ToString());
				break;
			}
		}
	}

	public void OnCampaignRulesetClick()
	{
		switch (m_Mode)
		{
		case EModeSelection.Sandbox:
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign, base.gameObject, TitleSandbox));
			break;
		case EModeSelection.LevelEditor:
			LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Campaign;
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Campaign, base.gameObject, TitleLevelEditor));
			break;
		default:
			Debug.LogError("Invalid mode " + m_Mode.ToString() + " sent to OnClick");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			break;
		}
	}

	public void OnGuildmasterRulesetClick()
	{
		switch (m_Mode)
		{
		case EModeSelection.Sandbox:
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Guildmaster, base.gameObject, TitleSandbox));
			break;
		case EModeSelection.LevelEditor:
			LevelEditorController.s_Instance.LastLoadedRuleset = ScenarioManager.EDLLMode.Guildmaster;
			StartCoroutine(LoadYMLAndSetMenu(ScenarioManager.EDLLMode.Guildmaster, base.gameObject, TitleLevelEditor));
			break;
		default:
			Debug.LogError("Invalid mode " + m_Mode.ToString() + " sent to OnClick");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			break;
		}
		m_Mode = EModeSelection.None;
	}

	public void OnBackClick()
	{
		base.gameObject.SetActive(value: false);
		switch (m_Mode)
		{
		case EModeSelection.Sandbox:
			MainMenuUIManager.Instance.mainMenu.Show(instant: true);
			break;
		case EModeSelection.LevelEditor:
			MainMenuUIManager.Instance.mainMenu.OpenExtras();
			break;
		default:
			Debug.LogError("Invalid mode " + m_Mode.ToString() + " sent to OnClick");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
			break;
		}
		controllerArea.Destroy();
		m_Mode = EModeSelection.None;
		m_OnModeLoadSuccessCallback = null;
	}

	private IEnumerator LoadYMLAndSetMenu(ScenarioManager.EDLLMode mode, GameObject setInactive, GameObject setActive)
	{
		SceneController.Instance.ShowLoadingScreen();
		BusyLoading = true;
		yield return null;
		Thread loadYML = new Thread((ThreadStart)delegate
		{
			SceneController.Instance.YML.LoadSingleScenarios(mode);
		});
		loadYML.Start();
		while (loadYML.IsAlive)
		{
			yield return null;
		}
		BusyLoading = false;
		if (YMLLoading.LastLoadResult)
		{
			setInactive.SetActive(value: false);
			setActive.SetActive(value: true);
			SceneController.Instance.DisableLoadingScreen();
			m_OnModeLoadSuccessCallback?.Invoke();
		}
		else
		{
			Debug.LogError("Unable to load YML for DLL Mode: " + mode);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", Environment.StackTrace, SceneController.Instance.LoadMainMenu);
		}
		m_OnModeLoadSuccessCallback = null;
	}
}
