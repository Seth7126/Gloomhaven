using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityInfuse : CAbility
{
	public enum EInfuseState
	{
		Infusing,
		ShowPicker,
		Done
	}

	private EInfuseState m_State;

	public List<ElementInfusionBoardManager.EElement> ElementsToInfuse { get; set; }

	public bool ShowPicker { get; set; }

	public CBaseCard ParentBaseCard { get; set; }

	public bool HasInfusedAnElement { get; set; }

	public CAbilityInfuse(List<ElementInfusionBoardManager.EElement> elementsToInfuse, bool showPicker = false, CBaseCard parentBaseCard = null)
	{
		ElementsToInfuse = elementsToInfuse;
		ShowPicker = showPicker;
		ParentBaseCard = parentBaseCard;
		m_State = EInfuseState.Infusing;
		HasInfusedAnElement = false;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_ActorsToTarget.Clear();
		m_ActorsToTarget.Add(base.TargetingActor);
		m_ValidActorsInRange = new List<CActor>();
		m_ValidActorsInRange.Add(base.TargetingActor);
		m_State = EInfuseState.Infusing;
		if (ShowPicker && ElementsToInfuse.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any))
		{
			m_State = EInfuseState.ShowPicker;
		}
		else
		{
			m_CanUndo = false;
		}
		m_CanSkip = false;
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
		if (m_CancelAbility || ElementAlreadyStrongAndInfuseIfNotStrongSet())
		{
			PhaseManager.NextStep();
			return true;
		}
		switch (m_State)
		{
		case EInfuseState.Infusing:
			m_CanUndo = false;
			if (base.TargetingActor.Type == CActor.EType.Player)
			{
				base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			base.AbilityHasHappened = true;
			DoInfuse();
			return true;
		case EInfuseState.ShowPicker:
			if (ShowPicker && ElementsToInfuse.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any))
			{
				CShowElementPicker_MessageData message = new CShowElementPicker_MessageData(base.TargetingActor)
				{
					m_InfuseAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		default:
			return false;
		}
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

	public void DoInfuse()
	{
		if (base.TargetingActor.Type == CActor.EType.Player)
		{
			List<ElementInfusionBoardManager.EElement> list = ElementsToInfuse.FindAll((ElementInfusionBoardManager.EElement x) => !x.Equals(ElementInfusionBoardManager.EElement.Any));
			AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.InfuseIfNotStrong == true)
			{
				list = list.Where((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) != ElementInfusionBoardManager.EColumn.Strong).ToList();
			}
			if (list.Count > 0)
			{
				ElementInfusionBoardManager.Infuse(list, base.TargetingActor);
				HasInfusedAnElement = true;
			}
		}
		else if (ElementsToInfuse.Count > 0)
		{
			ElementInfusionBoardManager.Infuse(ElementsToInfuse, base.TargetingActor);
			HasInfusedAnElement = true;
		}
		if (HasInfusedAnElement || (!ShowPicker && ElementsToInfuse.Any((ElementInfusionBoardManager.EElement a) => a == ElementInfusionBoardManager.EElement.Any)))
		{
			CElementsInfused_MessageData message = new CElementsInfused_MessageData(base.AnimOverload, base.TargetingActor)
			{
				m_InfusedElements = ElementsToInfuse,
				m_InfuseAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		else if (ShowPicker)
		{
			m_State = EInfuseState.ShowPicker;
			Perform();
		}
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EInfuseState.Infusing;
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
		if (m_State != EInfuseState.Done)
		{
			if (m_State == EInfuseState.ShowPicker)
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityInfuse(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, ElementsToInfuse, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public bool ElementAlreadyStrongAndInfuseIfNotStrongSet()
	{
		AbilityData.MiscAbilityData miscAbilityData = base.MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.InfuseIfNotStrong == true)
		{
			return ElementsToInfuse.All((ElementInfusionBoardManager.EElement x) => ElementInfusionBoardManager.ElementColumn(x) == ElementInfusionBoardManager.EColumn.Strong);
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public CAbilityInfuse()
	{
	}

	public CAbilityInfuse(CAbilityInfuse state, ReferenceDictionary references)
		: base(state, references)
	{
		ElementsToInfuse = references.Get(state.ElementsToInfuse);
		if (ElementsToInfuse == null && state.ElementsToInfuse != null)
		{
			ElementsToInfuse = new List<ElementInfusionBoardManager.EElement>();
			for (int i = 0; i < state.ElementsToInfuse.Count; i++)
			{
				ElementInfusionBoardManager.EElement item = state.ElementsToInfuse[i];
				ElementsToInfuse.Add(item);
			}
			references.Add(state.ElementsToInfuse, ElementsToInfuse);
		}
		ShowPicker = state.ShowPicker;
		HasInfusedAnElement = state.HasInfusedAnElement;
		m_State = state.m_State;
	}
}
