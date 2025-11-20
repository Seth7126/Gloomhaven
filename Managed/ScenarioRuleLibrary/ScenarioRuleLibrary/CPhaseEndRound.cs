namespace ScenarioRuleLibrary;

public class CPhaseEndRound : CPhase
{
	public CPhaseEndRound()
	{
		m_PhaseType = PhaseType.EndRound;
	}

	protected override void OnNextStep(bool passing = false)
	{
		ScenarioRuleClient.MessageHandler(new CEndRound_MessageData(GameState.InternalCurrentActor));
	}

	public void EndRoundSynchronise()
	{
		GameState.NextPhase();
	}
}
