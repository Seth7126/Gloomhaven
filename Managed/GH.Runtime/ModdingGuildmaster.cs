using System;
using System.Collections;
using GLOOM;
using GLOOM.MainMenu;
using UnityEngine;

public class ModdingGuildmaster : MonoBehaviour
{
	public GameObject ResumeButton;

	public GameObject ResumeButtonDummy;

	public GameObject LoadAdventureButton;

	public GameObject LoadAdventureButtonDummy;

	public GuildmasterModeService service;

	public UICreateGameWindow CreateGameWindow;

	private GHRuleset m_Ruleset;

	public bool Init(GHRuleset ruleset)
	{
		try
		{
			if (ruleset.CompiledHash != ruleset.GetRulesetHash())
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_HASH_COMPARE_FAIL_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_HASH_COMPARE_FAIL_MESSAGE"), "GUI_CLOSE");
				return false;
			}
			m_Ruleset = ruleset;
			CoroutineHelper.RunCoroutine(InitCoroutine());
			return true;
		}
		catch (Exception ex)
		{
			m_Ruleset = null;
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
			return true;
		}
	}

	private IEnumerator InitCoroutine()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		SceneController.Instance.YML.Unload(regenCards: true);
		yield return SceneController.Instance.YML.RegenCardsCoroutine();
		SaveData.Instance.Global.CurrentModdedRuleset = m_Ruleset.Name;
		yield return SceneController.Instance.YML.LoadRulesetZip(m_Ruleset);
		if (!YMLLoading.LastLoadResult)
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			yield break;
		}
		try
		{
			bool flag = SaveData.Instance.Global.ResumeAdventure != null;
			ResumeButton.SetActive(flag);
			ResumeButtonDummy.SetActive(!flag);
			if (SaveData.Instance.Global.AllAdventures.Length == 0)
			{
				LoadAdventureButton.SetActive(value: false);
				LoadAdventureButtonDummy.SetActive(value: true);
			}
			else
			{
				LoadAdventureButton.SetActive(value: true);
				LoadAdventureButtonDummy.SetActive(value: false);
			}
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			SceneController.Instance.DisableLoadingScreen();
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			m_Ruleset = null;
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	public void ResumeAdventure()
	{
		service.ResumeLastGame();
	}

	public void CreateAdventure()
	{
		base.gameObject.SetActive(value: false);
		CreateGameWindow.Show(service, delegate
		{
			base.gameObject.SetActive(value: true);
		});
	}

	public void LoadAdventure()
	{
		base.gameObject.SetActive(value: false);
		Singleton<UILoadGameWindow>.Instance.Show(service, delegate
		{
			base.gameObject.SetActive(value: true);
		});
	}
}
