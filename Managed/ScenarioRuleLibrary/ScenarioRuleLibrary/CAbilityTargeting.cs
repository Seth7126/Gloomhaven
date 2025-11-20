using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityTargeting : CAbility
{
	public enum TargetingState
	{
		None,
		ActorIsSelectingAoE,
		ActorIsSelectingTargetingFocus,
		PreProcessingBeforeActorApplies,
		ActorIsApplying,
		ApplyToActors,
		CheckForOneTargetAtATime,
		InlineSubAbilityCheck,
		Done
	}

	protected TargetingState m_State;

	protected bool m_OneTargetAtATime;

	protected bool m_SkipWaitForProgressChoreographer;

	protected CActor m_CameraFocusActor;

	private CActor m_LastActorSelected;

	public bool OneTargetAtATime => m_OneTargetAtATime;

	public bool AreaEffectSelected { get; private set; }

	public int ActiveBonusAddTargetBuff { get; private set; }

	public bool OriginatesFromAnAura { get; private set; }

	public CActor OriginatingAuraOwner { get; private set; }

	public string OriginatingAuraAnimOverload { get; private set; }

	public CActiveBonus.EAuraTriggerAbilityAnimType AuraTriggerAbilityAnimType { get; private set; }

	public CAbilityTargeting(EAbilityType abilityType)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (m_OneTargetAtATime && base.AreaEffect != null)
		{
			m_State = TargetingState.ActorIsSelectingAoE;
		}
		else
		{
			m_State = TargetingState.ActorIsSelectingTargetingFocus;
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (base.UseSubAbilityTargeting)
		{
			if (base.AreaEffect == null && base.ParentAbility?.AreaEffect != null)
			{
				base.AreaEffect = base.ParentAbility.AreaEffect.Copy();
			}
			if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				m_ValidActorsInRange = new List<CActor> { base.TargetingActor };
				m_ActorsToTarget.AddRange(base.ValidActorsInRange);
			}
			else
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.AllTargetsAdjacentToParentTargets.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.Value)
					{
						base.TilesInRange.Clear();
						m_ValidActorsInRange = new List<CActor>();
						if (base.ParentAbility is CAbilityCreate cAbilityCreate)
						{
							foreach (CTile item in cAbilityCreate.TilesSelected)
							{
								base.TilesInRange.AddRange(from x in GameState.GetTilesInRange(item.m_ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false)
									where !base.TilesInRange.Contains(x)
									select x);
								m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(item.m_ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible)
									where !m_ValidActorsInRange.Contains(x)
									select x);
							}
						}
						else
						{
							for (int num = 0; num < base.ParentAbility.ActorsTargeted.Count; num++)
							{
								CActor cActor = base.ParentAbility.ActorsTargeted[num];
								if (cActor != null)
								{
									m_ActorsToIgnore.Add(cActor);
									base.TilesInRange.AddRange(from x in GameState.GetTilesInRange(cActor.ArrayIndex, m_Range, base.Targeting, emptyTilesOnly: false)
										where !base.TilesInRange.Contains(x)
										select x);
									m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(cActor.ArrayIndex, base.TargetingActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, base.AreaEffect, m_ValidTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible)
										where !m_ValidActorsInRange.Contains(x)
										select x);
								}
							}
						}
						goto IL_065a;
					}
				}
				AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
				if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentMovePath.Value && base.ParentAbility is CAbilityMove)
					{
						base.TilesInRange.Clear();
						m_ValidActorsInRange = new List<CActor>();
						for (int num2 = 0; num2 < CAbilityMove.AllArrayIndexOnPath.Count; num2++)
						{
							if (base.MiscAbilityData?.MovePathIndexFilter == null || base.MiscAbilityData.MovePathIndexFilter.Compare(num2))
							{
								Point point = CAbilityMove.AllArrayIndexOnPath[num2];
								CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
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
						goto IL_065a;
					}
				}
				if (base.AreaEffect != null)
				{
					m_ValidTilesInAreaEffect = base.ParentAbility.ValidTilesInAreaAffected.ToList();
					m_ValidTilesInAreaEffectIncludingBlocked = base.ParentAbility.ValidTilesInAreaEffectedIncludingBlocked.ToList();
					base.ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
				}
				else if (base.ParentAbility?.ActorsTargeted != null)
				{
					if (base.AbilityFilter.Equals(base.ParentAbility.AbilityFilter))
					{
						base.ValidActorsInRange = base.ParentAbility.ActorsTargeted.Where((CActor x) => !x.IsDead).ToList();
					}
					else
					{
						base.ValidActorsInRange = new List<CActor>();
						foreach (CActor item3 in base.ParentAbility.ActorsTargeted)
						{
							if (!item3.IsDead && base.AbilityFilter.IsValidTarget(item3, filterActor, base.IsTargetedAbility, useTargetOriginalType: false, base.MiscAbilityData?.CanTargetInvisible))
							{
								base.ValidActorsInRange.Add(item3);
							}
						}
					}
				}
				else if (base.InlineSubAbilityTiles != null && base.InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in base.InlineSubAbilityTiles)
					{
						CActor cActor2 = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
						if (cActor2 != null)
						{
							m_ValidActorsInRange.Add(cActor2);
						}
					}
				}
			}
			goto IL_065a;
		}
		if (base.AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
			m_NumberTargets = m_ValidActorsInRange.Count;
			base.TilesInRange = GameState.GetTilesInRange(targetActor, m_Range, base.Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true);
			m_NumberTargetsRemaining = m_NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else if (targetActor != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else
		{
			base.ValidActorsInRange = new List<CActor>();
		}
		goto IL_06f0;
		IL_06f0:
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		if (base.UseSubAbilityTargeting && base.ValidActorsInRange.Count <= 0)
		{
			m_CancelAbility = true;
		}
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(targetActor, base.Range, base.Targeting, emptyTilesOnly: false);
			m_NumberTargets = base.ValidActorsInRange.Count;
			if (!m_OneTargetAtATime || m_NumberTargets <= 1)
			{
				m_AllTargets = true;
			}
		}
		else
		{
			m_AllTargets = false;
		}
		if (m_NumberTargets == 0)
		{
			m_NumberTargets = 1;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(base.TargetingActor, EAbilityType.AddTarget).ToList();
		ActiveBonusAddTargetBuff = 0;
		if (list != null)
		{
			ActiveBonusAddTargetBuff = list.Sum((CActiveBonus x) => x.ReferenceStrength(this, null));
			if (base.AreaEffect == null)
			{
				m_NumberTargets += ActiveBonusAddTargetBuff;
			}
		}
		m_NumberTargetsRemaining = m_NumberTargets + ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_SkipWaitForProgressChoreographer = false;
		if (base.TargetingActor == null || base.TargetingActor.Type != CActor.EType.Player)
		{
			m_CanUndo = false;
		}
		else if (PhaseManager.Phase.Type == CPhase.PhaseType.Action)
		{
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (cPhaseAction.PreviousPhaseAbilities != null && cPhaseAction.PreviousPhaseAbilities.Count > 0)
			{
				m_CanUndo = !cPhaseAction.PreviousPhaseAbilities.Last().IsCostAbility;
			}
		}
		if (base.TargetingActor.Type == CActor.EType.Player && !IsPositive() && (base.MiscAbilityData == null || !base.MiscAbilityData.NegativeAbilityIsOptional.HasValue || !base.MiscAbilityData.NegativeAbilityIsOptional.Value) && m_ValidActorsInRange.Count > 0 && (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) || base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion)))
		{
			foreach (CActor item4 in m_ValidActorsInRange)
			{
				if (CActor.AreActorsAllied(base.TargetingActor.Type, item4.Type) || item4 == base.TargetingActor)
				{
					m_CanSkip = false;
					break;
				}
			}
		}
		if (!base.AbilityStartListenersInvoked)
		{
			base.AbilityStartListenersInvoked = true;
			base.TargetingActor.m_OnAbilityTargetingStartListeners?.Invoke(this);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		m_AbilityStartComplete = true;
		return;
		IL_065a:
		m_ActorsToTarget = m_ValidActorsInRange.ToList();
		m_NumberTargets = base.ValidActorsInRange.Count;
		goto IL_06f0;
	}

	public virtual bool PreProcessBeforeActorApplies()
	{
		return true;
	}

	public virtual bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		LogEvent(ESESubTypeAbility.ActorIsApplying);
		foreach (CActor item in actorsAppliedTo)
		{
			m_Strength = m_ModifiedStrength;
			SetStatBasedOnXFromTarget(item, actorApplying, base.StatIsBasedOnXEntries, base.AbilityFilter);
		}
		return true;
	}

	public override bool Perform()
	{
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (!base.ProcessIfDead && m_State < TargetingState.CheckForOneTargetAtATime)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0081;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0081;
				}
			}
		}
		if (m_CancelAbility)
		{
			PhaseManager.NextStep();
			return true;
		}
		bool flag2;
		AbilityData.MiscAbilityData miscAbilityData7;
		switch (m_State)
		{
		case TargetingState.ActorIsSelectingAoE:
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			else
			{
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				Perform();
			}
			break;
		case TargetingState.ActorIsSelectingTargetingFocus:
		{
			if (m_OneTargetAtATime)
			{
				if (base.TargetingActor.Type == CActor.EType.Player && base.AreaEffect != null)
				{
					base.AreaEffect = null;
					AreaEffectSelected = true;
					m_NumberTargetsRemaining = base.ValidActorsInRange.Count;
					m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
					if (base.ValidActorsInRange.Count == 1)
					{
						TileSelected(ScenarioManager.Tiles[base.ValidActorsInRange[0].ArrayIndex.X, base.ValidActorsInRange[0].ArrayIndex.Y], null);
						PhaseManager.StepComplete();
						return true;
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				}
				for (int num = base.ValidActorsInRange.Count - 1; num >= 0; num--)
				{
					if (base.ValidActorsInRange[num].IsDead)
					{
						base.ValidActorsInRange.RemoveAt(num);
					}
				}
			}
			if (base.ValidActorsInRange.Count == 0 && base.AreaEffect == null && (base.TargetingActor.Type != CActor.EType.Player || m_AbilityHasHappened))
			{
				PhaseManager.NextStep();
				return true;
			}
			CActor.EType eType = base.TargetingActor.Type;
			AbilityData.MiscAbilityData miscAbilityData2 = base.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.UseOriginalActor == true)
			{
				eType = base.TargetingActor.OriginalType;
			}
			if (base.UseSubAbilityTargeting)
			{
				if (m_ActorsToTarget.Count > 0)
				{
					m_State = TargetingState.PreProcessingBeforeActorApplies;
					Perform();
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (eType == CActor.EType.Player)
			{
				AbilityData.MiscAbilityData miscAbilityData3 = base.MiscAbilityData;
				if (miscAbilityData3 != null && miscAbilityData3.AutotriggerAbility.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = base.MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AutotriggerAbility.Value)
					{
						goto IL_034e;
					}
				}
				bool flag = false;
				if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || m_AllTargets)
				{
					m_ActorsToTarget.AddRange(base.ValidActorsInRange);
					flag = true;
				}
				else if (m_OneTargetAtATime && !m_AllTargets)
				{
					m_ActorsToTarget.Clear();
				}
				AbilityData.MiscAbilityData miscAbilityData5 = base.MiscAbilityData;
				if (miscAbilityData5 != null && miscAbilityData5.AlsoTargetSelf == true)
				{
					m_ActorsToTarget.Add(base.TargetingActor);
					m_NumberTargets++;
					flag = true;
				}
				if (base.MiscAbilityData?.AlsoTargetAdjacent != null)
				{
					CAbilityFilter alsoTargetAdjacent = base.MiscAbilityData.AlsoTargetAdjacent;
					foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(base.TargetingActor))
					{
						if (alsoTargetAdjacent.IsValidTarget(allAdjacentActor, base.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false))
						{
							m_ActorsToTarget.Add(allAdjacentActor);
							m_NumberTargets++;
						}
					}
					flag = true;
				}
				if (flag)
				{
					m_ActorsToTarget = m_ActorsToTarget.Distinct().ToList();
				}
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData2 = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData2.m_TargetingAbility = this;
				AbilityData.MiscAbilityData miscAbilityData6 = base.MiscAbilityData;
				cActorIsSelectingTargetingFocus_MessageData2.m_CanUndo = (miscAbilityData6 == null || miscAbilityData6.CanUndo != false) && cActorIsSelectingTargetingFocus_MessageData2.m_CanUndo;
				cActorIsSelectingTargetingFocus_MessageData2.m_IsPositive = IsPositive();
				cActorIsSelectingTargetingFocus_MessageData2.m_CameraFocusActor = ((m_CameraFocusActor != null) ? m_CameraFocusActor : base.TargetingActor);
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData2);
				break;
			}
			goto IL_034e;
		}
		case TargetingState.PreProcessingBeforeActorApplies:
			if (PreProcessBeforeActorApplies())
			{
				m_State = TargetingState.ActorIsApplying;
				Perform();
			}
			break;
		case TargetingState.ActorIsApplying:
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			foreach (CActor item2 in m_ActorsToTarget)
			{
				CTile item = ScenarioManager.Tiles[item2.ArrayIndex.X, item2.ArrayIndex.Y];
				if (!m_TilesSelected.Contains(item))
				{
					base.TilesSelected.Add(item);
				}
			}
			if (ActorIsApplying(base.TargetingActor, m_ActorsToTarget))
			{
				m_State = TargetingState.ApplyToActors;
				Perform();
			}
			break;
		case TargetingState.ApplyToActors:
		{
			m_CanUndo = false;
			base.AbilityHasHappened = true;
			List<CActor> list2 = new List<CActor>();
			if (m_OneTargetAtATime && m_LastActorSelected != null)
			{
				list2 = new List<CActor> { m_LastActorSelected };
				m_LastActorSelected = null;
			}
			else if (base.AreaEffect == null)
			{
				if (base.AllTargetsOnAttackPath && m_ActorsToTarget.Count > 0)
				{
					bool foundPath;
					List<Point> list3 = ScenarioManager.PathFinder.FindPath(base.TargetingActor.ArrayIndex, m_ActorsToTarget[0].ArrayIndex, base.Range > 1, ignoreMoveCost: true, out foundPath);
					if (foundPath)
					{
						foreach (Point point in list3)
						{
							CActor cActor = base.ValidActorsInRange.Find((CActor x) => x.ArrayIndex == point);
							if (cActor != null && !m_ActorsToTarget.Contains(cActor))
							{
								m_ActorsToTarget.Add(cActor);
							}
						}
					}
				}
				List<CActor> list4 = list2;
				List<CActor> collection = (base.ValidActorsInRange = m_ActorsToTarget.Where((CActor w) => !m_ActorsToIgnore.Contains(w)).ToList());
				list4.AddRange(collection);
			}
			else
			{
				list2.AddRange(base.ValidActorsInRange);
			}
			bool flag3 = false;
			foreach (CActor item3 in list2)
			{
				flag3 = ApplyToActor(item3) || flag3;
				if (flag3 && this is CAbilityCondition)
				{
					base.TargetingActor.m_OnConditionApplyToActorListeners?.Invoke(this, item3);
				}
				m_ActorsToIgnore.Add(item3);
				if (m_OneTargetAtATime)
				{
					m_UndoNumberTargetsRemaining--;
					base.ValidActorsInRange.Remove(item3);
					if (!flag3)
					{
						PhaseManager.StepComplete();
						return true;
					}
					break;
				}
			}
			if (flag3)
			{
				CBaseCard cBaseCard = base.TargetingActor.FindCardWithAbility(this);
				if (cBaseCard != null)
				{
					List<CActiveBonus> list6 = cBaseCard.ActiveBonuses.FindAll((CActiveBonus x) => x.IsAura);
					List<CActor> list7 = new List<CActor>();
					foreach (CActiveBonus item4 in list6)
					{
						foreach (CActor item5 in item4.ValidActorsInRangeOfAura)
						{
							if (!list7.Contains(item5) && !list2.Contains(item5))
							{
								list7.Add(item5);
							}
						}
					}
					CAbilityTargetingCombatLog_MessageData message = new CAbilityTargetingCombatLog_MessageData(base.AnimOverload, base.TargetingActor)
					{
						m_Ability = this,
						m_ActorsAppliedTo = ((list7.Count > 0) ? list7 : list2)
					};
					ScenarioRuleClient.MessageHandler(message);
					LogEvent(ESESubTypeAbility.AbilityPerform);
					foreach (CActor item6 in list7)
					{
						if (m_PositiveConditions.Count > 0)
						{
							ProcessPositiveStatusEffects(item6);
						}
					}
				}
				if (m_SkipWaitForProgressChoreographer || (base.TargetingActor is CPlayerActor && (base.AbilityType.Equals(EAbilityType.RecoverDiscardedCards) || base.AbilityType.Equals(EAbilityType.RecoverLostCards) || base.AbilityType.Equals(EAbilityType.LoseCards) || base.AbilityType.Equals(EAbilityType.DiscardCards) || base.AbilityType.Equals(EAbilityType.IncreaseCardLimit) || base.AbilityType.Equals(EAbilityType.ExtraTurn) || base.AbilityType.Equals(EAbilityType.LoseGoalChestReward) || base.AbilityType.Equals(EAbilityType.TransferDooms)) && m_Strength < int.MaxValue) || base.AbilityType.Equals(EAbilityType.AddDoom))
				{
					if (m_SkipWaitForProgressChoreographer)
					{
						m_State++;
					}
					return true;
				}
				if (base.AbilityType.Equals(EAbilityType.ControlActor))
				{
					m_State++;
				}
			}
			CWaitForProgressChoreographer_MessageData message2 = new CWaitForProgressChoreographer_MessageData(base.TargetingActor)
			{
				WaitActor = base.TargetingActor,
				WaitTickFrame = 10000,
				ClearEvents = base.SkipAnim
			};
			ScenarioRuleClient.MessageHandler(message2);
			return true;
		}
		case TargetingState.CheckForOneTargetAtATime:
			if (m_OneTargetAtATime && (m_NumberTargetsRemaining > 0 || AreaEffectSelected) && base.ValidActorsInRange.Count > 0)
			{
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				Perform();
			}
			else
			{
				PhaseManager.StepComplete();
			}
			break;
		case TargetingState.InlineSubAbilityCheck:
			if (base.SubAbilities.Count > 0 && base.SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list = new List<CAbility>();
				foreach (CAbility item7 in base.SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					foreach (CActor item8 in base.ActorsTargeted)
					{
						if (item7.MiscAbilityData == null || !item7.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item7.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || item8.IsDead)
						{
							CAbility cAbility = CAbility.CopyAbility(item7, generateNewID: true, fullCopy: true);
							cAbility.ParentAbility = this;
							cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item8.ArrayIndex.X, item8.ArrayIndex.Y]);
							cAbility.TargetThisActorAutomatically = item8;
							list.Add(cAbility);
						}
					}
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				m_State = TargetingState.Done;
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case TargetingState.Done:
			{
				PhaseManager.NextStep();
				break;
			}
			IL_034e:
			if (base.AreaEffect == null)
			{
				SortValidActorsInRange(base.TargetingActor, ref m_ValidActorsInRange);
				if (!base.AllTargets)
				{
					int num2 = base.ValidActorsInRange.Count - 1;
					while (num2 >= 0 && base.ValidActorsInRange.Count > m_NumberTargets)
					{
						base.ValidActorsInRange.RemoveAt(num2);
						num2--;
					}
				}
			}
			m_ActorsToTarget.AddRange(base.ValidActorsInRange);
			flag2 = false;
			miscAbilityData7 = base.MiscAbilityData;
			if (miscAbilityData7 != null && miscAbilityData7.AlsoTargetSelf == true)
			{
				m_ActorsToTarget.Add(base.TargetingActor);
				m_NumberTargets++;
				flag2 = true;
			}
			if (base.MiscAbilityData?.AlsoTargetAdjacent != null)
			{
				CAbilityFilter alsoTargetAdjacent2 = base.MiscAbilityData.AlsoTargetAdjacent;
				foreach (CActor allAdjacentActor2 in ScenarioManager.GetAllAdjacentActors(base.TargetingActor))
				{
					if (alsoTargetAdjacent2.IsValidTarget(allAdjacentActor2, base.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false))
					{
						m_ActorsToTarget.Add(allAdjacentActor2);
						m_NumberTargets++;
					}
				}
				flag2 = true;
			}
			if (flag2)
			{
				m_ActorsToTarget = m_ActorsToTarget.Distinct().ToList();
			}
			m_TilesSelected.Clear();
			foreach (CActor item9 in m_ActorsToTarget)
			{
				m_TilesSelected.Add(ScenarioManager.Tiles[item9.ArrayIndex.X, item9.ArrayIndex.Y]);
			}
			m_State = TargetingState.PreProcessingBeforeActorApplies;
			Perform();
			break;
		}
		return true;
		IL_0081:
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
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_005f;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_005f;
				}
			}
		}
		bool flag = false;
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
		DLLDebug.Log("ValidTilesInAreaEffectedIncludingBlocked count: " + base.ValidTilesInAreaEffectedIncludingBlocked.Count);
		DLLDebug.Log("ValidActorsInRange Count: " + base.ValidActorsInRange.Count);
		DLLDebug.Log("m_ActorsToTarget Count: " + m_ActorsToTarget.Count + " m_NumberTargets: " + m_NumberTargets);
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, base.FilterActor, base.AbilityFilter, new List<CActor>(), base.IsTargetedAbility, base.MiscAbilityData?.CanTargetInvisible);
		if ((m_OneTargetAtATime ? (m_ActorsToTarget.Count <= 1) : (m_ActorsToTarget.Count < m_NumberTargets)) && (!m_OneTargetAtATime || !base.ActorsTargeted.Contains(actorOnTile)))
		{
			m_LastActorSelected = actorOnTile;
			if (m_ActorsToTarget.Contains(m_LastActorSelected))
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					base.TilesSelected.Add(selectedTile);
				}
			}
			else if (m_State == TargetingState.ActorIsSelectingTargetingFocus || m_State == TargetingState.ActorIsSelectingAoE)
			{
				if (m_OneTargetAtATime && m_LastActorSelected != null)
				{
					if (m_ActorsToTarget.Count > 0)
					{
						m_NumberTargetsRemaining++;
					}
					m_ActorsToTarget.Clear();
				}
				if (SharedAbilityTargeting.TileSelected(this, selectedTile, base.AreaEffect, base.TargetingActor, base.FilterActor, m_Range, ref m_ValidActorsInRange, m_ActorsToIgnore, base.AbilityFilter, m_OneTargetAtATime ? m_NumberTargetsRemaining : m_NumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, base.IsTargetedAbility))
				{
					if (base.AreaEffect != null && m_AreaEffectLocked)
					{
						m_ActorsToTarget.AddRange(m_ValidActorsInRange);
					}
					if ((m_State == TargetingState.ActorIsSelectingAoE || m_State == TargetingState.ActorIsSelectingTargetingFocus) && base.AreaEffect != null && ActiveBonusAddTargetBuff != 0)
					{
						m_AreaEffectBackup = base.AreaEffect;
						base.AreaEffect = null;
						m_ActorsToTarget = new List<CActor>(base.ValidActorsInRange);
						m_ActorsToIgnore = new List<CActor>(base.ValidActorsInRange);
						base.ValidActorsInRange = GameState.GetActorsInRange(base.TargetingActor, base.FilterActor, m_Range, m_ActorsToIgnore, base.AbilityFilter, null, base.AllPossibleTilesInAreaEffect, base.IsTargetedAbility, null, base.MiscAbilityData?.CanTargetInvisible);
						RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
						CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
						cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
						cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
						ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
					}
				}
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
		return;
		IL_005f:
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!base.ProcessIfDead)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_005f;
			}
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_005f;
				}
			}
		}
		bool flag = false;
		if (base.AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
		}
		else if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_OneTargetAtATime ? m_NumberTargetsRemaining : m_NumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
		{
			if (base.AreaEffectBackup != null)
			{
				base.AreaEffect = m_AreaEffectBackup;
				m_AreaEffectBackup = null;
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				m_ActorsToIgnore.Clear();
				m_NumberTargetsRemaining = m_NumberTargets + ActiveBonusAddTargetBuff;
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			m_AreaEffectLocked = false;
			SharedAbilityTargeting.GetValidActorsInRange(this);
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
		return;
		IL_005f:
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == TargetingState.ActorIsSelectingTargetingFocus)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == TargetingState.ActorIsSelectingTargetingFocus;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != TargetingState.ActorIsSelectingAoE)
			{
				return m_State == TargetingState.ActorIsSelectingTargetingFocus;
			}
			return true;
		}
		return false;
	}

	public virtual void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State >= TargetingState.Done;
	}

	public override void Restart()
	{
		base.IsUpdating = true;
		base.Restart();
		base.ActorsTargeted.Clear();
		if (m_OneTargetAtATime && base.AreaEffect != null)
		{
			m_State = TargetingState.ActorIsSelectingAoE;
		}
		else
		{
			m_State = TargetingState.ActorIsSelectingTargetingFocus;
		}
		m_ActorsToTarget = new List<CActor>();
		if (base.UseSubAbilityTargeting)
		{
			if (base.AreaEffect == null && base.ParentAbility?.AreaEffect != null)
			{
				base.AreaEffect = base.ParentAbility.AreaEffect.Copy();
			}
			if (base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				m_ValidActorsInRange = new List<CActor> { base.TargetingActor };
				m_ActorsToTarget.AddRange(base.ValidActorsInRange);
			}
			else if (base.AreaEffect != null)
			{
				m_ValidTilesInAreaEffect = base.ParentAbility.ValidTilesInAreaAffected.ToList();
				m_ValidTilesInAreaEffectIncludingBlocked = base.ParentAbility.ValidTilesInAreaEffectedIncludingBlocked.ToList();
				base.ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
				m_ActorsToTarget.AddRange(base.ValidActorsInRange);
			}
			else if (base.ParentAbility?.ActorsTargeted != null)
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
			m_ActorsToTarget = m_ValidActorsInRange.ToList();
			m_NumberTargets = base.ValidActorsInRange.Count;
		}
		else if (base.TargetingActor != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else
		{
			base.ValidActorsInRange = new List<CActor>();
		}
		if (m_NumberTargets == base.OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (m_NumberTargets == -1 || m_AllTargets)
		{
			if (!m_OneTargetAtATime)
			{
				m_AllTargets = true;
			}
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(base.TargetingActor, base.Range, base.Targeting, emptyTilesOnly: false);
			m_NumberTargets = base.ValidActorsInRange.Count;
		}
		m_NumberTargetsRemaining = m_NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_SkipWaitForProgressChoreographer = false;
		LogEvent(ESESubTypeAbility.AbilityRestart);
		base.IsUpdating = false;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public bool HasPassedState(TargetingState targetingState)
	{
		return m_State > targetingState;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count > 0;
	}

	public override bool MaxTargetsSelected()
	{
		if (m_OneTargetAtATime)
		{
			return m_ActorsToTarget.Count > 0;
		}
		return base.MaxTargetsSelected();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		EAbilityType abilityType = base.AbilityType;
		if (base.ActiveBonusData.OverrideAsSong)
		{
			abilityType = EAbilityType.PlaySong;
		}
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
		string actedOnClass = "";
		CActor.EType actedOnType = CActor.EType.Unknown;
		bool actedOnIsSummon = false;
		CActor actor = null;
		if (base.ActorsTargeted.Count > 0)
		{
			actor = base.ActorsTargeted[base.ActorsTargeted.Count - 1];
			actedOnClass = actor.Class.ID;
			actedOnType = actor.Type;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor2 != null)
				{
					actedOnIsSummon = cEnemyActor2.IsSummon;
				}
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(abilityType, subTypeAbility, m_State, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor?.Class.ID, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, actedOnClass, actedOnType, actedOnIsSummon, actor?.Tokens.CheckPositiveTokens, actor?.Tokens.CheckNegativeTokens));
	}

	public void SetOriginatingFromAura(CActor actorOwningAura, string auraAnimOverload, CActiveBonus.EAuraTriggerAbilityAnimType auraTriggerAbilityAnimType)
	{
		OriginatesFromAnAura = true;
		OriginatingAuraOwner = actorOwningAura;
		OriginatingAuraAnimOverload = auraAnimOverload ?? string.Empty;
		AuraTriggerAbilityAnimType = auraTriggerAbilityAnimType;
	}

	public CAbilityTargeting()
	{
	}

	public CAbilityTargeting(CAbilityTargeting state, ReferenceDictionary references)
		: base(state, references)
	{
		AreaEffectSelected = state.AreaEffectSelected;
		ActiveBonusAddTargetBuff = state.ActiveBonusAddTargetBuff;
		OriginatesFromAnAura = state.OriginatesFromAnAura;
		OriginatingAuraAnimOverload = state.OriginatingAuraAnimOverload;
		AuraTriggerAbilityAnimType = state.AuraTriggerAbilityAnimType;
		m_State = state.m_State;
		m_OneTargetAtATime = state.m_OneTargetAtATime;
		m_SkipWaitForProgressChoreographer = state.m_SkipWaitForProgressChoreographer;
	}
}
