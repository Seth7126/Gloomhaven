using System;
using System.Collections.Generic;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;

namespace MapRuleLibrary.YML.Locations;

public class CMapScenario : CLocation
{
	public List<int> EventChance { get; private set; }

	public List<Tuple<string, int>> EventPool { get; private set; }

	public List<Tuple<string, int>> ScenarioPool { get; private set; }

	public float? PathDistancePercentage { get; private set; }

	public string FilePath { get; private set; }

	public string LocalisedStartKey => base.ID + "_Start_{0}";

	public string LocalisedOpenRoomKey => base.ID + "_Room_{0}_{1}";

	public string LocalisedSuccessKey => base.ID + "_Success_{0}";

	public CMapScenario(string id, string localisedName, string localisedDescription, string mesh, TileIndex mapLocation, CUnlockCondition unlockCondition, string fileName, List<int> eventChance, List<Tuple<string, int>> eventPool, List<Tuple<string, int>> scenarioPool)
		: base(id, localisedName, localisedDescription, mesh, mapLocation, unlockCondition, fileName)
	{
		EventChance = eventChance;
		EventPool = eventPool;
		ScenarioPool = scenarioPool;
	}

	public CMapScenario(string id, string localisedName, string localisedDescription, string mesh, float? pathDistancePercentage, CUnlockCondition unlockCondition, string fileName, List<int> eventChance, List<Tuple<string, int>> eventPool, List<Tuple<string, int>> scenarioPool)
		: base(id, localisedName, localisedDescription, mesh, new TileIndex(200, 200), unlockCondition, fileName)
	{
		EventChance = eventChance;
		EventPool = eventPool;
		ScenarioPool = scenarioPool;
		PathDistancePercentage = pathDistancePercentage;
	}

	public string RollForScenario()
	{
		return MapYMLShared.RollTupleList(ScenarioPool);
	}
}
