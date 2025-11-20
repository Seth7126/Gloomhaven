using MapRuleLibrary.MapState;

namespace MapRuleLibrary.Client;

public class CEnterScenario_MapClientMessage : CMapClientMessage
{
	public CMapScenarioState m_ScenarioLocation;

	public CEnterScenario_MapClientMessage(CMapScenarioState scenarioLocation)
		: base(EMapClientMessageType.EnterScenario)
	{
		m_ScenarioLocation = scenarioLocation;
	}
}
