using System;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UIRetirementCharacterInformation : UICharacterInformation
{
	[SerializeField]
	private TextMeshProUGUI damageDone;

	[SerializeField]
	private TextMeshProUGUI kills;

	[SerializeField]
	private TextMeshProUGUI exhaustions;

	[SerializeField]
	private TextMeshProUGUI healingDone;

	[SerializeField]
	private TextMeshProUGUI winrate;

	public override void Display(CMapCharacter characterData)
	{
		damageDone.text = characterData.PlayerRecords.LifetimeTotal.DamageDone.ToString();
		kills.text = characterData.PlayerRecords.LifetimeTotal.Kills.ToString();
		healingDone.text = characterData.PlayerRecords.LifetimeTotal.HealingDone.ToString();
		exhaustions.text = characterData.PlayerRecords.LifetimeTotal.Exhaustions.ToString();
		winrate.text = ((characterData.PlayerRecords.LifetimeTotal.ScenariosPlayed == 0) ? "0%" : string.Format($"{Math.Round((float)characterData.PlayerRecords.LifetimeTotal.ScenariosWon / (float)characterData.PlayerRecords.LifetimeTotal.ScenariosPlayed * 100f, 2)}%"));
	}
}
