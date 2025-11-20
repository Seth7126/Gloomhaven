using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;

namespace MapRuleLibrary.Client;

public class CSoloScenarioImportCompletion_MapDLLMessage : CMapDLLMessage
{
	public CQuestState m_QuestState;

	public CMapCharacter m_SoloCharacter;

	public int m_ScenarioLevel;

	public CSoloScenarioImportCompletion_MapDLLMessage(CQuestState questState, CMapCharacter soloCharacter, int scenarioLevel)
		: base(EMapDLLMessageType.SoloScenarioImportCompletion)
	{
		m_QuestState = questState;
		m_SoloCharacter = soloCharacter;
		m_ScenarioLevel = scenarioLevel;
	}
}
