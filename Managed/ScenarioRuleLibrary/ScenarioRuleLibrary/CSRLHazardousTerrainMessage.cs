namespace ScenarioRuleLibrary;

internal class CSRLHazardousTerrainMessage : ScenarioRuleClient.CSRLMessage
{
	public CObjectProp m_HazardousTerrainProp;

	public CActor m_Actor;

	public CSRLHazardousTerrainMessage(CObjectProp hazardProp, CActor actor)
		: base(ScenarioRuleClient.EMessageType.EACTIVATEHAZARDOUSTERRAINMESSAGE)
	{
		m_HazardousTerrainProp = hazardProp;
		m_Actor = actor;
	}
}
