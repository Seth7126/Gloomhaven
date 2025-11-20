using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;

namespace Assets.Script.GUI.Quest;

public class QuestLocation
{
	public EQuestAreaType Area { get; private set; }

	public EQuestIconType Icon { get; private set; }

	public ECharacter RequiredCharacter { get; private set; }

	public QuestLocation(EQuestAreaType area, EQuestIconType icon, ECharacter requiredCharacter = ECharacter.None)
	{
		Area = area;
		Icon = icon;
		RequiredCharacter = requiredCharacter;
	}
}
