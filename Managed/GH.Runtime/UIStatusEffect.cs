using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatusEffect : MonoBehaviour
{
	[SerializeField]
	private TMP_Text effectName;

	[SerializeField]
	private TMP_Text effectDescription;

	[SerializeField]
	private Image effectIcon;

	[SerializeField]
	private Sprite[] effectSprites;

	public void Initialize(string effectLocKey, Color nameColor)
	{
		effectName.text = LocalizationManager.GetTranslation(effectLocKey);
		effectName.color = nameColor;
		effectDescription.gameObject.SetActive(value: false);
		Sprite sprite = effectSprites.GetSprite(effectLocKey, toLower: true, useDefaultWhenMissing: false);
		if (sprite == null)
		{
			sprite = UIInfoTools.Instance.GetActiveBonusIconFromDLC("AA_" + effectLocKey);
		}
		effectIcon.sprite = sprite;
	}

	public void Initialize(CCondition.ENegativeCondition condition)
	{
		effectName.text = LocalizationManager.GetTranslation(condition.ToString());
		effectName.color = UIInfoTools.Instance.negativeStatusEffectColor;
		effectDescription.gameObject.SetActive(value: false);
		Sprite sprite = effectSprites.GetSprite(condition.ToString(), toLower: true, useDefaultWhenMissing: false);
		if (sprite == null)
		{
			sprite = UIInfoTools.Instance.GetActiveBonusIconFromDLC($"AA_{condition}");
		}
		effectIcon.sprite = sprite;
	}

	public void Initialize(string customEffectName, Sprite customEffectSprite, Color customEffectColor, string customEffectDescription = "")
	{
		if (customEffectName == "")
		{
			effectName.gameObject.SetActive(value: false);
		}
		else
		{
			effectName.text = CreateLayout.LocaliseText(customEffectName);
		}
		effectIcon.sprite = customEffectSprite;
		effectName.color = customEffectColor;
		if (customEffectDescription == "")
		{
			effectDescription.gameObject.SetActive(value: false);
		}
		else
		{
			effectDescription.text = CreateLayout.LocaliseText(customEffectDescription);
		}
	}
}
