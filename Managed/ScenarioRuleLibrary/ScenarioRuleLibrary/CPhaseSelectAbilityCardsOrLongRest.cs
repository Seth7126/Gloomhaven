namespace ScenarioRuleLibrary;

public class CPhaseSelectAbilityCardsOrLongRest : CPhase
{
	public CPhaseSelectAbilityCardsOrLongRest()
	{
		m_PhaseType = PhaseType.SelectAbilityCardsOrLongRest;
	}

	protected override void OnNextStep(bool passing = false)
	{
		CPlayerToSelectAbilityCardsOrLongRest_MessageData message = new CPlayerToSelectAbilityCardsOrLongRest_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep();
	}

	protected override void OnEndPhase()
	{
		CPlayersHaveSelectedAbilityCardsOrLongRest_MessageData message = new CPlayersHaveSelectedAbilityCardsOrLongRest_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}
}
