using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace Assets.Script.GUI.Quest;

public class HeadqueartQuest : IQuest
{
	private class DummyRequirementCheckResult : IRequirementCheckResult
	{
		public bool IsUnlocked()
		{
			return true;
		}

		public bool IsOnlyMissingCharacters()
		{
			return false;
		}

		public string ToString(string format)
		{
			return string.Empty;
		}
	}

	public string LocalisedNameKey => "GUI_GUILD_City";

	public string Description
	{
		get
		{
			if (Singleton<MapChoreographer>.Instance.IsChoosingLinkedQuestOption())
			{
				return LocalizationManager.GetTranslation("GUI_LINKED_QUEST_HEADQUARTERS_CHOICE");
			}
			return string.Format(LocalizationManager.GetTranslation("GUI_CITY_QUEST_DESCRIPTION"), Singleton<MapChoreographer>.Instance.CityQuestLocations.Count((MapLocation it) => it.LocationQuest.QuestState < CQuestState.EQuestState.Completed));
		}
	}

	public Sprite Icon
	{
		get
		{
			IEnumerable<CQuestState> source = from it in Singleton<MapChoreographer>.Instance.CityQuestLocations
				where it.LocationQuest.QuestState < CQuestState.EQuestState.Completed
				select it.LocationQuest;
			if (source.Any())
			{
				if (source.Any((CQuestState it) => it.Quest.IconType.In(EQuestIconType.Boss, EQuestIconType.JOTLBoss)))
				{
					return UIInfoTools.Instance.GetQuestAreaMarker(EQuestAreaType.Gloomhaven, EQuestIconType.Boss);
				}
				if (source.Any((CQuestState it) => it.Quest.IconType == EQuestIconType.None))
				{
					return UIInfoTools.Instance.GetQuestAreaMarker(EQuestAreaType.Gloomhaven, EQuestIconType.None);
				}
				if (source.Any((CQuestState it) => it.Quest.IconType.In(EQuestIconType.BossSide, EQuestIconType.JOTLBossSide)))
				{
					return UIInfoTools.Instance.GetQuestAreaMarker(EQuestAreaType.Gloomhaven, EQuestIconType.BossSide);
				}
				if (source.Any((CQuestState it) => it.Quest.IconType.In(EQuestIconType.Side, EQuestIconType.JOTLSide)))
				{
					return UIInfoTools.Instance.GetQuestAreaMarker(EQuestAreaType.Gloomhaven, EQuestIconType.Side);
				}
			}
			return UIInfoTools.Instance.GetGuildmasterModeSprite(EGuildmasterMode.City);
		}
	}

	public List<string> SpecialRules => null;

	public List<CObjective> Objectives => null;

	public List<string> AdditionalInformation => null;

	public List<Reward> Rewards => null;

	public List<string> Enemies => null;

	public QuestLocation Location => null;

	public int Treasures => 0;

	public string TreasuresTextLoc => null;

	public int Level => AdventureState.MapState.MapParty.ScenarioLevel;

	public bool IsSoloQuest => false;

	public IRequirementCheckResult CheckRequirements()
	{
		return new DummyRequirementCheckResult();
	}

	public bool IsCompleted()
	{
		return false;
	}

	protected bool Equals(HeadqueartQuest other)
	{
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != typeof(HeadqueartQuest))
		{
			return false;
		}
		return Equals((HeadqueartQuest)obj);
	}

	public override int GetHashCode()
	{
		return 0;
	}
}
