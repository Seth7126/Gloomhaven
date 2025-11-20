namespace ScenarioRuleLibrary;

public class CActorWantsAnActionConfirmation_MessageData : CMessageData
{
	public CAbility m_Ability;

	public bool m_Confirm = true;

	public CActorWantsAnActionConfirmation_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorWantsAnActionConfirmation, actorSpawningMessage)
	{
	}
}
