using System.Collections.Generic;
using GLOOM.MainMenu;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;

internal class LoadCustomLevelDataService : ILoadGameService
{
	private CCustomLevelData m_LevelData;

	private ILoadGameService m_LoadGameService;

	public LoadCustomLevelDataService(CCustomLevelData levelData)
	{
		m_LevelData = levelData;
		GHRuleset gHRuleset = SceneController.Instance.Modding?.LevelEditorRuleset;
		if (gHRuleset != null)
		{
			SaveData.Instance.Global.CurrentModdedRuleset = gHRuleset.Name;
			if (gHRuleset.RulesetType == GHRuleset.ERulesetType.Campaign)
			{
				m_LoadGameService = new CampaignLoadService();
			}
			else
			{
				m_LoadGameService = new GuildmasterLoadGameService();
			}
		}
		else if (LevelEditorController.s_Instance.LastLoadedRuleset == ScenarioManager.EDLLMode.Campaign)
		{
			m_LoadGameService = new CampaignLoadService();
		}
		else
		{
			m_LoadGameService = new GuildmasterLoadGameService();
		}
	}

	public List<IGameSaveData> GetSaves()
	{
		return m_LoadGameService.GetSaves();
	}

	public void DeleteGame(IGameSaveData data)
	{
		m_LoadGameService.DeleteGame(data);
	}

	public void LoadGame(IGameSaveData data)
	{
		MainMenuUIManager.Instance.CustomPartySetupScreen.ShowForCustomLevel(m_LevelData, ((PartyGameSaveData)data).PartyAdventureData);
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		m_LoadGameService.EnableDLC(data, dlcToAdd);
	}
}
