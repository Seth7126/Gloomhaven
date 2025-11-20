using System;
using System.Collections.Generic;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public abstract class PartyGameSaveData : IGameSaveData
{
	public PartyAdventureData PartyAdventureData { get; }

	public string GameName => PartyAdventureData.PartyName;

	public string DisplayGameName => PartyAdventureData.DisplayPartyName;

	public SaveOwner Owner => PartyAdventureData.Owner;

	public DateTime LastSavedTimeStamp => PartyAdventureData.LastSavedTimeStamp;

	public string CurrentQuest => PartyAdventureData.LastSavedQuestName;

	public abstract EGoldMode GoldMode { get; }

	public abstract int? PartyGold { get; }

	public abstract int? Wealth { get; }

	public abstract int? Reputation { get; }

	public abstract List<IGameSaveCharacterData> SelectedCharacters { get; }

	protected PartyGameSaveData(PartyAdventureData data)
	{
		PartyAdventureData = data;
	}

	public List<DLCRegistry.EDLCKey> GetDLCAvailables()
	{
		List<DLCRegistry.EDLCKey> list = new List<DLCRegistry.EDLCKey>();
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey in dLCKeys)
		{
			if (PlatformLayer.DLC.UserInstalledDLC(eDLCKey))
			{
				list.Add(eDLCKey);
			}
		}
		return list;
	}

	public bool IsDLCActive(DLCRegistry.EDLCKey dlcKey)
	{
		return PartyAdventureData.DLCEnabled.HasFlag(dlcKey);
	}

	public bool HasInvalidDLCs(out List<DLCRegistry.EDLCKey> dlcInvalid)
	{
		dlcInvalid = new List<DLCRegistry.EDLCKey>();
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey dlcKey in dLCKeys)
		{
			if (dlcKey != DLCRegistry.EDLCKey.None)
			{
				if (PartyAdventureData.DLCEnabled.HasFlag(dlcKey) && !PlatformLayer.DLC.CanPlayDLC(dlcKey))
				{
					dlcInvalid.Add(dlcKey);
				}
				else if (SelectedCharacters.Exists((IGameSaveCharacterData it) => DLCRegistry.AllDLCCharacters[dlcKey].Contains(it.CharacterModel)) && !PlatformLayer.DLC.CanPlayDLC(dlcKey))
				{
					dlcInvalid.Add(dlcKey);
				}
			}
		}
		return !dlcInvalid.IsNullOrEmpty();
	}
}
