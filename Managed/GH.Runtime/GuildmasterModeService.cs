using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

public class GuildmasterModeService : MonoBehaviour, IGameModeService, ICreateGameService, ILoadGameService
{
	private GuildmasterLoadGameService m_LoadGameService = new GuildmasterLoadGameService();

	private BasicEventHandler onDeleted;

	[UsedImplicitly]
	private void OnDestroy()
	{
		onDeleted = null;
	}

	public bool CanLoadGames()
	{
		return SaveData.Instance.Global.AllAdventures.Length != 0;
	}

	public bool CanResume()
	{
		if (SaveData.Instance.Global.ResumeAdventure != null && SaveData.Instance.Global.AllAdventures.Contains(SaveData.Instance.Global.ResumeAdventure))
		{
			return !SaveData.Instance.Global.ResumeAdventure.HasInvalidDLCs();
		}
		return false;
	}

	public void CreateGame(IGameData data)
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		EGuildmasterTutorial param = data.GetParam<EGuildmasterTutorial>(typeof(EGuildmasterTutorial).ToString());
		SceneController.Instance.GuildmasterStart(data.GameName, data.Difficulty, data.HouseRules, data.DLCEnabled, data.GoldMode, param != EGuildmasterTutorial.Full, param == EGuildmasterTutorial.SkipIntro);
	}

	public void ResumeLastGame()
	{
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Guildmaster, SaveData.Instance.Global.ResumeAdventure);
		SaveData.Instance.LoadGuildmasterMode(SaveData.Instance.Global.ResumeAdventure, isJoiningMPClient: false);
	}

	public void RegisterToDeletedGame(BasicEventHandler action)
	{
		onDeleted = (BasicEventHandler)Delegate.Combine(onDeleted, action);
	}

	public bool ValidateExistsGame(string partyName)
	{
		return SaveData.Instance.ExistsPartyData(EGameMode.Guildmaster, partyName.Trim());
	}

	public List<IGameSaveData> GetSaves()
	{
		return m_LoadGameService.GetSaves();
	}

	public void DeleteGame(IGameSaveData data)
	{
		m_LoadGameService.DeleteGame(data);
		onDeleted?.Invoke();
	}

	public void LoadGame(IGameSaveData data)
	{
		m_LoadGameService.LoadGame(data);
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		m_LoadGameService.EnableDLC(data, dlcToAdd);
	}
}
