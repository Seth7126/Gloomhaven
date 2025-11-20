namespace ScenarioRuleLibrary;

public class CPhaseActionSelection : CPhase
{
	public CPhaseActionSelection()
	{
		m_PhaseType = PhaseType.ActionSelection;
		GameState.CurrentAction = null;
		GameState.InternalCurrentActor.ActionSelection();
		CActionSelectionPhaseStart_MessageData message = new CActionSelectionPhaseStart_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (GameState.ActorsToOverrideTurns.TryGetValue(GameState.InternalCurrentActor, out var value) && value.OverrideActionAbilities != null && value.OverrideActionAbilities.Count > 0 && value.OverrideActionBaseCard != null)
		{
			GameState.CurrentAction = null;
			PhaseManager.StartOverrideTurnAbilities(value.OverrideActionAbilities, value.OverrideActionBaseCard);
			return;
		}
		if (GameState.InternalCurrentActor is CPlayerActor cPlayerActor)
		{
			bool flag = false;
			if (!cPlayerActor.CharacterClass.LongRest)
			{
				if (!cPlayerActor.CharacterClass.HasLongRested || cPlayerActor.IsTakingExtraTurn)
				{
					if (GameState.CurrentActionSelectionSequence != GameState.ActionSelectionSequenceType.FirstAction && GameState.CurrentActionSelectionSequence != GameState.ActionSelectionSequenceType.SecondAction)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					cPlayerActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.AtEndOfTurn, CItem.EItemTrigger.DuringOwnTurn);
				}
			}
		}
		CActionSelection_MessageData message = new CActionSelection_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}
}
