using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIPersonalQuestObjectiveTracker : UIPersonalQuestStateInfo
{
	[SerializeField]
	protected TextLocalizedListener title;

	[SerializeField]
	protected FlowLayoutGroup progressContainer;

	[SerializeField]
	protected List<UIPersonalQuestObjectiveTrackerTarget> progressTextPool;

	[SerializeField]
	private int maxColumns = 3;

	public override void SetPersonalQuest(CPersonalQuestState personalQuest)
	{
		List<string> list = PersonalQuestObjectiveUtils.CalculateObjectives(personalQuest.PersonalQuestConditionState);
		if (list == null || (list.Count == 0 && personalQuest.ParentPersonalQuestData.LocalisedObjectiveNotProgressed.IsNullOrEmpty()))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		string localisedObjectiveTitle = personalQuest.ParentPersonalQuestData.LocalisedObjectiveTitle;
		if (localisedObjectiveTitle.IsNullOrEmpty())
		{
			title.gameObject.SetActive(value: false);
		}
		else
		{
			title.SetTextKey(localisedObjectiveTitle);
			title.gameObject.SetActive(value: true);
		}
		if (list.Count == 0)
		{
			BuildObjectives(UIInfoTools.Instance.greyedOutTextColor, LocalizationManager.GetTranslation(personalQuest.ParentPersonalQuestData.LocalisedObjectiveNotProgressed));
		}
		else
		{
			BuildObjectives(list, UIInfoTools.Instance.mainColor);
		}
		base.gameObject.SetActive(value: true);
	}

	private void BuildObjectives(Color textColor, params string[] texts)
	{
		BuildObjectives(texts, textColor);
	}

	private void BuildObjectives(IEnumerable<string> texts, Color textColor)
	{
		int num = texts.Count();
		HelperTools.NormalizePool(ref progressTextPool, progressTextPool[0].gameObject, progressContainer.transform, num);
		float num2 = (progressContainer.transform as RectTransform).sizeDelta.x - (float)progressContainer.padding.left - (float)progressContainer.padding.right;
		int num3 = 3;
		if (num > 6)
		{
			num3 = 5;
		}
		int num4 = Mathf.Min(maxColumns, Mathf.CeilToInt((float)num / (float)num3));
		float size = Mathf.Floor((num2 - progressContainer.SpacingX * (float)(num4 - 1)) / (float)num4);
		int num5 = 0;
		foreach (string text in texts)
		{
			progressTextPool[num5].SetText(text, textColor, size);
			num5++;
		}
	}
}
