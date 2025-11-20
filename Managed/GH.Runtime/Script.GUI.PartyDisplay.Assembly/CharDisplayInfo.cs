using ScenarioRuleLibrary;

namespace Script.GUI.PartyDisplay.Assembly;

public class CharDisplayInfo
{
	public CharacterManager CharacterManager { get; set; }

	public CActor.EType EType { get; set; }

	public string Character { get; set; }

	public string Skin { get; set; }
}
