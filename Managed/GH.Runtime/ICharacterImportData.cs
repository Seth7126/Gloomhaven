using UnityEngine;

public interface ICharacterImportData
{
	string CharacterID { get; }

	string CharacterName { get; }

	Sprite CharacterIcon { get; }

	int Gold { get; }

	int XP { get; }

	Sprite ItemIcon { get; }

	string ItemLocKey { get; }

	string Id { get; }

	bool CanImport();

	void Import();
}
