using UnityEngine;

public interface ITownRecordCharacter
{
	string CharacterName { get; }

	Sprite Icon { get; }

	int DamageDone { get; }

	int HealingDone { get; }

	int Kills { get; }

	int Exhausitons { get; }

	float Winrate { get; }

	bool IsRetired { get; }
}
