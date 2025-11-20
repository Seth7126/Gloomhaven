using System;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CPhaseEndTurn : CPhase
{
	private bool m_SentEndTurnMessage;

	public CPhaseEndTurn()
	{
		m_PhaseType = PhaseType.EndTurn;
		m_SentEndTurnMessage = false;
	}

	protected override void OnNextStep(bool passing = false)
	{
		if (!m_SentEndTurnMessage)
		{
			ScenarioRuleClient.MessageHandler(new CEndTurn_MessageData(GameState.InternalCurrentActor));
			m_SentEndTurnMessage = true;
		}
		else
		{
			DLLDebug.LogError("Attempted to send EndTurn message twice in the same EndTurn phase\n" + Environment.StackTrace);
		}
	}

	public void EndTurnSynchronise()
	{
		GameState.NextPhase();
	}
}
