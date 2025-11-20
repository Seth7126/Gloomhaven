namespace ScenarioRuleLibrary;

internal class CPhaseAutosave : CPhase
{
	public CPhaseAutosave()
	{
		m_PhaseType = PhaseType.Autosave;
	}

	protected override void OnNextStep(bool passing = false)
	{
		CProcessAutosaves_MessageData message = new CProcessAutosaves_MessageData(null);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnStepComplete(bool passingStep = false)
	{
		NextStep();
	}
}
