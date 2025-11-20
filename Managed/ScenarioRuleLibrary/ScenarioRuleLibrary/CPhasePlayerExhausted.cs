using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPhasePlayerExhausted : CPhase
{
	public CPhasePlayerExhausted()
	{
		m_PhaseType = PhaseType.PlayerExhausted;
	}

	protected override void OnNextStep(bool passing = false)
	{
		List<CPlayerActor> list = ScenarioManager.Scenario.PlayerActors.FindAll((CPlayerActor x) => x.CharacterClass.HandAbilityCards.Count < 2 && x.CharacterClass.DiscardedAbilityCards.Count < 2);
		if (list.Count > 0)
		{
			foreach (CPlayerActor item in list)
			{
				bool actorWasAsleep = item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.KillActor(item, item, CActor.ECauseOfDeath.NoMoreCards, out var _, null, actorWasAsleep);
			}
		}
		CPlayersExhausted_MessageData cPlayersExhausted_MessageData = new CPlayersExhausted_MessageData(null);
		cPlayersExhausted_MessageData.m_Players = list;
		ScenarioRuleClient.MessageHandler(cPlayersExhausted_MessageData);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		GameState.NextPhase();
	}
}
