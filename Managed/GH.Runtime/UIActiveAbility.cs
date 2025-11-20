using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class UIActiveAbility : MonoBehaviour
{
	[SerializeField]
	private TMP_Text abilityName;

	[SerializeField]
	private UIActiveAbilityDescription activeBonusDescription;

	[SerializeField]
	private GameObject abilityDescriptionHolder;

	[SerializeField]
	private PersistentAbilityTracker abilityTracker;

	[SerializeField]
	private UIActiveAbilityDescription activeBonusDescriptionPrefab;

	public CActiveBonus ActiveBonus;

	private List<UIActiveAbilityDescription> extraDescriptionsUI = new List<UIActiveAbilityDescription>();

	public PersistentAbilityTracker Tracker => abilityTracker;

	public void Initialize(CActiveBonus activeBonus, int instances = 1, bool forceGeneration = false)
	{
		if (ActiveBonus != activeBonus)
		{
			ActiveBonus = activeBonus;
			GenerateDesriptionLayout(instances, forceGeneration);
			if (ActiveBonus.Layout == null)
			{
				base.gameObject.SetActive(value: false);
				HelperTools.NormalizePool(ref extraDescriptionsUI, activeBonusDescriptionPrefab.gameObject, abilityDescriptionHolder.transform, 0);
			}
			else
			{
				base.gameObject.SetActive(value: true);
				RefreshTracker();
			}
		}
	}

	public void RefreshTracker()
	{
		if (abilityTracker != null)
		{
			abilityTracker.Init(ActiveBonus);
		}
	}

	private void GenerateDesriptionLayout(int instances, bool forceGeneration = false)
	{
		string empty = string.Empty;
		empty = CheckForLayoutToGenerate(ActiveBonus);
		if (ActiveBonus.Layout == null || (ActiveBonus.Ability.ActiveBonusData.Hidden && !forceGeneration))
		{
			return;
		}
		int count = ActiveBonus.Layout.IconNames.Count;
		int count2 = ActiveBonus.Layout.ListLayouts.Count;
		int num = Mathf.Max(count, count2);
		if (empty.IsNullOrEmpty() && ActiveBonus.Layout.CardName.IsNOTNullOrEmpty())
		{
			empty = LocalizationManager.GetTranslation(ActiveBonus.Layout.CardName);
		}
		abilityName.text = empty;
		abilityName.gameObject.SetActive(empty.IsNOTNullOrEmpty());
		CardLayoutGroup.SummonLayout summonLayout = null;
		if (ActiveBonus?.Layout?.Layout?.Collection != null)
		{
			summonLayout = ActiveBonus.Layout.Layout.Collection.SingleOrDefault((CardLayoutGroup x) => x.Summon != null)?.Summon;
		}
		string description = ActivePropertyLookup(LocalisationAndPropertyLookup(ActiveBonus.Layout.ListLayouts.FirstOrDefault()));
		int summonID = 0;
		if (ActiveBonus.Actor is CHeroSummonActor cHeroSummonActor)
		{
			summonID = cHeroSummonActor.ID;
		}
		activeBonusDescription.Init(description, ActiveBonus.Layout.IconNames.FirstOrDefault(), ActiveBonus.Duration, ActiveBonus.Layout.Discard, instances, summonLayout, summonID);
		HelperTools.NormalizePool(ref extraDescriptionsUI, activeBonusDescriptionPrefab.gameObject, abilityDescriptionHolder.transform, Mathf.Max(num - 1, 0));
		abilityDescriptionHolder.SetActive(num > 1);
		for (int num2 = 1; num2 < num; num2++)
		{
			string description2 = ((num2 < count2) ? ActivePropertyLookup(LocalisationAndPropertyLookup(ActiveBonus.Layout.ListLayouts[num2])) : string.Empty);
			extraDescriptionsUI[num2 - 1].Init(description2, (num2 < count) ? ActiveBonus.Layout.IconNames[num2] : null, ActiveBonus.Duration, ActiveBonus.Layout.Discard);
		}
	}

	private string ActivePropertyLookup(string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == ActiveBonus.Ability.Name)
			{
				num += ActiveBonus.Ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	private string LocalisationAndPropertyLookup(string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', ActiveBonus.Ability.Strength.ToString()) : desc);
		return desc;
	}

	public static string CheckForLayoutToGenerate(CActiveBonus activeBonus)
	{
		string result = string.Empty;
		if (activeBonus.Layout == null || (activeBonus.Layout.Layout == null && activeBonus.IsDoom) || activeBonus.Ability.ActiveBonusData.GroupID != null)
		{
			activeBonus.Layout = new ActiveBonusLayout(string.Empty, new List<string>(), null, null, new List<string>(), null, new List<int>(), DiscardType.Discard);
			if (activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1 != null && activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item2 != null)
			{
				if (activeBonus.Ability.ActiveBonusData.GroupID != null && activeBonus.Actor != null)
				{
					int num = 0;
					foreach (CActiveBonus item2 in (from x in CActiveBonus.FindApplicableActiveBonuses(activeBonus.Actor)
						where x?.Ability?.ActiveBonusData?.GroupID == activeBonus.Ability.ActiveBonusData.GroupID
						select x).ToList())
					{
						AbilityData.StatIsBasedOnXData statIsBasedOnXData = item2.Ability.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
						num = ((statIsBasedOnXData == null) ? (num + item2.Ability.Strength) : (num + item2.Ability.GetStatIsBasedOnXValue(item2.Actor, statIsBasedOnXData, item2.Ability.AbilityFilter)));
					}
					string format = CreateLayout.LocaliseText(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1);
					format = string.Format(format, num);
					activeBonus.Layout.ListLayouts.Add(format);
				}
				else
				{
					activeBonus.Layout.ListLayouts.Add(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1);
				}
				activeBonus.Layout.IconNames.Add(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item2);
			}
			else if (activeBonus.Actor is CEnemyActor && !activeBonus.IsDoom)
			{
				if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Shield)
				{
					int strengthOfActiveBonus = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.Shield);
					activeBonus.Layout.IconNames.Add("Shield");
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$Shield$ ") + strengthOfActiveBonus);
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Retaliate)
				{
					int strengthOfActiveBonus2 = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.Retaliate);
					string text = CreateLayout.LocaliseText("$Retaliate$ ") + strengthOfActiveBonus2;
					if ((activeBonus.Ability as CAbilityRetaliate).RetaliateRange > 1)
					{
						int retaliateRange = (activeBonus.Ability as CAbilityRetaliate).RetaliateRange;
						text = ((retaliateRange != 99999) ? (text + "\n" + CreateLayout.LocaliseText("$Range$ ") + retaliateRange) : (text + "\n" + CreateLayout.LocaliseText("$Any$ $Range$")));
					}
					activeBonus.Layout.IconNames.Add("Retaliate");
					activeBonus.Layout.ListLayouts.Add(text);
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.AttackersGainDisadvantage)
				{
					activeBonus.Layout.IconNames.Add("AA_Disadvantage");
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$AttackersGainDisadvantage$ "));
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack || activeBonus.Ability.AbilityType == CAbility.EAbilityType.Move)
				{
					string cardName = string.Empty;
					string item = ((activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack) ? "Attack" : "Move");
					string empty = string.Empty;
					if (activeBonus.BaseCard != null && activeBonus.BaseCard.CardType == CBaseCard.ECardType.Item && activeBonus.Caster != activeBonus.Actor && activeBonus.Ability.AbilityText.IsNOTNullOrEmpty())
					{
						cardName = CreateLayout.LocaliseText(activeBonus.BaseCard.Name);
						item = activeBonus.Caster.Class.DefaultModel;
						empty = string.Format(CreateLayout.LocaliseText(activeBonus.Ability.AbilityText), CreateLayout.LocaliseText(activeBonus.Caster.Class.LocKey));
					}
					else if (activeBonus.Ability.AbilityText.IsNOTNullOrEmpty())
					{
						empty = CreateLayout.LocaliseText(activeBonus.Ability.AbilityText);
					}
					else
					{
						string obj = ((activeBonus.Ability.Strength < 0) ? "-" : "+");
						string text2 = CreateLayout.LocaliseText((activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack) ? " $Attack$ " : " $Move$ ");
						string text3 = string.Empty;
						if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack)
						{
							CAbility ability = activeBonus.Ability;
							if (ability != null && ability.ActiveBonusData?.AttackType == CAbility.EAttackType.Melee)
							{
								text3 = CreateLayout.LocaliseText("$To_AllMeleeAttacks$");
							}
							else
							{
								CAbility ability2 = activeBonus.Ability;
								text3 = ((ability2 == null || ability2.ActiveBonusData?.AttackType != CAbility.EAttackType.Ranged) ? CreateLayout.LocaliseText("$To_AllAttacks$") : CreateLayout.LocaliseText("$To_AllRangedAttacks$"));
							}
						}
						empty = obj + activeBonus.Ability.Strength + text2 + text3;
					}
					activeBonus.Layout.CardName = cardName;
					activeBonus.Layout.IconNames.Add(item);
					activeBonus.Layout.ListLayouts.Add(empty);
				}
			}
			else if (activeBonus.IsAugment)
			{
				activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
				activeBonus.Layout.IconNames.Add("Mindthief");
				activeBonus.Layout.ListLayouts.Add(activeBonus.Ability?.Augment?.ActiveBonusLayout ?? activeBonus.Actor.Augments[0].ActiveBonusLayout);
				result = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#A7B3D2>$Augment$</color> <sprite name=Augment>");
			}
			else if (activeBonus.IsSong)
			{
				if (activeBonus.Ability?.Song != null)
				{
					activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
					activeBonus.Layout.IconNames.Add("Soothsinger");
					activeBonus.Layout.ListLayouts.Add(activeBonus.Ability.Song.ActiveBonusLayout);
				}
				result = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#EBB0AA>$Song$</color> <sprite name=Song>");
			}
			else if (activeBonus.IsDoom)
			{
				CDoom cDoom = activeBonus?.Caster?.Dooms?.SingleOrDefault((CDoom x) => x.DoomAbilities.Any((CAbility y) => y == activeBonus.Ability));
				if (cDoom != null)
				{
					activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
					activeBonus.Layout.IconNames.Add("Doomstalker");
					activeBonus.Layout.ListLayouts.Add(cDoom.ActiveBonusLayout);
					if (activeBonus?.Ability?.ActiveBonusData?.Tracker != null)
					{
						activeBonus.Layout.TrackerPattern.AddRange(activeBonus.Ability.ActiveBonusData.Tracker);
					}
				}
				result = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#39BEED>$Doom$</color> <sprite name=Doom>");
			}
			else if (activeBonus.Actor is CHeroSummonActor cHeroSummonActor)
			{
				if (cHeroSummonActor.SummonData.OnSummonAbilities != null && cHeroSummonActor.SummonData.OnSummonAbilities.Any((CAbility x) => x.ID == activeBonus.Ability.ID && activeBonus.Ability.AbilityText.IsNOTNullOrEmpty()))
				{
					activeBonus.Layout.CardName = cHeroSummonActor.ActorLocKey();
					activeBonus.Layout.IconNames.Add((activeBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus) ? cAbilityAddActiveBonus.AddAbility.AbilityType.ToString() : activeBonus.Ability.AbilityType.ToString());
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText(activeBonus.Ability.AbilityText));
				}
				result = CreateLayout.LocaliseText(cHeroSummonActor.ActorLocKey());
			}
			else
			{
				activeBonus.Layout = null;
			}
		}
		return result;
	}

	private static int GetStrengthOfActiveBonus(CActiveBonus bonus, CAbility.EAbilityType type)
	{
		int num = 0;
		AbilityData.StatIsBasedOnXData statIsBasedOnXData = bonus.Ability.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
		if (statIsBasedOnXData != null)
		{
			return num + bonus.Ability.GetStatIsBasedOnXValue(bonus.Actor, statIsBasedOnXData, bonus.Ability.AbilityFilter);
		}
		return bonus.Ability.Strength;
	}
}
