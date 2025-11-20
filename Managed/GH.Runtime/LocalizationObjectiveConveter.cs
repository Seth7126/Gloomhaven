using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public static class LocalizationObjectiveConveter
{
	public static string LocalizeText(this CObjective m_Objective, bool initialState = true)
	{
		if (m_Objective.LocKey != string.Empty)
		{
			int num = ((SaveData.Instance.Global.CurrentGameState == EGameState.Map) ? Mathf.Max(AdventureState.MapState.MapParty.SelectedCharacters.Count(), 1) : Mathf.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1));
			string text = LocalizationManager.GetTranslation(m_Objective.LocKey);
			switch (m_Objective.ObjectiveType)
			{
			case EObjectiveType.ReachRound:
			{
				int num2 = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? ((m_Objective as CObjective_ReachRound).ReachRoundNumber - (ScenarioManager.CurrentScenarioState.RoundNumber - 1)) : (m_Objective as CObjective_ReachRound).ReachRoundNumber);
				text = string.Format(text, num2);
				break;
			}
			case EObjectiveType.XCharactersDie:
			{
				CObjective_XCharactersDie cObjective_XCharactersDie = m_Objective as CObjective_XCharactersDie;
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XCharactersDie.KillAmount[num - 1] - cObjective_XCharactersDie.CurrentKills)) : string.Format(text, cObjective_XCharactersDie.KillAmount[num - 1]));
					break;
				}
				string actorStringsFromFilter2 = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XCharactersDie.KillAmount[num - 1] - cObjective_XCharactersDie.CurrentKills), actorStringsFromFilter2) : string.Format(text, cObjective_XCharactersDie.KillAmount[num - 1], actorStringsFromFilter2));
				break;
			}
			case EObjectiveType.LootX:
			{
				CObjective_LootX cObjective_LootX = m_Objective as CObjective_LootX;
				if (m_Objective.ObjectiveFilter != null && m_Objective.ObjectiveFilter.FilterHasValues && m_Objective.ObjectiveFilter.FilterLootType != CAbilityFilter.ELootType.None)
				{
					string translation2 = LocalizationManager.GetTranslation(m_Objective.ObjectiveFilter.FilterLootType.ToString());
					text = string.Format(text, cObjective_LootX.LootAmount[num - 1], translation2);
				}
				else
				{
					text = string.Format(text, cObjective_LootX.LootAmount[num - 1]);
				}
				break;
			}
			case EObjectiveType.DestroyXObjects:
			{
				CObjective_DestroyXObjects cObjective_DestroyXObjects = m_Objective as CObjective_DestroyXObjects;
				if (m_Objective.ObjectiveFilter != null && m_Objective.ObjectiveFilter.FilterHasValues)
				{
					string text2 = "";
					if (m_Objective.ObjectiveFilter.FilterObjectClassIDs != null)
					{
						for (int i = 0; i < m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count; i++)
						{
							string text3 = m_Objective.ObjectiveFilter.FilterObjectClassIDs[i];
							MonsterYMLData monsterData = ScenarioRuleClient.SRLYML.GetMonsterData(text3);
							if (monsterData != null)
							{
								string translation = LocalizationManager.GetTranslation(monsterData.LocKey);
								text2 += translation;
								if (m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count > 1 && i < m_Objective.ObjectiveFilter.FilterObjectClassIDs.Count - 1)
								{
									text2 += ", ";
								}
							}
							else
							{
								Debug.LogError("Unable to find Object class " + text3);
							}
						}
					}
					text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DestroyXObjects.DestroyAmount[num - 1] - cObjective_DestroyXObjects.CurrentObjectsDestroyed), text2) : string.Format(text, cObjective_DestroyXObjects.DestroyAmount[num - 1], text2));
				}
				else
				{
					text = string.Format(text, cObjective_DestroyXObjects.DestroyAmount[num - 1]);
				}
				break;
			}
			case EObjectiveType.ActorReachPosition:
			{
				CObjective_ActorReachPosition cObjective_ActorReachPosition = m_Objective as CObjective_ActorReachPosition;
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, cObjective_ActorReachPosition.ReachPositionAmounts[num - 1] - cObjective_ActorReachPosition.CurrentReachedPosition) : string.Format(text, cObjective_ActorReachPosition.ReachPositionAmounts[num - 1]));
				break;
			}
			case EObjectiveType.AnyActorReachPosition:
			{
				CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition = m_Objective as CObjective_AnyActorReachPosition;
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, cObjective_AnyActorReachPosition.ReachPositionAmounts[num - 1] - cObjective_AnyActorReachPosition.CurrentReachedPosition) : string.Format(text, cObjective_AnyActorReachPosition.ReachPositionAmounts[num - 1]));
				break;
			}
			case EObjectiveType.ActivateXPressurePlates:
			{
				CObjective_ActivatePressurePlateX cObjective_ActivatePressurePlateX = m_Objective as CObjective_ActivatePressurePlateX;
				text = string.Format(text, cObjective_ActivatePressurePlateX.NumberToActivate[num - 1]);
				break;
			}
			case EObjectiveType.DeactivateXSpawners:
			{
				CObjective_DeactivateXSpawners cObjective_DeactivateXSpawners = m_Objective as CObjective_DeactivateXSpawners;
				text = string.Format(text, cObjective_DeactivateXSpawners.NumberToDeactivate[num - 1]);
				break;
			}
			case EObjectiveType.ActivateXSpawners:
			{
				CObjective_ActivateXSpawners cObjective_ActivateXSpawners = m_Objective as CObjective_ActivateXSpawners;
				text = string.Format(text, cObjective_ActivateXSpawners.NumberToActivate[num - 1]);
				break;
			}
			case EObjectiveType.ActorsEscaped:
			{
				CObjective_ActorsEscaped cObjective_ActorsEscaped = m_Objective as CObjective_ActorsEscaped;
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, cObjective_ActorsEscaped.TargetNumberOfEscapees[num - 1] - cObjective_ActorsEscaped.CurrentEscapedAmount) : string.Format(text, cObjective_ActorsEscaped.TargetNumberOfEscapees[num - 1]));
				break;
			}
			case EObjectiveType.DealXDamage:
			{
				CObjective_DealXDamage cObjective_DealXDamage = m_Objective as CObjective_DealXDamage;
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DealXDamage.TargetDamage[num - 1] - cObjective_DealXDamage.CurrentDamageDealt)) : string.Format(text, cObjective_DealXDamage.TargetDamage[num - 1]));
					break;
				}
				string actorStringsFromFilter3 = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_DealXDamage.TargetDamage[num - 1] - cObjective_DealXDamage.CurrentDamageDealt), actorStringsFromFilter3) : string.Format(text, cObjective_DealXDamage.TargetDamage[num - 1], actorStringsFromFilter3));
				break;
			}
			case EObjectiveType.XActorsHealToMax:
			{
				CObjective_XActorsHealToMax cObjective_XActorsHealToMax = m_Objective as CObjective_XActorsHealToMax;
				if (m_Objective.ObjectiveFilter == null || !m_Objective.ObjectiveFilter.FilterHasValues)
				{
					text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XActorsHealToMax.ActorAmount[num - 1] - cObjective_XActorsHealToMax.CurrentActorsFullHealth)) : string.Format(text, cObjective_XActorsHealToMax.ActorAmount[num - 1]));
					break;
				}
				string actorStringsFromFilter = GetActorStringsFromFilter(m_Objective.ObjectiveFilter);
				text = ((!(SaveData.Instance.Global.CurrentGameState == EGameState.Map || initialState)) ? string.Format(text, Mathf.Max(0, cObjective_XActorsHealToMax.ActorAmount[num - 1] - cObjective_XActorsHealToMax.CurrentActorsFullHealth), actorStringsFromFilter) : string.Format(text, cObjective_XActorsHealToMax.ActorAmount[num - 1], actorStringsFromFilter));
				break;
			}
			}
			return text;
		}
		return m_Objective.LocKey;
	}

	public static string GetActorStringsFromFilter(CObjectiveFilter filter)
	{
		string text = "";
		if (filter.FilterActorGUIDs == null || filter.FilterActorGUIDs.Count <= 0)
		{
			if ((filter.FilterPlayerClassIDs != null && filter.FilterPlayerClassIDs.Count > 0) || (filter.FilterEnemyClassIDs != null && filter.FilterEnemyClassIDs.Count > 0) || (filter.FilterHeroSummonClassIDs != null && filter.FilterHeroSummonClassIDs.Count > 0))
			{
				if (filter.FilterPlayerClassIDs != null)
				{
					foreach (string playerClass in filter.FilterPlayerClassIDs)
					{
						CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == playerClass);
						if (characterYMLData != null)
						{
							string translation = LocalizationManager.GetTranslation(characterYMLData.LocKey);
							text += translation;
						}
						else
						{
							Debug.LogError("Unable to find player class " + playerClass);
						}
					}
				}
				if (filter.FilterEnemyClassIDs != null)
				{
					for (int num = 0; num < filter.FilterEnemyClassIDs.Count; num++)
					{
						string text2 = filter.FilterEnemyClassIDs[num];
						MonsterYMLData monsterData = ScenarioRuleClient.SRLYML.GetMonsterData(text2);
						if (monsterData != null)
						{
							string translation2 = LocalizationManager.GetTranslation(monsterData.LocKey);
							text += translation2;
							if (filter.FilterEnemyClassIDs.Count > 1 && num < filter.FilterEnemyClassIDs.Count - 1)
							{
								text += ", ";
							}
						}
						else
						{
							Debug.LogError("Unable to find monster class " + text2);
						}
					}
				}
				if (filter.FilterHeroSummonClassIDs != null)
				{
					foreach (string summonClass in filter.FilterHeroSummonClassIDs)
					{
						HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == summonClass);
						if (heroSummonYMLData != null)
						{
							string translation3 = LocalizationManager.GetTranslation(heroSummonYMLData.LocKey);
							text += translation3;
						}
						else
						{
							Debug.LogError("Unable to find summon class " + summonClass);
						}
					}
				}
			}
			else if (filter.FilterEnemyType != CAbilityFilter.EFilterEnemy.None)
			{
				string translation4 = LocalizationManager.GetTranslation(filter.FilterEnemyType.ToString());
				text += translation4;
			}
			else if (filter.FilterActorType != CAbilityFilter.EFilterActorType.None)
			{
				string translation5 = LocalizationManager.GetTranslation(filter.ActorLocKey.IsNullOrEmpty() ? ("GUI_" + filter.FilterActorType) : filter.ActorLocKey);
				text += translation5;
			}
		}
		return text;
	}
}
