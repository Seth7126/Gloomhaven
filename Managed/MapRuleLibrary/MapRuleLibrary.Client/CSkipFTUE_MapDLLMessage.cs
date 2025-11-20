namespace MapRuleLibrary.Client;

public class CSkipFTUE_MapDLLMessage : CMapDLLMessage
{
	public bool m_SkipTutorial;

	public bool m_SkipIntro;

	public CSkipFTUE_MapDLLMessage(bool skipTutorial, bool skipIntro)
		: base(EMapDLLMessageType.SkipFTUE)
	{
		m_SkipTutorial = skipTutorial;
		m_SkipIntro = skipIntro;
	}
}
