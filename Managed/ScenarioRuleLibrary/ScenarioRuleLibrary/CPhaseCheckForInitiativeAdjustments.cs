namespace ScenarioRuleLibrary;

public class CPhaseCheckForInitiativeAdjustments : CPhase
{
	public CPhaseCheckForInitiativeAdjustments()
	{
		m_PhaseType = PhaseType.CheckForInitiativeAdjustments;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (GameState.InternalCurrentActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.AdjustInitiative).Count > 0)
		{
			CCheckForInitiativeAdjustments_MessageData message = new CCheckForInitiativeAdjustments_MessageData(GameState.InternalCurrentActor);
			ScenarioRuleClient.MessageHandler(message);
		}
		else
		{
			GameState.NextPhase();
		}
	}
}
