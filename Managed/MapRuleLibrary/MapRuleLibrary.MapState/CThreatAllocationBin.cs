using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace MapRuleLibrary.MapState;

public class CThreatAllocationBin
{
	public MonsterSpawnRates MonsterEntry;

	public MonsterThreatValuesEntry ThreatValues;

	public int MinThreshold;

	public int MaxThreshold;

	public int EliteChance;

	private List<bool> Monsters = new List<bool>();

	public int TotalThreat { get; private set; }

	public CThreatAllocationBin(MonsterSpawnRates monsterEntry, MonsterThreatValuesEntry threatValues, float roomThreat, int baseEliteChance, int partySize = 2)
	{
		MonsterEntry = monsterEntry;
		ThreatValues = threatValues;
		MinThreshold = (int)((float)monsterEntry.Min * roomThreat) / 100;
		MaxThreshold = (int)((float)monsterEntry.Max * roomThreat) / 100;
		EliteChance = ((MonsterEntry.EliteChance.Length == 0) ? baseEliteChance : MonsterEntry.EliteChance[partySize]);
	}

	public void AddMonster(bool isElite)
	{
		Monsters.Add(isElite);
		TotalThreat += (isElite ? ThreatValues.Elite : ThreatValues.Normal);
	}

	public int GetMonsterCount(bool elite)
	{
		return Monsters.Count((bool x) => x == elite);
	}

	public override string ToString()
	{
		List<int> list = new List<int>();
		foreach (bool monster in Monsters)
		{
			list.Add(monster ? ThreatValues.Elite : ThreatValues.Normal);
		}
		return MonsterEntry.SpawnRatesID + ", min=" + MinThreshold + ": " + string.Join(",", list);
	}
}
