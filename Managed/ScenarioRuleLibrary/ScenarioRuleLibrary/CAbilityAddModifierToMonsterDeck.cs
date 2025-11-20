using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityAddModifierToMonsterDeck : CAbility
{
	public enum EAddModifierToMonsterDeckState
	{
		PlayerConfirming,
		AddingModifiers,
		AddingModifiersDone
	}

	private EAddModifierToMonsterDeckState m_State;

	private List<string> m_ModifierCardNamesToAdd;

	private List<string> m_CombatLogStrings = new List<string>();

	private bool isPositive;

	public List<string> ModifierCardNamesToAdd
	{
		get
		{
			return m_ModifierCardNamesToAdd;
		}
		set
		{
			m_ModifierCardNamesToAdd = value.ToList();
		}
	}

	public CAbilityAddModifierToMonsterDeck(List<string> addModifiers)
	{
		m_ModifierCardNamesToAdd = addModifiers;
		CheckSkippability();
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		m_ActorsToTarget = new List<CActor> { base.TargetingActor };
		CheckSkippability();
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
		case EAddModifierToMonsterDeckState.PlayerConfirming:
		{
			if (base.MiscAbilityData.AutotriggerAbility.HasValue && base.MiscAbilityData.AutotriggerAbility.Value)
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
		case EAddModifierToMonsterDeckState.AddingModifiers:
			if (ScenarioManager.Scenario.Enemies.Count > 0)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies[0];
				if (base.AbilityFilter.IsValidTarget(cEnemyActor, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int i = 0; i < m_Strength; i++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.Enemy2Monsters.Count > 0)
			{
				CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.Enemy2Monsters[0];
				if (base.AbilityFilter.IsValidTarget(cEnemyActor2, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int j = 0; j < m_Strength; j++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor2, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.AllyMonsters.Count > 0)
			{
				CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.AllyMonsters[0];
				if (base.AbilityFilter.IsValidTarget(cEnemyActor3, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int k = 0; k < m_Strength; k++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor3, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.NeutralMonsters.Count > 0)
			{
				CEnemyActor cEnemyActor4 = ScenarioManager.Scenario.NeutralMonsters[0];
				if (base.AbilityFilter.IsValidTarget(cEnemyActor4, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int l = 0; l < m_Strength; l++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor4, ModifierCardNamesToAdd);
					}
				}
			}
			if (ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList().Count > 0)
			{
				CEnemyActor cEnemyActor5 = ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass.Boss).ToList()[0];
				if (base.AbilityFilter.IsValidTarget(cEnemyActor5, base.TargetingActor, isTargetedAbility: false, useTargetOriginalType: false, true, skipUntargetableCheck: true))
				{
					for (int num = 0; num < m_Strength; num++)
					{
						MonsterClassManager.AddAdditionalModifierCards(cEnemyActor5, ModifierCardNamesToAdd);
					}
				}
			}
			foreach (string modifierName in ModifierCardNamesToAdd)
			{
				AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData f) => f.Name == modifierName);
				m_CombatLogStrings.Add(attackModifierYMLData.Card.AttackModifierLogString());
			}
			PhaseManager.NextStep();
			break;
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
		return m_State == EAddModifierToMonsterDeckState.AddingModifiersDone;
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
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityAddModifierToMonsterDeck(subTypeAbility, base.AbilityBaseCard?.Name, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.ID : int.MaxValue, (base.AbilityBaseCard != null) ? base.AbilityBaseCard.CardType : CBaseCard.ECardType.None, base.TargetingActor.Class.ID, m_State, m_CombatLogStrings, base.Strength, base.PositiveConditions.Values.ToList(), base.NegativeConditions.Values.ToList(), base.TargetingActor?.Type, isSummon, base.TargetingActor?.Tokens.CheckPositiveTokens, base.TargetingActor?.Tokens.CheckNegativeTokens, "", CActor.EType.Unknown, ActedOnIsSummon: false, null, null));
	}

	private void CheckSkippability()
	{
		if (m_ModifierCardNamesToAdd.Any((string x) => x.Contains("Bless") || x.Contains("_P") || x.Contains("_T2")))
		{
			isPositive = false;
			SetCanSkip(canSkip: false);
		}
	}

	public override string GetDescription()
	{
		return "AddModifierToMonsterDeck";
	}

	public override bool CanClearTargets()
	{
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}

	public bool HasPassedState(EAddModifierToMonsterDeckState addModifierToMonsterDeckState)
	{
		return m_State > addModifierToMonsterDeckState;
	}

	public override bool EnoughTargetsSelected()
	{
		return true;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return isPositive;
	}

	public CAbilityAddModifierToMonsterDeck()
	{
	}

	public CAbilityAddModifierToMonsterDeck(CAbilityAddModifierToMonsterDeck state, ReferenceDictionary references)
		: base(state, references)
	{
		m_State = state.m_State;
		m_ModifierCardNamesToAdd = references.Get(state.m_ModifierCardNamesToAdd);
		if (m_ModifierCardNamesToAdd == null && state.m_ModifierCardNamesToAdd != null)
		{
			m_ModifierCardNamesToAdd = new List<string>();
			for (int i = 0; i < state.m_ModifierCardNamesToAdd.Count; i++)
			{
				string item = state.m_ModifierCardNamesToAdd[i];
				m_ModifierCardNamesToAdd.Add(item);
			}
			references.Add(state.m_ModifierCardNamesToAdd, m_ModifierCardNamesToAdd);
		}
		m_CombatLogStrings = references.Get(state.m_CombatLogStrings);
		if (m_CombatLogStrings == null && state.m_CombatLogStrings != null)
		{
			m_CombatLogStrings = new List<string>();
			for (int j = 0; j < state.m_CombatLogStrings.Count; j++)
			{
				string item2 = state.m_CombatLogStrings[j];
				m_CombatLogStrings.Add(item2);
			}
			references.Add(state.m_CombatLogStrings, m_CombatLogStrings);
		}
		isPositive = state.isPositive;
	}
}
