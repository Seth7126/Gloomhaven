using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRedistributeDamage : CAbility
{
	public enum ERedistributeState
	{
		None,
		ShowRedistributeUI,
		ActorIsRedistributing,
		ApplyRedistributions,
		RedistributeDone
	}

	private ERedistributeState m_State;

	private List<Tuple<CActor, int>> m_HealthChanges;

	public CAbilityRedistributeDamage()
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ERedistributeState.ShowRedistributeUI;
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
		case ERedistributeState.ShowRedistributeUI:
		{
			m_ActorsToTarget.Add(base.TargetingActor);
			m_ActorsToTarget.AddRange(ScenarioManager.Scenario.HeroSummons);
			CShowRedistributeDamageUI_MessageData cShowRedistributeDamageUI_MessageData = new CShowRedistributeDamageUI_MessageData(base.TargetingActor);
			cShowRedistributeDamageUI_MessageData.m_ActorsToRedistributeBetween = m_ActorsToTarget;
			cShowRedistributeDamageUI_MessageData.m_RedistributeDamageAbility = this;
			ScenarioRuleClient.MessageHandler(cShowRedistributeDamageUI_MessageData);
			break;
		}
		case ERedistributeState.ActorIsRedistributing:
		{
			base.TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
			base.AbilityHasHappened = true;
			m_CanUndo = false;
			CActorIsRedistributingDamage_MessageData cActorIsRedistributingDamage_MessageData = new CActorIsRedistributingDamage_MessageData(base.TargetingActor);
			cActorIsRedistributingDamage_MessageData.m_ActorsRedistributingDamageTo = m_HealthChanges.Select((Tuple<CActor, int> x) => x.Item1).ToList();
			cActorIsRedistributingDamage_MessageData.m_RedistributeDamageAbility = this;
			ScenarioRuleClient.MessageHandler(cActorIsRedistributingDamage_MessageData);
			break;
		}
		case ERedistributeState.ApplyRedistributions:
			foreach (Tuple<CActor, int> healthChange in m_HealthChanges)
			{
				CActor item = healthChange.Item1;
				item.Health = healthChange.Item2;
				GameState.ActorHealthCheck(base.TargetingActor, item);
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(item, item, item.Health);
				ScenarioRuleClient.MessageHandler(message);
			}
			PhaseManager.StepComplete();
			break;
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

	public void StoreHealthChanges(List<Tuple<CActor, int>> healthChanges)
	{
		m_HealthChanges = healthChanges;
	}

	public int GetHealthChangeForActor(CActor actor)
	{
		if (m_HealthChanges == null)
		{
			return 0;
		}
		return m_HealthChanges.FirstOrDefault((Tuple<CActor, int> it) => it.Item1 == actor)?.Item2 ?? 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ERedistributeState.RedistributeDone;
	}

	public override void Undo()
	{
		base.Undo();
		CHideRedistributeDamageUI_MessageData message = new CHideRedistributeDamageUI_MessageData(base.TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityRedistributeDamage(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public CAbilityRedistributeDamage(CAbilityRedistributeDamage state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_HealthChanges = references.Get(state.m_HealthChanges);
		if (m_HealthChanges != null || state.m_HealthChanges == null)
		{
			return;
		}
		m_HealthChanges = new List<Tuple<CActor, int>>();
		for (int i = 0; i < state.m_HealthChanges.Count; i++)
		{
			Tuple<CActor, int> tuple = state.m_HealthChanges[i];
			CActor cActor = references.Get(tuple.Item1);
			if (cActor == null && tuple.Item1 != null)
			{
				cActor = new CActor(tuple.Item1, references);
				references.Add(tuple.Item1, cActor);
			}
			int item = tuple.Item2;
			Tuple<CActor, int> item2 = new Tuple<CActor, int>(cActor, item);
			m_HealthChanges.Add(item2);
		}
		references.Add(state.m_HealthChanges, m_HealthChanges);
	}
}
