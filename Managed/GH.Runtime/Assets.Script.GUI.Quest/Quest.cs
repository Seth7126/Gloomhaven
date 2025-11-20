using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.PhaseManager;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace Assets.Script.GUI.Quest;

internal class Quest : IQuest
{
	private CQuestState questState;

	private bool inLoadout;

	public string LocalisedNameKey => questState.Quest.LocalisedNameKey;

	public string Description => LocalizationManager.GetTranslation(questState.Quest.LocalisedDescriptionKey);

	public Sprite Icon => UIInfoTools.Instance.GetQuestMarkerSprite(questState);

	public List<string> SpecialRules
	{
		get
		{
			if (ScenarioState.NonSerializedInitialState == null)
			{
				ScenarioState.CheckForNonSerializedInitialScenario();
			}
			ScenarioState scenario = ScenarioState.NonSerializedInitialState;
			List<string> list = (from it in scenario.LoseObjectives
				where !it.IsHidden && it.IsActive
				select it.LocalizeText()).Concat(from it in scenario.ScenarioModifiers
				where !it.IsHidden && CMapScenarioState.ShouldBeActivated(it)
				select it.LocalizeText(scenario.ID) into it
				where it.IsNOTNullOrEmpty()
				select it).ToList();
			if (Singleton<MapChoreographer>.Instance.IsLinkedQuest(questState) && !inLoadout)
			{
				list.Add("<color=#96c990>" + LocalizationManager.GetTranslation("GUI_LINKED_QUEST_LINKEDQUEST_CHOICE") + "</color>");
			}
			return list;
		}
	}

	private CMapScenarioState ScenarioState => questState.ScenarioState;

	public List<CObjective> Objectives
	{
		get
		{
			if (AdventureState.MapState.HeadquartersState.Headquarters.TutorialQuestNames.Contains(questState.ID))
			{
				return null;
			}
			return ScenarioState.NonSerializedInitialState.WinObjectives.Where((CObjective o) => !o.IsHidden && o.IsActive).ToList();
		}
	}

	public List<string> AdditionalInformation => GenerateMultiplayerInfo(questState);

	public List<Reward> Rewards
	{
		get
		{
			if (!AdventureState.MapState.IsCampaign)
			{
				return questState.QuestCompletionRewardGroup.Rewards?.FindAll((Reward it) => it.IsVisibleInUI());
			}
			if (questState.QuestCompletionRewardGroup.Rewards != null && questState.QuestCompletionRewardGroup.Rewards.Exists((Reward it) => it.IsVisibleInUI()))
			{
				List<Reward> list = new List<Reward>();
				if (!IsCompleted())
				{
					list.Add(new Reward(ETreasureType.None, (Dictionary<string, int>)null, ETreasureDistributionType.None));
				}
				return list;
			}
			return null;
		}
	}

	public int Treasures
	{
		get
		{
			if (!questState.Quest.HideTreasureWhenCompleted)
			{
				return questState.CountTreasures();
			}
			return 0;
		}
	}

	public string TreasuresTextLoc
	{
		get
		{
			if (!questState.Quest.LocalisedCustomTreasureRewardKey.IsNullOrEmpty())
			{
				return questState.Quest.LocalisedCustomTreasureRewardKey;
			}
			return "GUI_QUEST_TREASURES_REWARD";
		}
	}

	public int Level => questState.ScenarioLevelToUse;

	public bool IsSoloQuest => questState.IsSoloScenario;

	public List<string> Enemies
	{
		get
		{
			if (ScenarioState.CachedMonsterClasses == null || ScenarioState.CachedMonsterClasses.Count == 0)
			{
				return (from it in ScenarioState.NonSerializedInitialState.Monsters.Select((EnemyState it) => it.ClassID).Distinct()
					orderby LocalizationManager.GetTranslation(MonsterClassManager.Find(it).LocKey)
					select it).ToList();
			}
			return ScenarioState.CachedMonsterClasses.OrderBy((string it) => LocalizationManager.GetTranslation(MonsterClassManager.Find(it).LocKey)).ToList();
		}
	}

	public QuestLocation Location
	{
		get
		{
			if (!AdventureState.MapState.IsCampaign || questState.Quest.LocationArea == EQuestAreaType.None)
			{
				return null;
			}
			ECharacter requiredCharacter = ECharacter.None;
			if (questState.Quest.IconType == EQuestIconType.RequiredCharacter && questState.IsSoloScenario)
			{
				if (!questState.SoloScenarioCharacterID.IsNullOrEmpty())
				{
					requiredCharacter = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == questState.SoloScenarioCharacterID).CharacterModel;
				}
				else
				{
					List<QuestYML.CQuestCharacterRequirement> requiredCharacters = questState.Quest.QuestCharacterRequirements.FindAll((QuestYML.CQuestCharacterRequirement it) => it.RequiredCharacterID.IsNOTNullOrEmpty() && it.RequiredCharacterCount == 1);
					if (requiredCharacters.Count == 1)
					{
						requiredCharacter = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == requiredCharacters[0].RequiredCharacterID).CharacterModel;
					}
				}
			}
			return new QuestLocation(questState.Quest.LocationArea, questState.Quest.IconType, requiredCharacter);
		}
	}

	public Quest(CQuestState questState)
	{
		this.questState = questState;
		inLoadout = AdventureState.MapState.CurrentMapPhaseType == EMapPhaseType.AtScenario;
		ScenarioState.CheckForNonSerializedInitialScenario();
	}

	private List<string> GenerateMultiplayerInfo(CQuestState quest)
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsClient && Singleton<UIMapMultiplayerController>.Instance.HostSelectedQuest == quest)
		{
			return new List<string> { LocalizationManager.GetTranslation("GUI_QUEST_SELECTED_BY_HOST") };
		}
		return null;
	}

	public IRequirementCheckResult CheckRequirements()
	{
		return questState.CheckRequirements();
	}

	public bool IsCompleted()
	{
		if (questState.QuestState >= CQuestState.EQuestState.Completed)
		{
			return questState.QuestState != CQuestState.EQuestState.Blocked;
		}
		return false;
	}

	protected bool Equals(Quest other)
	{
		if (object.Equals(questState, other.questState))
		{
			return inLoadout == other.inLoadout;
		}
		return false;
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
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Quest)obj);
	}

	public override int GetHashCode()
	{
		return (((questState != null) ? questState.GetHashCode() : 0) * 397) ^ inLoadout.GetHashCode();
	}

	public static bool operator ==(Quest left, Quest right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(Quest left, Quest right)
	{
		return !object.Equals(left, right);
	}
}
