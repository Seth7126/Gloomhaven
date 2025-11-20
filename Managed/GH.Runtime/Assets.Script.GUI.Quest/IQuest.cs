using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace Assets.Script.GUI.Quest;

public interface IQuest
{
	string LocalisedNameKey { get; }

	string Description { get; }

	QuestLocation Location { get; }

	Sprite Icon { get; }

	List<string> SpecialRules { get; }

	List<CObjective> Objectives { get; }

	List<string> AdditionalInformation { get; }

	List<Reward> Rewards { get; }

	List<string> Enemies { get; }

	int Treasures { get; }

	string TreasuresTextLoc { get; }

	int Level { get; }

	bool IsSoloQuest { get; }

	IRequirementCheckResult CheckRequirements();

	bool IsCompleted();
}
