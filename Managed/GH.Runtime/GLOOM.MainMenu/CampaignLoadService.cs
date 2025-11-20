using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public class CampaignLoadService : ILoadGameService
{
	private class CampaignSave : PartyGameSaveData
	{
		public override EGoldMode GoldMode => EGoldMode.CharacterGold;

		public override int? PartyGold => null;

		public override int? Wealth => base.PartyAdventureData.LastSavedWealth;

		public override int? Reputation => base.PartyAdventureData.LastSavedReputation;

		public override List<IGameSaveCharacterData> SelectedCharacters
		{
			get
			{
				if ((base.PartyAdventureData.LastSavedSelectedCharacterIDs == null || base.PartyAdventureData.LastSavedSelectedCharacterIDs.Count == 0) && base.PartyAdventureData.AdventureMapState != null)
				{
					base.PartyAdventureData.UpdateLastSavedSelectedCharacterIDs();
				}
				if (base.PartyAdventureData.LastSavedSelectedCharacterIDs.Count != base.PartyAdventureData.LastSavedSelectedCharacterInfo.Count)
				{
					base.PartyAdventureData.RefreshMapStateFromFile(base.PartyAdventureData.AdventureMapStateFilePath);
					base.PartyAdventureData.UpdateLastSavedSelectedCharacterGold();
				}
				return base.PartyAdventureData.LastSavedSelectedCharacterIDs.ConvertAll((Converter<Tuple<string, int>, IGameSaveCharacterData>)delegate(Tuple<string, int> it)
				{
					CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == it.Item1);
					if (cCharacterClass == null)
					{
						return new GameSaveCharacterData((ECharacter)Enum.Parse(typeof(ECharacter), it.Item1.ReplaceLastOccurrence("ID", string.Empty)), null, it.Item2, null);
					}
					Tuple<string, int, string> tuple = base.PartyAdventureData.LastSavedSelectedCharacterInfo.FirstOrDefault((Tuple<string, int, string> character) => character.Item1 == it.Item1);
					return (tuple != null) ? new GameSaveCharacterData(cCharacterClass.CharacterModel, tuple.Item3, it.Item2, tuple.Item2) : new GameSaveCharacterData(cCharacterClass.CharacterModel, null, it.Item2, null);
				}).ToList();
			}
		}

		public CampaignSave(PartyAdventureData data)
			: base(data)
		{
		}
	}

	public PartyAdventureData GetAdventureData(IGameSaveData data)
	{
		return ((CampaignSave)data).PartyAdventureData;
	}

	public void DeleteGame(IGameSaveData data)
	{
		if (SaveData.Instance.Global.ResumeCampaignName == data.GameName)
		{
			SaveData.Instance.Global.ResumeCampaignName = string.Empty;
			SaveData.Instance.SaveGlobalData();
		}
		SaveData.Instance.DeletePartyData(GetAdventureData(data), EGameMode.Campaign);
	}

	public List<IGameSaveData> GetSaves()
	{
		if (SaveData.Instance.Global.CurrentModdedRuleset.IsNullOrEmpty())
		{
			return ((IEnumerable<PartyAdventureData>)(from it in SaveData.Instance.Global.AllCampaigns
				where !it.IsModded
				orderby it.LastSavedTimeStamp descending
				select it)).Select((Func<PartyAdventureData, IGameSaveData>)((PartyAdventureData it) => new CampaignSave(it))).ToList();
		}
		return ((IEnumerable<PartyAdventureData>)SaveData.Instance.Global.ModdedCampaigns.OrderByDescending((PartyAdventureData it) => it.LastSavedTimeStamp)).Select((Func<PartyAdventureData, IGameSaveData>)((PartyAdventureData it) => new CampaignSave(it))).ToList();
	}

	public void LoadGame(IGameSaveData gameData)
	{
		PartyAdventureData adventureData = GetAdventureData(gameData);
		SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Campaign, adventureData);
		try
		{
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
			SaveData.Instance.LoadCampaignMode(adventureData, isJoiningMPClient: false, loadMenuOnCancel: false, null, delegate
			{
				MainMenuUIManager.Instance.RequestDisableInteraction(disable: false, this);
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to resume Campaign game\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, delegate
			{
				try
				{
					SaveData.Instance.Global.CampaignData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Campaign);
				}
				catch
				{
				}
				SceneController.Instance.LoadMainMenu();
			}, ex.Message);
		}
	}

	public void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		SaveData.Instance.SaveQueue.EnqueueOperation(delegate(Action onCompleted)
		{
			SaveData.Instance.CopySaveToBackupBeforeEnablingDLCOnIt(GetAdventureData(data), dlcToAdd);
			onCompleted?.Invoke();
		});
	}
}
