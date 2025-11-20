using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityDamage : CAbility
{
	public enum EDamageState
	{
		PreDamageBuffTargeting,
		ApplyDamageBuff,
		SelectDamageFocus,
		ActorHasDamaged,
		SelectActorToDamage,
		ActorBeenDamaged,
		NextActorCheck,
		InlineSubAbilityCheck,
		DamageDone
	}

	private EDamageState m_State;

	private CActor m_ActorBeingDamaged;

	private CAttackSummary m_DamageSummary;

	public CAttackSummary DamageSummary => m_DamageSummary;

	public CAbilityDamage()
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		bool flag = false;
		if (base.ParentAbility?.ActorsTargeted != null && base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter) && base.IsInlineSubAbility && (base.InlineSubAbilityTiles == null || base.InlineSubAbilityTiles.Count == 0))
		{
			base.InlineSubAbilityTiles = new List<CTile>();
			foreach (CActor item in base.ParentAbility.ActorsTargeted.ToList())
			{
				base.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item.ArrayIndex.X, item.ArrayIndex.Y]);
			}
		}
		if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = EDamageState.PreDamageBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(base.TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(base.TargetingActor);
		}
		else if (base.UseSubAbilityTargeting || base.TargetingActor.Type != CActor.EType.Player || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value))
		{
			m_State = EDamageState.ActorHasDamaged;
			if (base.UseSubAbilityTargeting)
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.UseParentTargets == true)
				{
					if (base.ParentAbility?.ActorsTargeted != null)
					{
						if (base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter))
						{
							base.ValidActorsInRange = base.ParentAbility.ActorsTargeted.ToList();
						}
						else
						{
							base.ValidActorsInRange = new List<CActor>();
							foreach (CActor item2 in base.ParentAbility.ActorsTargeted)
							{
								if (base.AbilityFilter.IsValidTarget(item2, filterActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
								{
									base.ValidActorsInRange.Add(item2);
								}
							}
						}
					}
					RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					m_NumberTargets = base.ValidActorsInRange.Count;
					flag = true;
				}
			}
		}
		else
		{
			m_State = EDamageState.SelectDamageFocus;
		}
		m_ActorBeingDamaged = null;
		if (base.AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(targetActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		}
		if (m_NumberTargets == -1 || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_AllTargets = true;
			if (base.UseSubAbilityTargeting || base.IsInlineSubAbility)
			{
				m_ValidActorsInRange = new List<CActor>();
				base.TilesInRange.Clear();
				if (base.IsInlineSubAbility)
				{
					if (base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
					{
						AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
						if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.HasValue)
						{
							AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
							if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.Value)
							{
								foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
								{
									CActor cActor = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
									if (cActor != null)
									{
										m_ActorsToIgnore.Add(cActor);
									}
									base.TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
									m_ValidActorsInRange.AddRange(GameState.GetActorsInRange(inlineSubAbilityTile.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible));
								}
								base.TilesInRange = base.TilesInRange.Distinct().ToList();
								m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
								goto IL_0964;
							}
						}
						base.TilesInRange = base.InlineSubAbilityTiles.ToList();
						foreach (CTile inlineSubAbilityTile2 in base.InlineSubAbilityTiles)
						{
							CActor cActor2 = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile2.m_ArrayIndex);
							if (cActor2 != null && !m_ValidActorsInRange.Contains(cActor2) && base.AbilityFilter.IsValidTarget(cActor2, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false))
							{
								m_ValidActorsInRange.Add(cActor2);
							}
						}
					}
					goto IL_0964;
				}
				AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
					if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentMovePath.Value && base.ParentAbility is CAbilityMove)
					{
						List<CTile> list = new List<CTile>();
						for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
						{
							if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
							{
								Point point = CAbilityMove.AllArrayIndexOnPath[i];
								CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
								list.Add(cTile);
								if (cTile != null)
								{
									base.TilesInRange.AddRange(GameState.GetTilesInRange(cTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
								}
							}
						}
						base.TilesInRange = base.TilesInRange.Distinct().ToList();
						foreach (CTile item3 in base.TilesInRange)
						{
							if (item3 != null)
							{
								CActor actorOnTile = GameState.GetActorOnTile(item3, base.TargetingActor, base.AbilityFilter, m_ActorsToIgnore, base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
								if (actorOnTile != null && !m_ValidActorsInRange.Contains(actorOnTile))
								{
									m_ValidActorsInRange.Add(actorOnTile);
								}
							}
						}
						goto IL_0964;
					}
				}
				if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					if (base.ParentAbility.AreaEffect != null)
					{
						base.TilesInRange.AddRange(base.ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
					}
					else
					{
						base.TilesInRange.AddRange(base.ParentAbility.TilesSelected);
					}
					AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
					if (miscAbilityData6 != null && miscAbilityData6.UseParentTiles.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
						if (miscAbilityData7 != null && miscAbilityData7.UseParentTiles.Value)
						{
							foreach (CTile item4 in base.TilesInRange)
							{
								CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item4.m_ArrayIndex);
								if (cActor3 != null && base.AbilityFilter.IsValidTarget(cActor3, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
								{
									m_ValidActorsInRange.Add(cActor3);
								}
							}
							goto IL_094e;
						}
					}
					foreach (CTile item5 in base.TilesInRange.ToList())
					{
						CActor cActor4 = ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndex);
						m_ActorsToIgnore.Clear();
						if (cActor4 != null)
						{
							m_ActorsToIgnore.Add(cActor4);
						}
						base.TilesInRange.AddRange(GameState.GetTilesInRange(item5.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
						m_ValidActorsInRange.AddRange(GameState.GetActorsInRange(item5.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible));
					}
				}
				goto IL_094e;
			}
			SharedAbilityTargeting.GetValidActorsInRange(this);
			m_NumberTargets = m_ValidActorsInRange.Count;
			m_ActorsToTarget = m_ValidActorsInRange.ToList();
		}
		else if (!flag)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		goto IL_09c7;
		IL_094e:
		m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
		goto IL_0964;
		IL_0964:
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_NumberTargets = m_ValidActorsInRange.Count;
		m_ActorsToTarget = m_ValidActorsInRange.ToList();
		goto IL_09c7;
		IL_09c7:
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.TargetingActor.Type == CActor.EType.Player && !IsPositive() && (base.MiscAbilityData == null || !base.MiscAbilityData.NegativeAbilityIsOptional.HasValue || !base.MiscAbilityData.NegativeAbilityIsOptional.Value) && m_ValidActorsInRange.Count > 0 && (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion)))
		{
			foreach (CActor item6 in m_ValidActorsInRange)
			{
				if (CActor.AreActorsAllied(base.TargetingActor.Type, item6.Type) || item6 == base.TargetingActor)
				{
					m_CanSkip = false;
					break;
				}
			}
		}
		if (base.Strength <= 0)
		{
			m_CancelAbility = true;
		}
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
		case EDamageState.PreDamageBuffTargeting:
		{
			if (base.TargetingActor.Type != CActor.EType.Player || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value))
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EDamageState.ApplyDamageBuff:
		{
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			CDamageBuff_MessageData message = new CDamageBuff_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_DamageAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.NextStep();
			return true;
		}
		case EDamageState.SelectDamageFocus:
		{
			if (base.UseSubAbilityTargeting && m_ValidActorsInRange.Count == 0 && base.AreaEffect == null)
			{
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				PhaseManager.NextStep();
				return true;
			}
			InitDamageSummary();
			if (m_DamageSummary != null)
			{
				m_DamageSummary.UpdateTargetData(this, m_ActiveSingleTargetItems, m_ActiveSingleTargetActiveBonuses);
			}
			CActorIsSelectingDamageFocusTargets cActorIsSelectingDamageFocusTargets = new CActorIsSelectingDamageFocusTargets(base.AnimOverload, base.TargetingActor);
			cActorIsSelectingDamageFocusTargets.m_DamageSelf = base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true);
			cActorIsSelectingDamageFocusTargets.m_DamageAbility = this;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingDamageFocusTargets);
			break;
		}
		case EDamageState.ActorHasDamaged:
			m_CanUndo = false;
			if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.UseParentTiles.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.UseParentTiles.Value)
					{
						m_ActorsToTarget = m_ValidActorsInRange;
						goto IL_03a0;
					}
				}
			}
			if (base.AreaEffect != null)
			{
				m_ActorsToTarget = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
			}
			else if ((base.Targeting == EAbilityTargeting.All || base.Targeting == EAbilityTargeting.AllConnectedRooms || base.Targeting == EAbilityTargeting.Room) && (m_OriginalTargetCount == -1 || !(base.TargetingActor is CPlayerActor)))
			{
				m_ActorsToTarget = m_ValidActorsInRange.OrderBy((CActor x) => SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(x, base.TargetingActor)).Take(m_NumberTargets).ToList();
				m_NumberTargetsRemaining = 0;
			}
			goto IL_03a0;
		case EDamageState.SelectActorToDamage:
			if (m_ActorsToTarget.Count == 0)
			{
				base.AbilityHasHappened = true;
				if (base.TargetingActor.Type == CActor.EType.Player)
				{
					base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				PhaseManager.NextStep();
				return true;
			}
			m_ActorBeingDamaged = m_ActorsToTarget[0];
			m_State = EDamageState.ActorBeenDamaged;
			Perform();
			break;
		case EDamageState.ActorBeenDamaged:
			if (m_ActorBeingDamaged != null)
			{
				if (!base.ActorsTargeted.Contains(m_ActorBeingDamaged))
				{
					if (base.TargetingActor.Type == CActor.EType.Player)
					{
						base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
						if (!m_AbilityHasHappened)
						{
							ScenarioRuleClient.FirstAbilityStarted();
						}
					}
					base.AbilityHasHappened = true;
					base.ActorsTargeted.Add(m_ActorBeingDamaged);
					if (base.ResourcesToTakeFromTargets != null && base.ResourcesToTakeFromTargets.Count > 0)
					{
						foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in base.ResourcesToTakeFromTargets)
						{
							if (m_ActorBeingDamaged.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
							{
								m_ActorBeingDamaged.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
								base.TargetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
							}
						}
					}
					if (base.ResourcesToGiveToTargets != null && base.ResourcesToGiveToTargets.Count > 0)
					{
						foreach (KeyValuePair<string, int> resourcesToGiveToTarget in base.ResourcesToGiveToTargets)
						{
							if (base.TargetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
							{
								base.TargetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
								m_ActorBeingDamaged.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
							}
						}
					}
					_ = m_ActorBeingDamaged.Health;
					bool actorWasAsleep = m_ActorBeingDamaged.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					GameState.ActorBeenDamaged(m_ActorBeingDamaged, m_ModifiedStrength, checkIfPlayerCanAvoidDamage: true, base.TargetingActor, this, base.AbilityType);
					if (ScenarioManager.Scenario.HasActor(m_ActorBeingDamaged))
					{
						if (GameState.ActorHealthCheck(base.TargetingActor, m_ActorBeingDamaged, out var onDeathAbility, isTrap: false, isTerrain: false, actorWasAsleep))
						{
							m_ActorsToIgnore.Add(m_ActorBeingDamaged);
						}
						if (onDeathAbility)
						{
							return true;
						}
					}
				}
				m_State = EDamageState.NextActorCheck;
				Perform();
			}
			else
			{
				m_State = EDamageState.SelectActorToDamage;
				Perform();
			}
			break;
		case EDamageState.NextActorCheck:
			if (m_ActorBeingDamaged != null)
			{
				m_ActorsToTarget.Remove(m_ActorBeingDamaged);
				if (m_NegativeConditions.Count > 0)
				{
					ProcessNegativeStatusEffects(m_ActorBeingDamaged);
				}
			}
			m_NumberTargetsRemaining = m_ActorsToTarget.Count;
			if (m_NumberTargetsRemaining == 0)
			{
				PhaseManager.StepComplete();
				return true;
			}
			m_State = EDamageState.SelectActorToDamage;
			Perform();
			break;
		case EDamageState.InlineSubAbilityCheck:
			if (base.SubAbilities.Count > 0 && base.SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list = new List<CAbility>();
				foreach (CAbility item2 in base.SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					foreach (CActor item3 in base.ActorsTargeted)
					{
						if (item2.MiscAbilityData == null || !item2.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item2.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || item3.IsDead)
						{
							CAbility cAbility = CAbility.CopyAbility(item2, generateNewID: true, fullCopy: true);
							cAbility.ParentAbility = this;
							cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y]);
							cAbility.TargetThisActorAutomatically = item3;
							list.Add(cAbility);
						}
					}
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				m_State = EDamageState.DamageDone;
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EDamageState.DamageDone:
			{
				PhaseManager.NextStep();
				return true;
			}
			IL_03a0:
			if (base.AnimOverload.Length == 0 && (base.UseSubAbilityTargeting || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value)))
			{
				m_State = EDamageState.SelectActorToDamage;
				Perform();
			}
			else
			{
				CActorHasDamaged_MessageData message2 = new CActorHasDamaged_MessageData(base.AnimOverload, base.TargetingActor)
				{
					m_DamageAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			break;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
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
		bool flag = false;
		if (m_State == EDamageState.SelectDamageFocus && !base.AllTargets)
		{
			if (base.AreaEffect != null)
			{
				bool flag2 = false;
				List<CActor> validActorsInArea = GetValidActorsInArea(base.ValidTilesInAreaEffectedIncludingBlocked);
				if (validActorsInArea.Count > 0)
				{
					flag2 = true;
					m_AreaEffectLocked = true;
					base.ValidActorsInRange = validActorsInArea;
				}
				if (!flag2)
				{
					base.TileSelected(selectedTile, optionalTileList);
					LogEvent(ESESubTypeAbility.AbilityTileSelected);
					return;
				}
			}
			SharedAbilityTargeting.TileSelected(this, selectedTile, base.AreaEffect, base.TargetingActor, base.FilterActor, m_Range, ref m_ValidActorsInRange, m_ActorsToIgnore, base.AbilityFilter, m_NumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, base.IsTargetedAbility);
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!base.AllTargets)
		{
			if (base.AreaEffect != null && m_AreaEffectLocked)
			{
				m_AreaEffectLocked = false;
			}
			else if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_NumberTargetsRemaining, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
			{
				flag = true;
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void Restart()
	{
		base.Restart();
		m_CancelAbility = false;
		base.ActorsTargeted.Clear();
		m_ActorsToTarget.Clear();
		m_ModifiedStrength = m_Strength;
		bool flag = false;
		if (base.ActiveBonusData != null && base.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = EDamageState.PreDamageBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(base.TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(base.TargetingActor);
		}
		else if (base.UseSubAbilityTargeting || base.TargetingActor.Type != CActor.EType.Player || (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value))
		{
			m_State = EDamageState.ActorHasDamaged;
			if (base.UseSubAbilityTargeting)
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.UseParentTargets == true)
				{
					if (base.ParentAbility?.ActorsTargeted != null)
					{
						if (base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter))
						{
							base.ValidActorsInRange = base.ParentAbility.ActorsTargeted.ToList();
						}
						else
						{
							base.ValidActorsInRange = new List<CActor>();
							foreach (CActor item in base.ParentAbility.ActorsTargeted)
							{
								if (base.AbilityFilter.IsValidTarget(item, base.FilterActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
								{
									base.ValidActorsInRange.Add(item);
								}
							}
						}
					}
					RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
					m_ActorsToTarget = m_ValidActorsInRange.ToList();
					m_NumberTargets = base.ValidActorsInRange.Count;
					flag = true;
				}
			}
		}
		else
		{
			m_State = EDamageState.SelectDamageFocus;
		}
		m_ActorBeingDamaged = null;
		if (base.AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(base.TargetingActor, ScenarioManager.Tiles[base.TargetingActor.ArrayIndex.X, base.TargetingActor.ArrayIndex.Y], base.AreaEffect, 0f, GameState.GetTilesInRange(base.TargetingActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true), getBlocked: true, ref m_ValidTilesInAreaEffectIncludingBlocked);
		}
		if (m_NumberTargets == -1 || m_AllTargets || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_AllTargets = true;
			if (base.UseSubAbilityTargeting || base.IsInlineSubAbility)
			{
				m_ValidActorsInRange = new List<CActor>();
				base.TilesInRange.Clear();
				if (base.IsInlineSubAbility)
				{
					if (base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
					{
						AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
						if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.HasValue)
						{
							AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
							if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.Value)
							{
								foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
								{
									CActor cActor = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
									if (cActor != null)
									{
										m_ActorsToIgnore.Add(cActor);
									}
									base.TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
									m_ValidActorsInRange.AddRange(GameState.GetActorsInRange(inlineSubAbilityTile.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible));
								}
								base.TilesInRange = base.TilesInRange.Distinct().ToList();
								m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
								goto IL_08e6;
							}
						}
						base.TilesInRange = base.InlineSubAbilityTiles.ToList();
						foreach (CTile inlineSubAbilityTile2 in base.InlineSubAbilityTiles)
						{
							CActor cActor2 = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile2.m_ArrayIndex);
							if (cActor2 != null && !m_ValidActorsInRange.Contains(cActor2) && base.AbilityFilter.IsValidTarget(cActor2, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, false))
							{
								m_ValidActorsInRange.Add(cActor2);
							}
						}
					}
					goto IL_08e6;
				}
				AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
					if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentMovePath.Value && base.ParentAbility is CAbilityMove)
					{
						List<CTile> list = new List<CTile>();
						for (int i = 0; i < CAbilityMove.AllArrayIndexOnPath.Count; i++)
						{
							if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(i))
							{
								Point point = CAbilityMove.AllArrayIndexOnPath[i];
								CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
								list.Add(cTile);
								if (cTile != null)
								{
									base.TilesInRange.AddRange(GameState.GetTilesInRange(cTile.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
								}
							}
						}
						base.TilesInRange = base.TilesInRange.Distinct().ToList();
						foreach (CTile item2 in base.TilesInRange)
						{
							if (item2 != null)
							{
								CActor actorOnTile = GameState.GetActorOnTile(item2, base.TargetingActor, base.AbilityFilter, m_ActorsToIgnore, base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
								if (actorOnTile != null && !m_ValidActorsInRange.Contains(actorOnTile))
								{
									m_ValidActorsInRange.Add(actorOnTile);
								}
							}
						}
						goto IL_08e6;
					}
				}
				if (base.ParentAbility != null && base.ParentAbility.TilesSelected != null && base.ParentAbility.TilesSelected.Count > 0)
				{
					if (base.ParentAbility.AreaEffect != null)
					{
						base.TilesInRange.AddRange(base.ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
					}
					else
					{
						base.TilesInRange.AddRange(base.ParentAbility.TilesSelected);
					}
					AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
					if (miscAbilityData6 != null && miscAbilityData6.UseParentTiles.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData7 = base.MiscAbilityData;
						if (miscAbilityData7 != null && miscAbilityData7.UseParentTiles.Value)
						{
							foreach (CTile item3 in base.TilesInRange)
							{
								CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
								if (cActor3 != null && base.AbilityFilter.IsValidTarget(cActor3, base.TargetingActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
								{
									m_ValidActorsInRange.Add(cActor3);
								}
							}
							goto IL_08d0;
						}
					}
					foreach (CTile item4 in base.TilesInRange.ToList())
					{
						CActor cActor4 = ScenarioManager.Scenario.FindActorAt(item4.m_ArrayIndex);
						m_ActorsToIgnore.Clear();
						if (cActor4 != null)
						{
							m_ActorsToIgnore.Add(cActor4);
						}
						base.TilesInRange.AddRange(GameState.GetTilesInRange(item4.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false));
						m_ValidActorsInRange.AddRange(GameState.GetActorsInRange(item4.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible));
					}
				}
				goto IL_08d0;
			}
			SharedAbilityTargeting.GetValidActorsInRange(this);
			m_NumberTargets = m_ValidActorsInRange.Count;
			m_ActorsToTarget = m_ValidActorsInRange.ToList();
		}
		else if (!flag)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		goto IL_0949;
		IL_0949:
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (base.TargetingActor.Type == CActor.EType.Player && m_AllTargets && base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) && m_ValidActorsInRange.Count > 0)
		{
			foreach (CActor item5 in m_ValidActorsInRange)
			{
				if (CActor.AreActorsAllied(base.TargetingActor.Type, item5.Type))
				{
					m_CanSkip = false;
					break;
				}
			}
		}
		if (base.Strength <= 0)
		{
			m_CancelAbility = true;
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
		return;
		IL_08e6:
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_NumberTargets = m_ValidActorsInRange.Count;
		m_ActorsToTarget = m_ValidActorsInRange.ToList();
		goto IL_0949;
		IL_08d0:
		m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
		goto IL_08e6;
	}

	public override bool CanClearTargets()
	{
		return m_State == EDamageState.SelectDamageFocus;
	}

	public override bool CanReceiveTileSelection()
	{
		if (!base.CanReceiveTileSelection() || m_State != EDamageState.SelectActorToDamage)
		{
			return m_State == EDamageState.SelectDamageFocus;
		}
		return true;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
			if (base.ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, base.TargetingActor);
			}
			else if (cBaseCard != null)
			{
				cBaseCard.AddActiveBonus(this, actor, base.TargetingActor);
			}
			else
			{
				DLLDebug.LogError("Unable to find base ability card for ability " + base.Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EDamageState.SelectDamageFocus)
		{
			Perform();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EDamageState.DamageDone;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDamage(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override string GetDescription()
	{
		if (m_Range > 0 || m_NumberTargets > 0)
		{
			return "Damage(" + m_Strength + ") R: " + m_Range + " N: " + m_NumberTargets;
		}
		return "Damage(" + m_Strength + ")";
	}

	private void InitDamageSummary()
	{
		m_DamageSummary = new CAttackSummary(this);
		m_ModifiedStrength = m_DamageSummary.ModifiedAttackStrength;
		m_NumberTargetsRemaining = m_NumberTargets + m_DamageSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_Range += m_DamageSummary.ActiveBonusAddRangeBuff;
		if (m_DamageSummary.ActiveBonusAddRangeBuff != 0)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count > 0;
	}

	public CAbilityDamage(CAbilityDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
	}
}
