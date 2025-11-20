using GLOOM;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdventureCharacterXPInformation : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI xpHighlightText;

	[SerializeField]
	private Image xpBar;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private GameObject xpPanel;

	public void Setup(CMapCharacter character)
	{
		if (character.Level + 1 >= character.XPTable.Count)
		{
			xpPanel.SetActive(value: false);
		}
		else
		{
			int num = character.EXP - character.GetXPThreshold(character.Level);
			int num2 = character.GetXPThreshold(character.Level + 1) - character.GetXPThreshold(character.Level);
			xpHighlightText.text = num + "/" + num2;
			xpBar.fillAmount = Mathf.Clamp01((float)num / (float)num2);
			xpPanel.SetActive(value: true);
		}
		levelText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), character.Level);
	}
}
