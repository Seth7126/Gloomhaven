using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

public class ScenarioLevelTable
{
	public string Name;

	public List<ScenarioLevelTableEntry> Entries;

	public ScenarioLevelTable(string name)
	{
		Name = name;
		Entries = new List<ScenarioLevelTableEntry>();
	}
}
