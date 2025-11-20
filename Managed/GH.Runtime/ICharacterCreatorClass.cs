using System.Collections.Generic;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public interface ICharacterCreatorClass
{
	string ID { get; }

	bool IsNew { get; }

	List<CAbilityCard> OwnedAbilityCards { get; }

	CharacterYMLData Data { get; }

	int Health { get; }

	int StartingPerks { get; }
}
