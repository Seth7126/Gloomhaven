using System;
using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public class GuildmasterLoadGameService : ILoadGameService
{
	private class GuildmasterSave : PartyGameSaveData
	{
		public override EGoldMode GoldMode => base.PartyAdventureData.GoldMode;

		public override int? PartyGold => base.PartyAdventureData.LastSavedPartyGoldAmount;

		public override int? Wealth => null;

		public override int? Reputation => null;

		public override List<IGameSaveCharacterData> SelectedCharacters
		{
			get
			{
				if ((base.PartyAdventureData.LastSavedSelectedCharacterIDs == null || base.PartyAdventureData.LastSavedSelectedCharacterIDs.Count == 0) && base.PartyAdventureData.AdventureMapState != null)
				{
					base.PartyAdventureData.UpdateLastSavedSelectedCharacterIDs();
				}
				return base.PartyAdventureData.LastSavedSelectedCharacterIDs.ConvertAll((Converter<Tuple<string, int>, IGameSaveCharacterData>)delegate(Tuple<string, int> it)
				{
					CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == it.Item1);
					if (cCharacterClass == null)
					{
						return new GameSaveCharacterData((ECharacter)Enum.Parse(typeof(ECharacter), it.Item1.ReplaceLastOccurrence("ID", string.Empty)), null, it.Item2, null);
					}
					int? gold = null;
					if (GoldMode == EGoldMode.CharacterGold)
					{
						Tuple<string, int, string> tuple = base.PartyAdventureData.LastSavedSelectedCharacterInfo.FirstOrDefault((Tuple<string, int, string> character) => character.Item1 == it.Item1);
						if (tuple != null)
						{
							gold = tuple.Item2;
						}
					}
					return new GameSaveCharacterData(cCharacterClass.CharacterModel, null, it.Item2, gold);
				}).ToList();
			}
		}

		public GuildmasterSave(PartyAdventureData data)
			: base(data)
		{
		}
	}

	public void DeleteGame(IGameSaveData data)
	{
		if (SaveData.Instance.Global.ResumeAdventureName == data.GameName)
		{
			SaveData.Instance.Global.ResumeAdventureName = string.Empty;
			SaveData.Instance.SaveGlobalData();
		}
		SaveData.Instance.DeletePartyData(((GuildmasterSave)data).PartyAdventureData, EGameMode.Guildmaster);
	}

	public List<IGameSaveData> GetSaves()
	{
		return ((IEnumerable<PartyAdventureData>)SaveData.Instance.Global.AllAdventures.OrderByDescending((PartyAdventureData it) => it.LastSavedTimeStamp)).Select((Func<PartyAdventureData, IGameSaveData>)((PartyAdventureData it) => new GuildmasterSave(it))).ToList();
	}

	public void LoadGame(IGameSaveData gameData)
	{
		PartyAdventureData partyAdventureData = ((GuildmasterSave)gameData).PartyAdventureData;
		SaveData.Instance.Global.DeserializeMapStateAndValidateOnLoad(EGameMode.Guildmaster, partyAdventureData);
		try
		{
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
			SaveData.Instance.LoadGuildmasterMode(partyAdventureData, isJoiningMPClient: false, loadMenuOnCancel: false, null, delegate
			{
				MainMenuUIManager.Instance.RequestDisableInteraction(disable: false, this);
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to resume Adventure game\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, delegate
			{
				try
				{
					SaveData.Instance.Global.AdventureData.MoveMapStateFileToCorruptedSaveFolder(EGameMode.Guildmaster);
				}
				catch
				{
				}
				SceneController.Instance.LoadMainMenu();
			}, ex.Message);
		}
	}

	public void EnableDLC(IGameSaveData gameData, DLCRegistry.EDLCKey dlcToAdd)
	{
		SaveData.Instance.CopySaveToBackupBeforeEnablingDLCOnIt(((GuildmasterSave)gameData).PartyAdventureData, dlcToAdd);
	}
}
