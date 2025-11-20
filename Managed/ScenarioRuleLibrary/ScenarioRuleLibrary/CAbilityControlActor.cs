using System;
using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityControlActor : CAbilityTargeting
{
	public enum EControlDurationType
	{
		None,
		ControlForOneAction,
		ControlForOneTurn
	}

	public class ControlActorAbilityData
	{
		public List<CAbility> ControlAbilities;

		public EControlDurationType ControlDurationType;

		public CActor.EType? ControlType;

		public bool TransferAugments;

		public bool TreatControlAbilityParentAsParent;

		public bool TreatPreviousAbilityToControlAbilityAsParent;

		public CAbilityFilter UseControllingActorModifierDeckFilter;

		public bool UseControllingActorModifierDeck;

		public bool UseControllingActorPlayerControl;

		public ControlActorAbilityData()
		{
			ControlAbilities = new List<CAbility>();
			ControlDurationType = EControlDurationType.ControlForOneAction;
			ControlType = CActor.EType.Player;
			UseControllingActorModifierDeckFilter = CAbilityFilter.CreateDefaultFilter();
		}

		public ControlActorAbilityData Copy()
		{
			return new ControlActorAbilityData
			{
				ControlAbilities = ControlAbilities.ToList(),
				ControlDurationType = ControlDurationType,
				ControlType = ControlType,
				TransferAugments = TransferAugments,
				TreatControlAbilityParentAsParent = TreatControlAbilityParentAsParent,
				TreatPreviousAbilityToControlAbilityAsParent = TreatPreviousAbilityToControlAbilityAsParent,
				UseControllingActorModifierDeckFilter = UseControllingActorModifierDeckFilter.Copy(),
				UseControllingActorModifierDeck = UseControllingActorModifierDeck,
				UseControllingActorPlayerControl = UseControllingActorPlayerControl
			};
		}

		public ControlActorAbilityData Merge(ControlActorAbilityData dataToMerge)
		{
			return new ControlActorAbilityData
			{
				ControlAbilities = ControlAbilities.Concat(dataToMerge.ControlAbilities).ToList(),
				ControlDurationType = ControlDurationType,
				ControlType = ControlType,
				TransferAugments = (TransferAugments || dataToMerge.TransferAugments),
				TreatControlAbilityParentAsParent = (TreatControlAbilityParentAsParent || dataToMerge.TreatControlAbilityParentAsParent),
				TreatPreviousAbilityToControlAbilityAsParent = (TreatPreviousAbilityToControlAbilityAsParent || dataToMerge.TreatPreviousAbilityToControlAbilityAsParent),
				UseControllingActorModifierDeckFilter = dataToMerge.UseControllingActorModifierDeckFilter.Copy(),
				UseControllingActorModifierDeck = (UseControllingActorModifierDeck || dataToMerge.UseControllingActorModifierDeck),
				UseControllingActorPlayerControl = (UseControllingActorPlayerControl || dataToMerge.UseControllingActorPlayerControl)
			};
		}
	}

	public static EControlDurationType[] ControlDurationTypes = (EControlDurationType[])Enum.GetValues(typeof(EControlDurationType));

	public ControlActorAbilityData ControlActorData { get; set; }

	public CAbilityControlActor(ControlActorAbilityData controlActorAbilityData)
		: base(EAbilityType.ControlActor)
	{
		ControlActorData = controlActorAbilityData;
		m_OneTargetAtATime = true;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingControlActor message = new CActorIsApplyingControlActor(base.AnimOverload, actorApplying)
		{
			m_ActorsAppliedTo = actorsAppliedTo,
			m_ControlActorAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (ControlActorData.ControlDurationType.Equals(EControlDurationType.ControlForOneAction))
		{
			if (ControlActorData.ControlAbilities != null && ControlActorData.ControlAbilities.Count > 0 && base.ApplyToActor(actor))
			{
				base.AbilityHasHappened = true;
				actor.MindControlDuration = EControlDurationType.ControlForOneAction;
				List<CAbility> list = new List<CAbility>();
				if (actor is CHeroSummonActor cHeroSummonActor)
				{
					foreach (CAbility controlAbility in ControlActorData.ControlAbilities)
					{
						CAbility cAbility = cHeroSummonActor.ApplyBaseStats(controlAbility);
						if (cAbility != null)
						{
							cAbility.IsControlAbility = true;
							if (ControlActorData.TreatControlAbilityParentAsParent)
							{
								cAbility.ParentAbility = base.ParentAbility;
								cAbility.IsSubAbility = true;
							}
							else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.PreviousPhaseAbilities.Count > 0)
							{
								cAbility.ParentAbility = cPhaseAction.PreviousPhaseAbilities.Last().m_Ability;
								cAbility.IsSubAbility = true;
							}
							list.Add(cAbility);
						}
					}
				}
				else if (actor is CPlayerActor)
				{
					foreach (CAbility controlAbility2 in ControlActorData.ControlAbilities)
					{
						CAbility cAbility2 = CAbility.CopyAbility(controlAbility2, generateNewID: false);
						cAbility2.IsControlAbility = true;
						if (ControlActorData.TreatControlAbilityParentAsParent)
						{
							cAbility2.ParentAbility = base.ParentAbility;
							cAbility2.IsSubAbility = true;
						}
						else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction2 && cPhaseAction2.PreviousPhaseAbilities.Count > 0)
						{
							cAbility2.ParentAbility = cPhaseAction2.PreviousPhaseAbilities.Last().m_Ability;
							cAbility2.IsSubAbility = true;
						}
						list.Add(cAbility2);
					}
				}
				else if (actor is CEnemyActor cEnemyActor)
				{
					foreach (CAbility controlAbility3 in ControlActorData.ControlAbilities)
					{
						CAbility cAbility3 = cEnemyActor.CloneAbilityAndApplyBaseStats(controlAbility3);
						cAbility3.IsControlAbility = true;
						if (ControlActorData.TreatControlAbilityParentAsParent)
						{
							cAbility3.ParentAbility = base.ParentAbility;
							cAbility3.IsSubAbility = true;
						}
						else if (ControlActorData.TreatPreviousAbilityToControlAbilityAsParent && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction3 && cPhaseAction3.PreviousPhaseAbilities.Count > 0)
						{
							cAbility3.ParentAbility = cPhaseAction3.PreviousPhaseAbilities.Last().m_Ability;
							cAbility3.IsSubAbility = true;
						}
						list.Add(cAbility3);
					}
				}
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction4 && cPhaseAction4.ActionAugmentationsConsumed.Count > 0)
				{
					foreach (CAbility clonedAbility in list)
					{
						foreach (CActionAugmentationOp item in cPhaseAction4.ActionAugmentationsConsumed.SelectMany((CActionAugmentation x) => x.AugmentationOps.Where((CActionAugmentationOp y) => y.ParentAbilityName == clonedAbility.Name && y.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)).ToList())
						{
							clonedAbility.OverrideAbilityValues(item.AbilityOverride, perform: false);
						}
					}
				}
				bool useControllingActorModifierDeck = ControlActorData.UseControllingActorModifierDeck && ControlActorData.UseControllingActorModifierDeckFilter.IsValidTarget(actor, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, false);
				GameState.OverrideCurrentActorForOneAction(actor, ControlActorData.ControlType, killActorAfterAction: false, ControlActorData.TransferAugments ? base.TargetingActor.Augments : null, base.TargetingActor, useControllingActorModifierDeck, ControlActorData.UseControllingActorPlayerControl);
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, base.TargetingActor, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: true);
				CActorIsControlled_MessageData message = new CActorIsControlled_MessageData(base.TargetingActor)
				{
					m_ControlActorAbility = this,
					m_ControlledActor = actor
				};
				ScenarioRuleClient.MessageHandler(message);
				return true;
			}
		}
		else if (ControlActorData.ControlDurationType.Equals(EControlDurationType.ControlForOneTurn) && ControlActorData.ControlType.HasValue && base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			actor.MindControlDuration = EControlDurationType.ControlForOneTurn;
			GameState.QueueOverrideActorForOneTurn(actor, ControlActorData.ControlType.Value, ControlActorData.ControlAbilities, base.AbilityBaseCard, base.TargetingActor, ControlActorData.UseControllingActorPlayerControl);
			if (actor is CPlayerActor cPlayerActor)
			{
				foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
				{
					if (heroSummon.Summoner == cPlayerActor)
					{
						GameState.QueueOverrideActorForOneTurn(heroSummon, ControlActorData.ControlType.Value, null, null, base.TargetingActor, ControlActorData.UseControllingActorPlayerControl);
					}
				}
			}
			return true;
		}
		return false;
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type != CActor.EType.Player)
		{
			validActorsInRange.Sort((CActor x, CActor y) => x.Initiative().CompareTo(y.Initiative()));
			validActorsInRange.Reverse();
		}
	}

	public override void RemoveImmuneActorsFromList(ref List<CActor> actorList)
	{
		actorList.RemoveAll((CActor x) => x.Tokens.HasKey(CCondition.ENegativeCondition.Sleep));
		base.RemoveImmuneActorsFromList(ref actorList);
	}

	public override bool IsPositive()
	{
		if (!base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally))
		{
			return base.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion);
		}
		return true;
	}

	public CAbilityControlActor()
	{
	}

	public CAbilityControlActor(CAbilityControlActor state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
