using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using UnityEngine;

public class CampaignModeService : MonoBehaviour, IGameModeService, ICreateGameService, ILoadGameService
{
	private CampaignLoadService loadService = new CampaignLoadService();

	private BasicEventHandler onDeleted;

	[UsedImplicitly]
	private void OnDestroy()
	{
		onDeleted = null;
	}

	public void CreateGame(IGameData data)
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		SceneController.Instance.NewCampaignStart(data.GameName, data.Difficulty, data.HouseRules, data.DLCEnabled, EGoldMode.CharacterGold, data.GetParam<EEnhancementMode>(typeof(EEnhancementMode).ToString()));
	}

	public bool ValidateExistsGame(string partyName)
	{
		return SaveData.Instance.ExistsPartyData(EGameMode.Campaign, partyName);
	}

	public bool CanLoadGames()
	{
		return SaveData.Instance.Global.AllCampaigns.Any((PartyAdventureData x) => !x.IsModded);
	}

	public bool CanResume()
	{
		PartyAdventureData resumeCampaign = SaveData.Instance.Global.ResumeCampaign;
		if (resumeCampaign != null && SaveData.Instance.Global.AllCampaigns.Contains(resumeCampaign))
		{
			return !resumeCampaign.HasInvalidDLCs();
		}
		return false;
	}

	public void ResumeLastGame()
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Campaign, SaveData.Instance.Global.ResumeCampaign);
		SaveData.Instance.LoadCampaignMode(SaveData.Instance.Global.ResumeCampaign, isJoiningMPClient: false);
	}

	public void RegisterToDeletedGame(BasicEventHandler action)
	{
		onDeleted = (BasicEventHandler)Delegate.Combine(onDeleted, action);
	}

	public List<IGameSaveData> GetSaves()
	{
		return loadService.GetSaves();
	}

	public void DeleteGame(IGameSaveData data)
	{
		loadService.DeleteGame(data);
		onDeleted?.Invoke();
	}

	public void LoadGame(IGameSaveData data)
	{
		loadService.LoadGame(data);
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		loadService.EnableDLC(data, dlcToAdd);
	}
}
