using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPhaseSelectMonsterClassAbilityCards : CPhase
{
	public CPhaseSelectMonsterClassAbilityCards()
	{
		m_PhaseType = PhaseType.MonsterClassesSelectAbilityCards;
	}

	protected override void OnNextStep(bool passing = false)
	{
		List<CMonsterClass> list = new List<CMonsterClass>();
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			list.Add(allAliveMonster.MonsterClass);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			list.Add(@object.MonsterClass);
		}
		CMonsterClassesToSelectAbilityCards_MessageData cMonsterClassesToSelectAbilityCards_MessageData = new CMonsterClassesToSelectAbilityCards_MessageData(null);
		cMonsterClassesToSelectAbilityCards_MessageData.m_MonsterClasses = list;
		ScenarioRuleClient.MessageHandler(cMonsterClassesToSelectAbilityCards_MessageData);
	}

	protected override void OnEndPhase()
	{
		CMonsterClassesHaveSelectedAbilityCards_MessageData message = new CMonsterClassesHaveSelectedAbilityCards_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}
}
