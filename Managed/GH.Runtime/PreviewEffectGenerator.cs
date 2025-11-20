using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class PreviewEffectGenerator : Singleton<PreviewEffectGenerator>
{
	[SerializeField]
	private TMP_SpriteAsset iconSpriteAsset;

	private const string SCALAR_FORMAT = "X{0}";

	public static string GeneratePreview(ItemData data)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (data.ShieldValue > 0 && (data.Abilities == null || !data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Shield)))
		{
			if (data.ShieldValue == int.MaxValue)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.PreventDamage));
			}
			else
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Shield, data.ShieldValue));
			}
		}
		if (data.RetaliateValue > 0 && (data.Abilities == null || !data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Retaliate)))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Retaliate, data.RetaliateValue));
		}
		if (data.Abilities != null && data.Abilities.Count > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(GenerateDescription(data.Abilities));
		}
		else if (data.Overrides != null)
		{
			if (data.CompareAbility != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine(GenerateDescription(data.Overrides[0], data.CompareAbility.AbilityType));
			}
			else
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine(GenerateDescription(data.Overrides));
			}
		}
		return stringBuilder.ToString();
	}

	public static string GenerateDescription(CActionAugmentation augmentation)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < augmentation.AugmentationOps.Count; i++)
		{
			CActionAugmentationOp op = augmentation.AugmentationOps[i];
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(op));
		}
		for (int j = 0; j < augmentation.Infusions.Count; j++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(GenerateLine(augmentation.Infusions[j].ToString()));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CAbility.EAbilityType type, int value = 0, bool self = false, bool isScalar = false)
	{
		if (type == CAbility.EAbilityType.Damage && self)
		{
			type = CAbility.EAbilityType.AddHeal;
			value = -Math.Abs(value);
		}
		string icon = GetIcon(type);
		AbilityTypePreviewConfigUI abilityPreviewConfig = UIInfoTools.Instance.GetAbilityPreviewConfig(type);
		if (abilityPreviewConfig != null)
		{
			string format = abilityPreviewConfig.GetFormat();
			if (format.IsNOTNullOrEmpty())
			{
				return string.Format(format, icon, isScalar ? $"X{value}" : ((abilityPreviewConfig.showValueSign && value > 0) ? $"+{value}" : value.ToString()));
			}
		}
		string text = null;
		if (type != CAbility.EAbilityType.PreventDamage && value != 0)
		{
			switch (type)
			{
			case CAbility.EAbilityType.Shield:
			case CAbility.EAbilityType.Retaliate:
				return GenerateLine(icon, isScalar ? $"X{value}" : value.ToString());
			case CAbility.EAbilityType.Push:
			case CAbility.EAbilityType.Pull:
				return GenerateLine(icon, isScalar ? $"X{value}" : value.ToString(), iconBeforeValue: false);
			default:
				return GenerateLine(icon, isScalar ? $"X{value}" : ((value > 0) ? $"+{value}" : value.ToString()), iconBeforeValue: false);
			}
		}
		return GenerateLine(icon);
	}

	public static string GenerateDescription(CActionAugmentationOp op)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (op.Ability != null)
		{
			stringBuilder.Append(GenerateDescription(op.Ability));
		}
		CAbilityOverride abilityOverride = op.AbilityOverride;
		if (abilityOverride != null && abilityOverride.AbilityType.HasValue)
		{
			stringBuilder.Append(GenerateDescription(op.AbilityOverride));
			if (op.AbilityOverride.AbilityType != CAbility.EAbilityType.None && op.ParentAbilityType != CAbility.EAbilityType.None)
			{
				stringBuilder.Append(GenerateDescription(op.AbilityOverride.AbilityType.Value));
			}
		}
		else if (op.AbilityOverride != null)
		{
			stringBuilder.Append(GenerateDescription(op.AbilityOverride, op.AbilityOverride.AbilityType ?? op.ParentAbilityType));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateLine(string icon, string value = null, bool iconBeforeValue = true)
	{
		string text = string.Empty;
		if (icon != null && icon.Length > 0)
		{
			text = "<sprite name=\"" + icon + "\">";
		}
		if (value != null)
		{
			if (iconBeforeValue)
			{
				return string.Format("{1}{0}", value, text);
			}
			return value + text;
		}
		return text;
	}

	public static string GenerateDescription(CAbilityOverride cAbilityOverride, CAbility.EAbilityType? abilityType = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!abilityType.HasValue)
		{
			abilityType = cAbilityOverride.AbilityType;
		}
		if (cAbilityOverride.DamageSelfBeforeAttack.HasValue)
		{
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Damage, cAbilityOverride.DamageSelfBeforeAttack.Value, self: true));
		}
		if (abilityType.HasValue && cAbilityOverride.Strength.HasValue)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(abilityType.Value, cAbilityOverride.Strength.Value, cAbilityOverride.AbilityFilter != null && cAbilityOverride.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), cAbilityOverride.ActiveBonusData != null && (cAbilityOverride.ActiveBonusData.StrengthIsScalar || cAbilityOverride.ActiveBonusData.AbilityStrengthIsScalar)));
		}
		if (cAbilityOverride.Pierce.HasValue && cAbilityOverride.Pierce > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			string text = ((cAbilityOverride.Pierce > 1) ? cAbilityOverride.Pierce.ToString() : null);
			if (text != null && cAbilityOverride.Pierce >= 99999)
			{
				stringBuilder.Append(GenerateLine("IgnoreShield"));
			}
			else
			{
				stringBuilder.Append(GenerateLine("Pierce", text));
			}
		}
		if (cAbilityOverride.Range.HasValue)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription((abilityType.HasValue && abilityType == CAbility.EAbilityType.Loot) ? abilityType.Value : CAbility.EAbilityType.AddRange, cAbilityOverride.Range.Value));
		}
		if (cAbilityOverride.NumberOfTargets.HasValue && cAbilityOverride.NumberOfTargets > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddTarget, cAbilityOverride.NumberOfTargets.Value));
		}
		if (cAbilityOverride.Jump.HasValue && cAbilityOverride.Jump.Value)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateLine("Jump"));
		}
		if (cAbilityOverride.MiscAbilityData != null && cAbilityOverride.MiscAbilityData.AttackHasAdvantage.HasValue && cAbilityOverride.MiscAbilityData.AttackHasAdvantage.Value)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Advantage));
		}
		if (cAbilityOverride.NegativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition negativeCondition in cAbilityOverride.NegativeConditions)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(negativeCondition.ToString())));
			}
		}
		if (cAbilityOverride.PositiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition positiveCondition in cAbilityOverride.PositiveConditions)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(positiveCondition.ToString())));
			}
		}
		if (cAbilityOverride.SubAbilities != null)
		{
			foreach (CAbility subAbility in cAbilityOverride.SubAbilities)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateDescription(subAbility));
			}
		}
		if (cAbilityOverride.ConditionalOverrides != null)
		{
			foreach (CConditionalOverride conditionalOverride in cAbilityOverride.ConditionalOverrides)
			{
				foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
				{
					string value = GenerateDescription(abilityOverride, abilityType);
					if (value.IsNOTNullOrEmpty())
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append(value);
					}
				}
			}
		}
		if (cAbilityOverride.AttackEffects != null)
		{
			foreach (CAttackEffect attackEffect in cAbilityOverride.AttackEffects)
			{
				string value2 = GenerateDescription(attackEffect);
				if (value2.IsNOTNullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(value2);
				}
			}
		}
		if (cAbilityOverride.ChooseAbilities != null)
		{
			foreach (CAbility chooseAbility in cAbilityOverride.ChooseAbilities)
			{
				string value3 = GenerateDescription(chooseAbility);
				if (value3.IsNOTNullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(value3);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CAttackEffect effect)
	{
		if (effect.Effect == CAttackEffect.EAttackEffect.StandardBuff)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (effect.Strength > 0)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Damage, effect.Strength));
			}
			if (effect.AttackType == CAbility.EAttackType.Ranged)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddRange));
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	public static string GenerateDescription(List<CAbility> abilities)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < abilities.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(abilities[i]));
		}
		return stringBuilder.ToString();
	}

	public static string GenerateDescription(CAbility ability)
	{
		if (ability.PreviewEffectText.IsNOTNullOrEmpty())
		{
			return ability.PreviewEffectText;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (ability.AbilityType == CAbility.EAbilityType.Infuse)
		{
			foreach (IGrouping<ElementInfusionBoardManager.EElement, ElementInfusionBoardManager.EElement> item in from it in (ability as CAbilityInfuse).ElementsToInfuse
				group it by it)
			{
				int num = item.Count();
				stringBuilder.Append(GenerateLine(item.Key.ToString(), (num > 1) ? ("x" + num) : null));
			}
		}
		else if (ability.AbilityType == CAbility.EAbilityType.ConsumeElement)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in (ability as CAbilityConsumeElement).ElementsToConsume)
			{
				stringBuilder.Append(GenerateLine(item2.ToString()));
			}
		}
		else if (ability is CAbilityPlaySong cAbilityPlaySong)
		{
			stringBuilder.Append(GenerateDescription(cAbilityPlaySong.Song));
		}
		else if (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
		{
			stringBuilder.AppendLine(GenerateDescription(cAbilityForgoActionsForCompanion.ForgoTopActionAbility));
			stringBuilder.Append(GenerateDescription(cAbilityForgoActionsForCompanion.ForgoBottomActionAbility));
		}
		else if (ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
		{
			stringBuilder.Append(GenerateDescription(cAbilityAddActiveBonus.AddAbility));
		}
		else if (ability is CAbilityChoose cAbilityChoose)
		{
			for (int num2 = 0; num2 < cAbilityChoose.ChooseAbilities.Count; num2++)
			{
				if (num2 > 0)
				{
					stringBuilder.Append("/");
				}
				stringBuilder.Append(GenerateDescription(cAbilityChoose.ChooseAbilities[num2].AbilityType));
			}
		}
		else if (ability is CAbilityAdjustInitiative)
		{
			stringBuilder.Append("+/-" + ability.Strength + " " + LocalizationManager.GetTranslation("Initiative"));
		}
		else
		{
			stringBuilder.Append(GenerateDescription(ability.AbilityType, ability.Strength, ability.AbilityFilter != null && ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), ability.ActiveBonusData != null && (ability.ActiveBonusData.StrengthIsScalar || ability.ActiveBonusData.AbilityStrengthIsScalar)));
		}
		if (ability is CAbilityMove { Jump: not false })
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateLine("Jump"));
		}
		else if (ability.Range > 1)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddRange, ability.Range));
		}
		else if (ability is CAbilityRetaliate { RetaliateRange: >1 } cAbilityRetaliate)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddRange, cAbilityRetaliate.RetaliateRange));
		}
		if (ability is CAbilityAttack cAbilityAttack)
		{
			AbilityData.MiscAbilityData miscAbilityData = cAbilityAttack.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.AttackHasAdvantage == true)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(CCondition.EPositiveCondition.Advantage.ToString())));
			}
			AbilityData.MiscAbilityData miscAbilityData2 = cAbilityAttack.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.AttackHasDisadvantage == true)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(CCondition.ENegativeCondition.Disadvantage.ToString())));
			}
		}
		foreach (CAbility subAbility in ability.SubAbilities)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateDescription(subAbility));
		}
		if (ability.ActiveBonusData?.AbilityData != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateDescription(ability.ActiveBonusData.AbilityData));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CSong song)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < song.SongEffects.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(song.SongEffects[i]));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CSong.SongEffect songEffect)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (songEffect.Abilities != null)
		{
			for (int i = 0; i < songEffect.Abilities.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateDescription(songEffect.Abilities[i]));
			}
		}
		if (songEffect.AbilityOverrides != null)
		{
			for (int j = 0; j < songEffect.AbilityOverrides.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateDescription(songEffect.AbilityOverrides[j], songEffect.AbilityType));
			}
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(List<CAbilityOverride> abilities)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < abilities.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(abilities[i]));
		}
		return stringBuilder.ToString();
	}

	private string ConvertoToIconText(string icon)
	{
		string text = icon.SplitCamelCase().ToTitleCase().Replace(" ", "_");
		if (iconSpriteAsset.GetSpriteIndexFromName(text) < 0)
		{
			text = "AA_" + text;
		}
		if (iconSpriteAsset.GetSpriteIndexFromName(text) < 0)
		{
			text = string.Empty;
		}
		return text;
	}

	public static string GetIcon(CAbility.EAbilityType type)
	{
		string abilityPreviewEffectIconName = UIInfoTools.Instance.GetAbilityPreviewEffectIconName(type);
		if (abilityPreviewEffectIconName.IsNOTNullOrEmpty())
		{
			return abilityPreviewEffectIconName;
		}
		if (Choreographer.TargetingAbilitySpritesToSkip.Contains(type) || Choreographer.TargetingAbilityTypesToSkip.Contains(type))
		{
			return string.Empty;
		}
		return Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(type.ToString());
	}
}
