using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;

public class UICampaignPartyAssemblyRosterInformation : UICharacterInformation
{
	[SerializeField]
	private TextMeshProUGUI characterName;

	public override void Display(CMapCharacter characterData)
	{
		if (characterData.DisplayCharacterName.IsNullOrEmpty())
		{
			characterName.text = LocalizationManager.GetTranslation(characterData.CharacterYMLData.LocKey);
		}
		else
		{
			characterName.SetTextCensored(characterData.DisplayCharacterName);
		}
	}
}
