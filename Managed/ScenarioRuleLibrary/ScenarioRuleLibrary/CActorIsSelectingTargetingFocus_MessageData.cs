namespace ScenarioRuleLibrary;

public class CActorIsSelectingTargetingFocus_MessageData : CMessageData
{
	public CAbility m_TargetingAbility;

	public bool m_IsPositive;

	public bool m_ObjectiveRelated;

	public CActor m_CameraFocusActor;

	public bool m_CanUndo = true;

	public CActorIsSelectingTargetingFocus_MessageData(CActor actorSpawningMessage)
		: base(MessageType.ActorIsSelectingTargetingFocus, actorSpawningMessage)
	{
	}
}
