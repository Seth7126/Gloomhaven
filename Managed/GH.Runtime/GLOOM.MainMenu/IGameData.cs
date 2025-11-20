using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface IGameData
{
	string GameName { get; }

	EAdventureDifficulty Difficulty { get; }

	StateShared.EHouseRulesFlag HouseRules { get; }

	EGoldMode GoldMode { get; }

	DLCRegistry.EDLCKey DLCEnabled { get; }

	T GetParam<T>(string key);
}
