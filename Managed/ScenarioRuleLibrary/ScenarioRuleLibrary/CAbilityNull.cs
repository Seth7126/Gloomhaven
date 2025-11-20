using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityNull : CAbility
{
	public enum ENullState
	{
		NullBegin,
		NullDone
	}

	private ENullState m_State;

	public CAbilityNull()
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ENullState.NullBegin;
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
		if (base.TargetingActor.Type != CActor.EType.Player)
		{
			if (base.TargetingActor.Type != CActor.EType.Player)
			{
				base.AbilityHasHappened = true;
				m_State = ENullState.NullDone;
			}
			PhaseManager.NextStep();
			return true;
		}
		if (m_State == ENullState.NullBegin)
		{
			CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = new CActorWantsAnActionConfirmation_MessageData(base.TargetingActor);
			cActorWantsAnActionConfirmation_MessageData.m_Ability = this;
			cActorWantsAnActionConfirmation_MessageData.m_Confirm = true;
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (cPhaseAction != null)
			{
				if (base.TargetingActor.Type == CActor.EType.Player && !m_AbilityHasHappened)
				{
					base.AbilityHasHappened = true;
					base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				if (cPhaseAction.ActionAugmentationsAvailableForCurrentAbility().Count <= 0)
				{
					m_CancelAbility = true;
					PhaseManager.NextStep();
					return true;
				}
				if (cPhaseAction.HasAnyUntoggleableActionAugments() || cPhaseAction.AnyActionAugmentationsConsumed() || cPhaseAction.PreviousPhaseAbilities.Count > 0)
				{
					m_CanUndo = false;
				}
			}
			ScenarioRuleClient.MessageHandler(cActorWantsAnActionConfirmation_MessageData);
			return true;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				CPlayerIsStunned_MessageData message2 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ENullState.NullDone;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityNull(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public CAbilityNull(CAbilityNull state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
	}
}
