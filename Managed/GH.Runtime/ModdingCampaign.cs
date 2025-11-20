using System;
using System.Collections;
using GLOOM;
using GLOOM.MainMenu;
using UnityEngine;

public class ModdingCampaign : MonoBehaviour
{
	public GameObject ResumeButton;

	public GameObject LoadCampaignButton;

	public CampaignModeService service;

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
		SceneController.Instance.YML.Unload(regenCards: true);
		yield return SceneController.Instance.YML.RegenCardsCoroutine();
		SaveData.Instance.Global.CurrentModdedRuleset = m_Ruleset.Name;
		yield return SceneController.Instance.YML.LoadRulesetZip(m_Ruleset);
		if (!YMLLoading.LastLoadResult)
		{
			yield break;
		}
		try
		{
			bool active = SaveData.Instance.Global.ResumeCampaign != null;
			ResumeButton.SetActive(active);
			if (SaveData.Instance.Global.ModdedCampaigns.Count == 0)
			{
				LoadCampaignButton.SetActive(value: false);
			}
			else
			{
				LoadCampaignButton.SetActive(value: true);
			}
			SceneController.Instance.DisableLoadingScreen();
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			m_Ruleset = null;
			Debug.LogError("Unable to load Modded Ruleset Menu\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, SceneController.Instance.LoadMainMenu, ex.Message);
		}
	}

	public void ResumeAdventure()
	{
		service.ResumeLastGame();
	}

	public void LoadAdventure()
	{
		base.gameObject.SetActive(value: false);
		Singleton<UILoadGameWindow>.Instance.Show(service, delegate
		{
			base.gameObject.SetActive(value: true);
		});
	}

	public void CreateAdventure()
	{
		base.gameObject.SetActive(value: false);
		CreateGameWindow.Show(service, delegate
		{
			base.gameObject.SetActive(value: true);
		});
	}
}
