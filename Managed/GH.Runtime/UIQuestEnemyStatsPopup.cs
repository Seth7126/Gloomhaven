using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestEnemyStatsPopup : UILocalTooltip
{
	[Header("Information")]
	[SerializeField]
	private TextLocalizedListener enemyName;

	[SerializeField]
	private Graphic eliteMask;

	[Space]
	[SerializeField]
	private TextMeshProUGUI levelText;

	[Header("Basic Stats")]
	[SerializeField]
	private UIQuestEnemyStat healthStat;

	[SerializeField]
	private UIQuestEnemyStat movementStat;

	[SerializeField]
	private UIQuestEnemyStat attackStat;

	[SerializeField]
	private UIQuestEnemyStat rangeStat;

	[Header("Extra Stats")]
	[SerializeField]
	private string innateStatFormat = "{0} ($Innate$)";

	[SerializeField]
	private string immunityStatFormat = "$ImmunityTo$ {0}";

	[SerializeField]
	private List<UIQuestEnemyStat> extraStatPool;

	[Header("Stat Colors")]
	[SerializeField]
	private Color basicStatColor;

	[SerializeField]
	private Color disabledStatColor;

	[SerializeField]
	private Color extraStatColor;

	private const string IGNORED_STAT = "-";

	private CMonsterClass monster;

	private Color levelTextColor;

	private Color nameTextColor;

	private void Awake()
	{
		levelTextColor = levelText.color;
		nameTextColor = enemyName.Text.color;
	}

	public void Show(CMonsterClass monster, int monsterLevel, RectTransform target, bool focused = true)
	{
		Show(monster, monster.GetMonsterStatsData(monsterLevel), target, focused);
	}

	public void Show(CMonsterClass monster, BaseStats statsData, RectTransform target, bool focused = true)
	{
		int num = statsData.Health;
		if (monster.StatIsBasedOnXEntries != null)
		{
			foreach (AbilityData.StatIsBasedOnXData item in statsData.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData it) => it.BaseStatType == EMonsterBaseStats.Health && it.BasedOn == CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount))
			{
				int num2 = (int)Math.Round((float)Math.Max(AdventureState.MapState.MapParty.SelectedCharacters.Count(), 1) * item.Multiplier, MidpointRounding.AwayFromZero);
				num = Math.Max(0, (item.AddTo ? num : 0) + num2);
			}
		}
		healthStat.SetStatValue(num.ToString());
		if (this.monster != monster)
		{
			this.monster = monster;
			eliteMask.gameObject.SetActive(monster.NonEliteVariant != null && monster.NonEliteVariant.DefaultModel != monster.DefaultModel);
			enemyName.SetTextKey(monster.LocKey);
			levelText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), statsData.Level);
			movementStat.SetStatValue(statsData.Move.ToString());
			attackStat.SetStatValue(statsData.Attack.ToString());
			rangeStat.SetStatValue((statsData.Range > 1) ? statsData.Range.ToString() : "-");
			rangeStat.SetColor((statsData.Range > 1) ? basicStatColor : disabledStatColor, (statsData.Range > 1) ? basicStatColor : disabledStatColor);
			CreateExtraStats(statsData);
		}
		SetTarget(target);
		Show();
		SetFocused(focused);
	}

	public void SetFocused(bool focused)
	{
		if (!base.IsShown)
		{
			enemyName.Text.color = (focused ? nameTextColor : UIInfoTools.Instance.greyedOutTextColor);
			levelText.color = (focused ? levelTextColor : UIInfoTools.Instance.greyedOutTextColor);
			eliteMask.material = (focused ? null : UIInfoTools.Instance.greyedOutMaterial);
			healthStat.SetFocused(focused);
			movementStat.SetFocused(focused);
			attackStat.SetFocused(focused);
			rangeStat.SetFocused(focused);
			for (int i = 0; i < extraStatPool.Count && extraStatPool[i].gameObject.activeSelf; i++)
			{
				extraStatPool[i].SetFocused(focused);
			}
		}
	}

	private void CreateExtraStats(BaseStats stats)
	{
		int num = 0;
		if (stats.Advantage)
		{
			CreateInnateStat(num++, "$Advantage$", UIInfoTools.Instance.GetActiveAbilityIcon("Advantage"));
		}
		if (stats.AttackersGainDisadvantage)
		{
			CreateInnateStat(num++, "$AttackersGainDisadvantage$", UIInfoTools.Instance.GetActiveAbilityIcon("Disadvantage"));
		}
		if (stats.Flying)
		{
			CreateInnateStat(num++, "$Flying$", UIInfoTools.Instance.GetActiveAbilityIcon("Flying"));
		}
		if (stats.Target > 1)
		{
			CreateInnateStat(num++, "$Target$", UIInfoTools.Instance.GetActiveAbilityIcon("Target"), stats.Target);
		}
		if (stats.Shield > 0)
		{
			CreateInnateStat(num++, "$Shield$", UIInfoTools.Instance.GetActiveAbilityIcon("Shield"), stats.Shield);
		}
		if (stats.Retaliate > 0)
		{
			CreateInnateStat(num++, "$Retaliate$", UIInfoTools.Instance.GetActiveAbilityIcon("Retaliate"), stats.Retaliate);
		}
		if (stats.Pierce > 0)
		{
			CreateInnateStat(num++, "$Pierce$", UIInfoTools.Instance.GetActiveAbilityIcon("Pierce"), stats.Pierce);
		}
		foreach (CAbility.EAbilityType immunity in stats.Immunities)
		{
			UIQuestEnemyStat extraStatSlot = GetExtraStatSlot(num++);
			extraStatSlot.SetStat(string.Format(immunityStatFormat, LocalizationManager.GetTranslation(immunity.ToString())), UIInfoTools.Instance.GetImmunityIcon(immunity.ToString()));
			extraStatSlot.SetColor(extraStatColor);
		}
		for (int i = num; i < extraStatPool.Count && extraStatPool[i].gameObject.activeSelf; i++)
		{
			extraStatPool[i].gameObject.SetActive(value: false);
		}
	}

	private void CreateInnateStat(int position, string statName, Sprite icon, int? value = null)
	{
		UIQuestEnemyStat extraStatSlot = GetExtraStatSlot(position);
		extraStatSlot.SetStat(string.Format(innateStatFormat, statName), icon, value?.ToString());
		extraStatSlot.SetColor(extraStatColor, extraStatColor);
	}

	private UIQuestEnemyStat GetExtraStatSlot(int position)
	{
		if (position >= extraStatPool.Count)
		{
			extraStatPool.Add(UnityEngine.Object.Instantiate(extraStatPool[0], extraStatPool[0].transform.parent));
		}
		UIQuestEnemyStat uIQuestEnemyStat = extraStatPool[position];
		uIQuestEnemyStat.gameObject.SetActive(value: true);
		return uIQuestEnemyStat;
	}
}
