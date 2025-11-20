using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityConsumeElement : CAbility
{
	public enum EConsumeState
	{
		ActorSelectingTargetingFocus,
		Consuming,
		ShowPicker,
		Done
	}

	private EConsumeState m_State;

	public List<ElementInfusionBoardManager.EElement> ElementsToConsume { get; set; }

	public bool ShowPicker { get; set; }

	public CBaseCard ParentBaseCard { get; set; }

	public CAbilityConsumeElement(List<ElementInfusionBoardManager.EElement> elementsToConsume, bool showPicker = false, CBaseCard parentBaseCard = null)
	{
		ElementsToConsume = elementsToConsume;
		ShowPicker = showPicker;
		ParentBaseCard = parentBaseCard;
		m_State = EConsumeState.Consuming;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_ActorsToTarget.Clear();
		m_ActorsToTarget.Add(base.TargetingActor);
		m_ValidActorsInRange = new List<CActor>();
		m_ValidActorsInRange.Add(base.TargetingActor);
		m_State = EConsumeState.ActorSelectingTargetingFocus;
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
		case EConsumeState.ActorSelectingTargetingFocus:
		{
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(base.TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EConsumeState.Consuming:
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			base.AbilityHasHappened = true;
			DoConsume();
			return true;
		case EConsumeState.ShowPicker:
			if (ShowPicker && ElementsToConsume.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any))
			{
				CShowElementPicker_MessageData message = new CShowElementPicker_MessageData(base.TargetingActor)
				{
					m_ConsumeAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		return false;
		IL_0075:
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			if (base.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(base.TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public void DoConsume()
	{
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			foreach (ElementInfusionBoardManager.EElement item in ElementsToConsume.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any)))
			{
				ElementInfusionBoardManager.Consume(item, base.TargetingActor);
			}
		}
		else
		{
			foreach (ElementInfusionBoardManager.EElement item2 in ElementsToConsume)
			{
				ElementInfusionBoardManager.Consume(item2, base.TargetingActor);
			}
		}
		CElementsInfused_MessageData message = new CElementsInfused_MessageData(base.AnimOverload, base.TargetingActor)
		{
			m_InfusedElements = ElementsToConsume,
			m_ConsumeAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EConsumeState.ActorSelectingTargetingFocus;
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
		if (m_State != EConsumeState.Done)
		{
			if (m_State == EConsumeState.ShowPicker)
			{
				return !ShowPicker;
			}
			return false;
		}
		return true;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		Enum.TryParse<CAbilityInfuse.EInfuseState>(m_State.ToString(), out var result);
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityInfuse(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, result, ElementsToConsume, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityConsumeElement()
	{
	}

	public CAbilityConsumeElement(CAbilityConsumeElement state, ReferenceDictionary references)
		: base(state, references)
	{
		ElementsToConsume = references.Get(state.ElementsToConsume);
		if (ElementsToConsume == null && state.ElementsToConsume != null)
		{
			ElementsToConsume = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.ElementsToConsume.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.ElementsToConsume[i];
				ElementsToConsume.Add(item);
			}
			references.Add(state.ElementsToConsume, ElementsToConsume);
		}
		ShowPicker = state.ShowPicker;
		m_State = state.m_State;
	}
}
