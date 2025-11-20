using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIActiveAbilityDescription : MonoBehaviour
{
	[SerializeField]
	private TMP_Text descriptionText;

	[SerializeField]
	private GameObject summonHolder;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image durationImage;

	[SerializeField]
	private Image lostImage;

	[SerializeField]
	private TMP_Text instancesText;

	public void Init(string description, string icon, CActiveBonus.EActiveBonusDurationType duration, DiscardType discardType, int instances = 1, CardLayoutGroup.SummonLayout summonLayout = null, int summonID = 0)
	{
		if (description.IsNullOrEmpty())
		{
			descriptionText.gameObject.SetActive(value: false);
		}
		else
		{
			descriptionText.gameObject.SetActive(value: true);
			descriptionText.text = description;
		}
		if (string.IsNullOrEmpty(icon))
		{
			iconImage.enabled = false;
		}
		else
		{
			iconImage.enabled = true;
			iconImage.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(icon);
		}
		if (duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			durationImage.gameObject.SetActive(value: false);
		}
		else
		{
			durationImage.gameObject.SetActive(value: true);
			durationImage.sprite = UIInfoTools.Instance.GetActiveAbilityDurationIcon(duration);
		}
		if (summonLayout != null && summonHolder != null)
		{
			foreach (Transform item in summonHolder.transform)
			{
				Object.Destroy(item.gameObject);
			}
			summonHolder.SetActive(value: true);
			CreateLayout.CreateSummon(summonLayout, summonHolder.transform, isItemCard: false, 0, null, leftAlignSummonName: true, useDataLocKey: false, summonID);
		}
		else if (summonHolder != null)
		{
			summonHolder.SetActive(value: false);
		}
		lostImage.enabled = discardType == DiscardType.Lost || discardType == DiscardType.PermanentlyLost;
		instancesText.gameObject.SetActive(instances > 1);
		instancesText.text = "x" + instances;
	}
}
