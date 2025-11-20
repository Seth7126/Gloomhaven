using System.Collections.Generic;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterCreatorClassInformation : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI characterName;

	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	private TextMeshProUGUI health;

	[SerializeField]
	private TextMeshProUGUI abilities;

	[SerializeField]
	private List<Image> characterColorMasks;

	[SerializeField]
	private TextMeshProUGUI perks;

	[SerializeField]
	private TextLocalizedListener characterDescription;

	[SerializeField]
	private TextLocalizedListener difficulty;

	[SerializeField]
	private TextLocalizedListener role;

	[SerializeField]
	private TextLocalizedListener strengths;

	[SerializeField]
	private TextLocalizedListener weaknesses;

	public void Display(ICharacterCreatorClass characterData)
	{
		characterName.text = LocalizationManager.GetTranslation(characterData.Data.LocKey);
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(characterData.Data.Model, characterData.Data.CustomCharacterConfig);
		foreach (Image characterColorMask in characterColorMasks)
		{
			characterColorMask.color = characterColor;
		}
		health.text = characterData.Health.ToString();
		abilities.text = characterData.OwnedAbilityCards.Count.ToString();
		characterIcon.sprite = UIInfoTools.Instance.GetCharacterMarker(characterData.Data);
		perks.text = characterData.StartingPerks.ToString();
		characterDescription.SetTextKey(characterData.Data.Adventure_Description);
		difficulty.SetTextKey(characterData.Data.Difficulty);
		role.SetTextKey(characterData.Data.Role);
		strengths.SetTextKey(characterData.Data.Strengths);
		weaknesses.SetTextKey(characterData.Data.Weaknesses);
	}
}
