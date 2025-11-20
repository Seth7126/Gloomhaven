using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UICampaignCharacterClassInformation : UICharacterClassInformation
{
	[SerializeField]
	private TextMeshProUGUI gold;

	[SerializeField]
	private UIPersonalQuestState personalQuestInfo;

	public override void Display(CMapCharacter characterData)
	{
		base.Display(characterData);
		if (gold != null)
		{
			gold.text = characterData.CharacterGold.ToString();
		}
		if (personalQuestInfo != null)
		{
			if (characterData.PersonalQuest == null)
			{
				personalQuestInfo.gameObject.SetActive(value: false);
				return;
			}
			personalQuestInfo.SetPersonalQuest(characterData.PersonalQuest);
			personalQuestInfo.gameObject.SetActive(value: true);
		}
	}
}
