using ScenarioRuleLibrary;

public readonly struct CharacterClickedData
{
	public CPlayerActor CPlayerActor { get; }

	public CharacterSymbolTab CharacterSymbolTab { get; }

	public CharacterClickedData(CPlayerActor cPlayerActor, CharacterSymbolTab characterSymbolTab)
	{
		CPlayerActor = cPlayerActor;
		CharacterSymbolTab = characterSymbolTab;
	}
}
