using System.Collections.Generic;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentAbilityTracker : MonoBehaviour
{
	[SerializeField]
	private Transform stepsHolder;

	[SerializeField]
	private GameObject elementPrefab;

	[SerializeField]
	private Image startImage;

	[SerializeField]
	public Image endingImage;

	[SerializeField]
	private Sprite[] endSprites;

	[SerializeField]
	private TextMeshProUGUI title;

	private List<PersistentAbilityTrackerElement> elements = new List<PersistentAbilityTrackerElement>();

	private CActiveBonus m_ActiveBonus;

	public void Init(CActiveBonus activeBonus)
	{
		HelperTools.NormalizePool(ref elements, elementPrefab, stepsHolder, activeBonus.Layout?.TrackerPattern.Count ?? 0);
		m_ActiveBonus = activeBonus;
		title.text = LocalizationManager.GetTranslation((activeBonus.Ability.AbilityType == CAbility.EAbilityType.Shield) ? "Damage" : "XP") + ":";
		if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			startImage.gameObject.SetActive(value: false);
		}
		else
		{
			startImage.gameObject.SetActive(value: true);
			startImage.sprite = UIInfoTools.Instance.GetActiveAbilityDurationIcon(activeBonus.Duration);
		}
		if (activeBonus.Layout != null && activeBonus.Layout.TrackerPattern.Count > 0)
		{
			UpdateElements(activeBonus.TrackerIndex, activeBonus.Layout.TrackerPattern);
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		if (activeBonus.Layout != null && activeBonus.Layout.Discard != DiscardType.None)
		{
			Sprite sprite = endSprites[(int)(activeBonus.Layout.Discard - 1)];
			endingImage.sprite = sprite;
			endingImage.gameObject.SetActive(sprite != null);
		}
		else
		{
			endingImage.gameObject.SetActive(value: false);
		}
	}

	public void OnActiveBonusTriggered()
	{
		if (m_ActiveBonus.Layout.TrackerPattern.Count > 0)
		{
			UpdateElements(m_ActiveBonus.TrackerIndex, m_ActiveBonus.Layout.TrackerPattern);
		}
	}

	private void UpdateElements(int selectedElement, List<int> pattern)
	{
		int num = 0;
		foreach (int item in pattern)
		{
			if (m_ActiveBonus.Ability.AbilityType == CAbility.EAbilityType.Shield)
			{
				elements[num].SetElementShield(selectedElement == num, num < selectedElement);
			}
			else
			{
				elements[num].SetElementXP(item, selectedElement == num, num < selectedElement);
			}
			num++;
		}
	}
}
