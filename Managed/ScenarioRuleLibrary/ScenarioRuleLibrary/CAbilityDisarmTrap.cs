using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;
using UnityEngine;

namespace ScenarioRuleLibrary;

public class CAbilityDisarmTrap : CAbility
{
	public enum DisarmTrapState
	{
		SelectTrapPosition,
		DisarmTrap,
		SelectActorToTargetWithTrapEffects,
		AffectingActorWithTrapEffects,
		DisarmTrapDone
	}

	private DisarmTrapState m_State;

	private Dictionary<string, int> m_disarmedTrapsDictionary;

	private CObjectTrap m_lastTrapDisarmed;

	public bool TargetActorWithTrapEffects { get; set; }

	public int TargetActorWithTrapEffectRange { get; set; }

	public CAbilityDisarmTrap(bool targetActorWithTrapEffects, int targetActorWithTrapEffectRange)
	{
		TargetActorWithTrapEffects = targetActorWithTrapEffects;
		TargetActorWithTrapEffectRange = targetActorWithTrapEffectRange;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = DisarmTrapState.SelectTrapPosition;
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
			{
				if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						base.TilesInRange.Add(cTile);
					}
				}
			}
		}
		else if (base.AreaEffect == null)
		{
			base.TilesInRange = GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			if (m_Range > 1)
			{
				base.TilesInRange.Add(ScenarioManager.GetAdjacentTile(base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter));
			}
		}
		if (m_NumberTargets == -1 || base.Targeting == EAbilityTargeting.All || base.Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		m_disarmedTrapsDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0075;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0075;
				}
			}
		}
		if (m_CancelAbility)
		{
			PhaseManager.NextStep();
			return true;
		}
		switch (m_State)
		{
		case DisarmTrapState.SelectTrapPosition:
			if (base.UseSubAbilityTargeting)
			{
				if (base.IsInlineSubAbility && base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
					{
						TileSelected(inlineSubAbilityTile, null);
					}
					break;
				}
				if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					foreach (CTile item in base.ParentAbility.TilesSelected)
					{
						TileSelected(item, null);
					}
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
					m_TilesSelected.Clear();
					foreach (CTile item2 in base.TilesInRange)
					{
						if (item2.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap { Activated: false } && !m_TilesSelected.Contains(item2))
						{
							m_TilesSelected.Add(item2);
						}
					}
					if (m_TilesSelected.Count <= 0)
					{
						m_CancelAbility = true;
						if (base.IsMergedAbility)
						{
							PhaseManager.StepComplete();
						}
						else
						{
							PhaseManager.NextStep();
						}
					}
				}
				if (!m_CancelAbility)
				{
					CPlayerSelectingObjectPosition_MessageData message4 = new CPlayerSelectingObjectPosition_MessageData(base.TargetingActor)
					{
						m_SpawnType = ScenarioManager.ObjectImportType.None,
						m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Trap },
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message4);
				}
				break;
			}
			foreach (CTile item3 in base.TilesInRange)
			{
				if (item3.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap { Activated: false })
				{
					if (!m_TilesSelected.Contains(item3))
					{
						m_TilesSelected.Add(item3);
					}
					m_State = DisarmTrapState.DisarmTrap;
					Perform();
					break;
				}
			}
			break;
		case DisarmTrapState.DisarmTrap:
		{
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count <= 0)
			{
				break;
			}
			List<CObjectTrap> list = new List<CObjectTrap>();
			foreach (CTile item4 in m_TilesSelected)
			{
				if (!(item4.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap { Activated: false } cObjectTrap))
				{
					continue;
				}
				base.AbilityHasHappened = true;
				list.Add(cObjectTrap);
				cObjectTrap.DestroyProp(0f, sendMessageToClient: false);
				string characterID = "";
				if (base.TargetingActor != null)
				{
					if (base.TargetingActor is CPlayerActor cPlayerActor)
					{
						characterID = cPlayerActor.CharacterClass.ID;
					}
					if (base.TargetingActor is CHeroSummonActor cHeroSummonActor)
					{
						characterID = cHeroSummonActor.Summoner?.CharacterClass.ID;
					}
					SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropTrap(0, characterID, "Disarm", ESESubTypeObjectProp.Activated, cObjectTrap.ObjectType, cObjectTrap.PrefabName, cObjectTrap.Owner?.ActorGuid));
				}
				m_disarmedTrapsDictionary.TryGetValue(cObjectTrap.PrefabName, out var value);
				m_disarmedTrapsDictionary[cObjectTrap.PrefabName] = value + 1;
				if (TargetActorWithTrapEffects)
				{
					m_lastTrapDisarmed = cObjectTrap;
				}
			}
			CDisarmTrap_MessageData message3 = new CDisarmTrap_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_ActorDisarmingTrap = base.TargetingActor,
				m_Tiles = m_TilesSelected.ToList(),
				m_DisarmedTraps = list.ToList(),
				m_DisarmTrapAbility = this
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case DisarmTrapState.SelectActorToTargetWithTrapEffects:
			m_TilesSelected.Clear();
			if (TargetActorWithTrapEffects && base.TargetingActor.Type == CActor.EType.Player)
			{
				m_Range = TargetActorWithTrapEffectRange;
				base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, TargetActorWithTrapEffectRange, m_ActorsToIgnore, base.AbilityFilter, null, base.AllPossibleTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
				m_ActorsToTarget.Clear();
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = false;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			else
			{
				m_State = DisarmTrapState.AffectingActorWithTrapEffects;
				Perform();
			}
			break;
		case DisarmTrapState.AffectingActorWithTrapEffects:
			if (TargetActorWithTrapEffects && base.TargetingActor.Type == CActor.EType.Player)
			{
				foreach (CActor item5 in m_ActorsToTarget)
				{
					if (!ScenarioManager.Scenario.HasActor(item5))
					{
						continue;
					}
					if (GameState.ActorHealthCheck(item5, item5) && m_lastTrapDisarmed.Conditions.Count > 0)
					{
						foreach (CCondition.ENegativeCondition condition in m_lastTrapDisarmed.Conditions)
						{
							EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((EAbilityType s) => s.ToString() == condition.ToString());
							if (condition != CCondition.ENegativeCondition.NA && eAbilityType != EAbilityType.None)
							{
								CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
								cAbility.Start(base.TargetingActor, base.TargetingActor);
								((CAbilityTargeting)cAbility).ApplyToActor(item5);
								if (eAbilityType == EAbilityType.Stun || eAbilityType == EAbilityType.Immobilize || eAbilityType == EAbilityType.Sleep)
								{
									CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
									ScenarioRuleClient.MessageHandler(message);
								}
							}
							else
							{
								DLLDebug.LogError("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
							}
						}
					}
					int health = item5.Health;
					bool actorWasAsleep = item5.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					if (m_lastTrapDisarmed.Damage)
					{
						GameState.ActorBeenDamaged(item5, m_lastTrapDisarmed.GetTrapDamage(), checkIfPlayerCanAvoidDamage: true, base.TargetingActor, null, EAbilityType.DisarmTrap, 0, isTrapDamage: true);
					}
					if ((item5.Type != CActor.EType.Player || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None || !m_lastTrapDisarmed.Damage) && GameState.ActorHealthCheck(item5, item5, isTrap: false, isTerrain: false, actorWasAsleep))
					{
						CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(item5)
						{
							m_ActorBeingDamaged = item5,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
			}
			if (base.IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message5 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
			else
			{
				CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.AllTargets && !base.TilesSelected.Contains(selectedTile) && base.TilesSelected.Count < m_NumberTargets)
		{
			if (m_State == DisarmTrapState.SelectTrapPosition)
			{
				if (base.TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap { Activated: false })
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
					}
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			else if (TargetActorWithTrapEffects && m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
				if (m_ValidActorsInRange.Contains(actorOnTile))
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						base.TilesSelected.Add(selectedTile);
					}
					m_ActorsToTarget.Add(actorOnTile);
					m_NumberTargetsRemaining = Mathf.Max(0, --m_NumberTargetsRemaining);
					CActorSelectedAttackFocus_MessageData message2 = new CActorSelectedAttackFocus_MessageData(base.TargetingActor)
					{
						m_AttackingActor = base.TargetingActor,
						m_Ability = this,
						m_AttackFocus = actorOnTile,
						m_Adding = false
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.AllTargets && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			if (m_State == DisarmTrapState.SelectTrapPosition)
			{
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(base.TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else if (TargetActorWithTrapEffects && m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
				m_ActorsToTarget.Remove(actorOnTile);
				m_NumberTargetsRemaining = Mathf.Min(m_NumberTargets, ++m_NumberTargetsRemaining);
			}
		}
		if (false)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == DisarmTrapState.SelectTrapPosition || m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != DisarmTrapState.SelectTrapPosition)
		{
			return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != DisarmTrapState.SelectTrapPosition)
			{
				return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
			}
			return true;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == DisarmTrapState.DisarmTrapDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		bool isSummon = false;
		CActor targetingActor = base.TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == base.TargetingActor.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDisarmTrap(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_disarmedTrapsDictionary, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override string GetDescription()
	{
		return "DisarmTrap";
	}

	public bool HasPassedState(DisarmTrapState disarmState)
	{
		return m_State > disarmState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public CAbilityDisarmTrap()
	{
	}

	public CAbilityDisarmTrap(CAbilityDisarmTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		TargetActorWithTrapEffects = state.TargetActorWithTrapEffects;
		TargetActorWithTrapEffectRange = state.TargetActorWithTrapEffectRange;
		m_State = state.m_State;
		m_disarmedTrapsDictionary = references.Get(state.m_disarmedTrapsDictionary);
		if (m_disarmedTrapsDictionary == null && state.m_disarmedTrapsDictionary != null)
		{
			m_disarmedTrapsDictionary = new Dictionary<string, int>(state.m_disarmedTrapsDictionary.Comparer);
			foreach (KeyValuePair<string, int> item in state.m_disarmedTrapsDictionary)
			{
				string key = item.Key;
				int value = item.Value;
				m_disarmedTrapsDictionary.Add(key, value);
			}
			references.Add(state.m_disarmedTrapsDictionary, m_disarmedTrapsDictionary);
		}
		m_lastTrapDisarmed = references.Get(state.m_lastTrapDisarmed);
		if (m_lastTrapDisarmed == null && state.m_lastTrapDisarmed != null)
		{
			m_lastTrapDisarmed = new CObjectTrap(state.m_lastTrapDisarmed, references);
			references.Add(state.m_lastTrapDisarmed, m_lastTrapDisarmed);
		}
	}
}
