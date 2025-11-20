namespace ScenarioRuleLibrary;

public class CPhaseStartTurn : CPhase
{
	public CPhaseStartTurn()
	{
		m_PhaseType = PhaseType.StartTurn;
	}

	protected override void OnNextStep(bool passing = false)
	{
		CStartTurn_MessageData message = new CStartTurn_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}
}
