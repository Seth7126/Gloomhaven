using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

public class ChooseAbility : ISingleOptionHolder, IAbility
{
	private CPlayerActor playerActor;

	private CAbilityChooseAbility ability;

	private List<IOption> options;

	private IOption selectedOption;

	private UIUseOption optionUI;

	public List<IInfuseElement> Infusions => null;

	public List<CAbility> Abilities => new List<CAbility> { ability };

	public Sprite Icon
	{
		get
		{
			if (ability.AbilityBaseCard is CItem cItem)
			{
				return UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
			}
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(playerActor.GetPrefabName(), playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
	}

	public string ID => ability.Name;

	public CItem DescriptionItem
	{
		get
		{
			if (ability.AbilityBaseCard is CItem result)
			{
				return result;
			}
			return null;
		}
	}

	public string DescriptionTitle
	{
		get
		{
			if (!(ability.AbilityBaseCard is CItem))
			{
				return LocalizationManager.GetTranslation((ability.AbilityBaseCard != null) ? ability.AbilityBaseCard.Name : ability.ParentAbilityBaseCard.Name);
			}
			return null;
		}
	}

	public string DescriptionText
	{
		get
		{
			if (ability.AbilityBaseCard is CItem)
			{
				return null;
			}
			CActiveBonus cActiveBonus = null;
			cActiveBonus = ((ability.AbilityBaseCard == null) ? ability.ParentAbilityBaseCard.ActiveBonuses.FirstOrDefault((CActiveBonus it) => it is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.AddAbility == ability) : ability.AbilityBaseCard.ActiveBonuses.FirstOrDefault((CActiveBonus it) => it is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.AddAbility == ability));
			if (cActiveBonus?.Layout?.ListLayouts != null)
			{
				return CreateLayout.LocaliseText(cActiveBonus.Layout.ListLayouts.FirstOrDefault());
			}
			return null;
		}
	}

	public override IOption SelectedOption
	{
		get
		{
			return selectedOption;
		}
		set
		{
			selectedOption = value;
			if (selectedOption == null)
			{
				optionUI.Clear();
				ability.ResetChosenAbility();
			}
			else
			{
				int num = options.IndexOf(selectedOption);
				optionUI.SetOption(PreviewEffectGenerator.GenerateDescription(ability.ApplicableAbilities[num]));
				ability.AbilityChosen(num);
			}
		}
	}

	public CAbility SelectedAbility
	{
		get
		{
			if (SelectedOption != null)
			{
				return ability.ApplicableAbilities[options.IndexOf(selectedOption)];
			}
			return null;
		}
	}

	public ChooseAbility(CPlayerActor playerActor, CAbilityChooseAbility ability)
	{
		this.ability = ability;
		options = new List<IOption>();
		this.playerActor = playerActor;
		foreach (CAbility applicableAbility in ability.ApplicableAbilities)
		{
			options.Add(new AbilityOption(applicableAbility));
		}
	}

	public Tuple<IOptionHolder, List<IOption>> GenerateOption(UIUseOption optionUI)
	{
		this.optionUI = optionUI;
		optionUI.Clear();
		optionUI.Show();
		return new Tuple<IOptionHolder, List<IOption>>(this, options);
	}

	public string GetSelectAudioItem()
	{
		if (ability.AbilityBaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemConfig(cItem.YMLData.Art).toggleAudioItem;
		}
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}
}
