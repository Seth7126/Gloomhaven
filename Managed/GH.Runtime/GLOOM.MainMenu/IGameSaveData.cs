using System;
using System.Collections.Generic;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface IGameSaveData
{
	string GameName { get; }

	string DisplayGameName { get; }

	SaveOwner Owner { get; }

	DateTime LastSavedTimeStamp { get; }

	EGoldMode GoldMode { get; }

	int? PartyGold { get; }

	int? Wealth { get; }

	int? Reputation { get; }

	string CurrentQuest { get; }

	List<IGameSaveCharacterData> SelectedCharacters { get; }

	List<DLCRegistry.EDLCKey> GetDLCAvailables();

	bool IsDLCActive(DLCRegistry.EDLCKey dlcKey);

	bool HasInvalidDLCs(out List<DLCRegistry.EDLCKey> invalidDLCs);
}
