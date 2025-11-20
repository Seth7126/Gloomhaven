using System.Collections.Generic;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public class GameData : IGameData
{
	private readonly Dictionary<string, object> extraData;

	public string GameName { get; set; }

	public EAdventureDifficulty Difficulty { get; set; }

	public StateShared.EHouseRulesFlag HouseRules { get; set; }

	public EGoldMode GoldMode { get; set; }

	public DLCRegistry.EDLCKey DLCEnabled { get; set; }

	public GameData()
	{
		extraData = new Dictionary<string, object>();
		HouseRules = StateShared.EHouseRulesFlag.FrosthavenRollingAttackModifiers | StateShared.EHouseRulesFlag.FrosthavenLOS | StateShared.EHouseRulesFlag.FrosthavenSummonFocus;
	}

	public T GetParam<T>(string key)
	{
		if (HasParam(key))
		{
			return (T)extraData[key];
		}
		return default(T);
	}

	public bool HasParam(string key)
	{
		return extraData.ContainsKey(key);
	}

	public void AddParam(string key, object value)
	{
		extraData[key] = value;
	}
}
