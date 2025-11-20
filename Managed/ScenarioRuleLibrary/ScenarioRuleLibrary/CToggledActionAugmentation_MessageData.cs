namespace ScenarioRuleLibrary;

public class CToggledActionAugmentation_MessageData : CMessageData
{
	public CAbility m_CurrentAbilityAfterToggling;

	public CToggledActionAugmentation_MessageData()
		: base(MessageType.ToggledActionAugmentation, null)
	{
	}
}
