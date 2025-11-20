using System;
using System.Collections.Generic;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using Assets.Script.GUI.Quest;
using GLOOM;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestDescription : MonoBehaviour
{
	[Serializable]
	private class TextGhosted
	{
		public TextMeshProUGUI text;

		public Color defaultColor;

		public Color incompleteColor;
	}

	[SerializeField]
	private Image icon;

	[Space]
	[SerializeField]
	private TextLocalizedListener titleText;

	[SerializeField]
	private Color titleDefaultColor;

	[SerializeField]
	private Color titleLockedColor;

	[Space]
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[Space]
	[SerializeField]
	private TextMeshProUGUI locationText;

	[Space]
	[SerializeField]
	private List<GameObject> completedMasks;

	[SerializeField]
	private List<GameObject> incompletedMasks;

	[Space]
	[SerializeField]
	private TextMeshProUGUI goalText;

	[SerializeField]
	private Color goalTextColor;

	[SerializeField]
	private Color goalLockedTextColor;

	[SerializeField]
	private Color goalSubtextColor;

	[Space]
	[SerializeField]
	private TextMeshProUGUI specialRulesText;

	[SerializeField]
	private Color specialRulesDefaultColor;

	[SerializeField]
	private Color specialRulesLockedColor;

	[SerializeField]
	private float indentSpecialRules = 10f;

	[Space]
	[SerializeField]
	private TextMeshProUGUI requirementsText;

	[Space]
	[SerializeField]
	private GameObject incompletedMask;

	[SerializeField]
	private List<Graphic> lockedGhostedGraphics;

	[SerializeField]
	private List<TextGhosted> lockedGhostedTexts;

	private IQuest quest;

	public void Setup(IQuest quest)
	{
		if (!object.Equals(this.quest, quest))
		{
			this.quest = quest;
			DecorateQuest(quest);
		}
		RefreshLocked();
	}

	private void DecorateQuest(IQuest quest)
	{
		titleText.SetTextKey(quest.LocalisedNameKey);
		descriptionText.text = quest.Description;
		icon.sprite = quest.Icon;
		bool flag = quest.IsCompleted();
		for (int i = 0; i < completedMasks.Count; i++)
		{
			completedMasks[i].SetActive(flag);
		}
		for (int j = 0; j < incompletedMasks.Count; j++)
		{
			incompletedMasks[j].SetActive(!flag);
		}
		DecorateSpecialRules(quest.SpecialRules);
		DecorateObjectives(quest.Objectives);
		DecorateLocation(quest.Location);
	}

	private void DecorateLocation(QuestLocation location, bool unlocked = true)
	{
		if (locationText == null || locationText.gameObject == null)
		{
			return;
		}
		if (location == null || location.Area == EQuestAreaType.None)
		{
			locationText.gameObject.SetActive(value: false);
			return;
		}
		EQuestIconType eQuestIconType = location.Icon;
		EQuestAreaType area = location.Area;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("<b><sprite name=\"QuestDescription_Location\" color=#{1}> <color=#{1}>{0}</color></b>", LocalizationManager.GetTranslation(area.ToString()), unlocked ? UIInfoTools.Instance.GetQuestAreaColor(area, eQuestIconType).ToHex() : locationText.color.ToHex());
		bool flag = eQuestIconType == EQuestIconType.Boss || eQuestIconType == EQuestIconType.JOTLBoss;
		bool flag2 = eQuestIconType == EQuestIconType.BossSide || eQuestIconType == EQuestIconType.JOTLBossSide;
		if (eQuestIconType.In(EQuestIconType.Side, EQuestIconType.BossSide, EQuestIconType.JOTLSide, EQuestIconType.JOTLBossSide))
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<sprite name=\"{1}\" color=#{2}> {0}", LocalizationManager.GetTranslation("GUI_SIDE_QUEST"), flag2 ? "QuestDescription_Side_Boss" : "QuestDescription_Side", locationText.color.ToHex());
			if (flag2)
			{
				stringBuilder.AppendFormat(", <b>{0}</b>", LocalizationManager.GetTranslation("Boss"));
			}
		}
		else if (eQuestIconType == EQuestIconType.None || flag)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<sprite name=\"{1}\" color=#{2}> {0}", LocalizationManager.GetTranslation("GUI_CORE_QUEST"), flag ? "QuestDescription_Core_Boss" : "QuestDescription_Core", locationText.color.ToHex());
			if (flag)
			{
				stringBuilder.AppendFormat(", <b>{0}</b>", LocalizationManager.GetTranslation("Boss"));
			}
		}
		else if (eQuestIconType == EQuestIconType.RequiredCharacter && location.RequiredCharacter != ECharacter.None)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<sprite name=\"AA_{1}\" color=#{2}> {0}", string.Format(LocalizationManager.GetTranslation("GUI_SOLO_QUEST"), LocalizationManager.GetTranslation(location.RequiredCharacter.ToString())), location.RequiredCharacter, locationText.color.ToHex());
		}
		locationText.text = stringBuilder.ToString();
		locationText.gameObject.SetActive(value: true);
	}

	private void DecorateSpecialRules(List<string> specialRules)
	{
		if (specialRules == null || specialRules.Count == 0)
		{
			specialRulesText.gameObject.SetActive(value: false);
			return;
		}
		specialRulesText.text = BuildIndentList(specialRules);
		specialRulesText.gameObject.SetActive(value: true);
	}

	private void DecorateObjectives(List<CObjective> objectives)
	{
		if (objectives == null || objectives.Count == 0)
		{
			goalText.gameObject.SetActive(value: false);
		}
		else
		{
			UpdateText(isDark: false, objectives);
		}
	}

	public void UpdateText(bool isDark, List<CObjective> objectives = null)
	{
		if (objectives != null || (quest != null && quest.Objectives != null))
		{
			if (objectives == null)
			{
				objectives = quest.Objectives;
			}
			string translation = LocalizationManager.GetTranslation("GUI_RESULTS_OBJECTIVE");
			string prefix = ((!isDark) ? ("<color=#" + goalTextColor.ToHex() + ">" + translation + "</color>") : translation);
			List<string> elements = objectives.ConvertAll((CObjective it) => isDark ? it.LocalizeText() : ("<color=#" + goalSubtextColor.ToHex() + ">" + it.LocalizeText() + "</color>"));
			goalText.text = BuildNumeratedList(prefix, elements);
			goalText.gameObject.SetActive(value: true);
		}
	}

	private string BuildIndentList(List<string> elements)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < elements.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("-<indent={0}>{1}</indent>", indentSpecialRules, elements[i].TrimEnd('\n'));
		}
		return stringBuilder.ToString();
	}

	private string BuildNumeratedList(string prefix, List<string> elements)
	{
		if (elements.Count == 1)
		{
			return string.Format(prefix + ": " + elements[0]);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < elements.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("{0} {1}: {2}", prefix, i + 1, elements[i].TrimEnd('\n'));
		}
		return stringBuilder.ToString();
	}

	public void RefreshLocked()
	{
		SetRequirements(quest.CheckRequirements());
	}

	public void SetRequirements(IRequirementCheckResult requirements)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (requirements.IsUnlocked() || Singleton<MapChoreographer>.Instance.ShowAllScenariosMode)
		{
			incompletedMask.SetActive(value: false);
			for (int i = 0; i < lockedGhostedGraphics.Count; i++)
			{
				lockedGhostedGraphics[i].material = null;
			}
			for (int j = 0; j < lockedGhostedTexts.Count; j++)
			{
				lockedGhostedTexts[j].text.color = lockedGhostedTexts[j].defaultColor;
			}
			icon.material = null;
			titleText.Text.color = titleDefaultColor;
			specialRulesText.color = specialRulesDefaultColor;
			goalText.color = goalTextColor;
			DecorateLocation(quest.Location);
		}
		else
		{
			stringBuilder.AppendLine(requirements.ToString($"-<indent={indentSpecialRules}>{{0}}</indent>"));
			incompletedMask.SetActive(!requirements.IsOnlyMissingCharacters());
			for (int k = 0; k < lockedGhostedGraphics.Count; k++)
			{
				lockedGhostedGraphics[k].material = UIInfoTools.Instance.greyedOutMaterial;
			}
			for (int l = 0; l < lockedGhostedTexts.Count; l++)
			{
				lockedGhostedTexts[l].text.color = lockedGhostedTexts[l].incompleteColor;
			}
			icon.material = UIInfoTools.Instance.greyedOutMaterial;
			titleText.Text.color = titleLockedColor;
			specialRulesText.color = specialRulesLockedColor;
			goalText.color = goalLockedTextColor;
			DecorateLocation(quest.Location, unlocked: false);
		}
		List<string> additionalInformation = quest.AdditionalInformation;
		if (additionalInformation != null)
		{
			for (int m = 0; m < additionalInformation.Count; m++)
			{
				stringBuilder.AppendLine($"<color=white>-</color><indent={indentSpecialRules}>{additionalInformation[m]}</indent>");
			}
		}
		if (stringBuilder.Length == 0)
		{
			requirementsText.gameObject.SetActive(value: false);
			return;
		}
		requirementsText.text = stringBuilder.ToString();
		requirementsText.gameObject.SetActive(value: true);
	}
}
