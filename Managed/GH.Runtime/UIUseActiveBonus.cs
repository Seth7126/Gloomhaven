using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseActiveBonus : UIUseConsumeInfuseOptionsSlot<IActiveBonus>
{
	[Header("Bonus")]
	[SerializeField]
	private Image icon;

	[SerializeField]
	private UIUseActiveBonusTooltip tooltip;

	[SerializeField]
	private GUIAnimator mandatoryHighlight;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIUseOption initiativeOption;

	[SerializeField]
	private Sprite initiativePickerIcon;

	private Action<IActiveBonus, bool> onHovered;

	protected override void Awake()
	{
		base.Awake();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnPointerDown, base.ShowHotkey, base.HideHotkey).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
		OnPointerExit();
	}

	public void SetActiveBonus(CActor actor, CActiveBonus bonus, Action<CActiveBonus, CActor> onSelectBonus = null, Action<CActiveBonus, CActor> onUnselectBonus = null, Func<CActiveBonus, bool> isMandatoryChecker = null)
	{
		Tuple<IOptionHolder, List<IOption>> tuple = null;
		Action<IActiveBonus, bool> action = null;
		for (int i = 0; i < optionsUI.Count; i++)
		{
			optionsUI[i].Hide();
		}
		IActiveBonus bonus2;
		if (bonus is CAdjustInitiativeActiveBonus cAdjustInitiativeActiveBonus && bonus.BespokeBehaviour is CAdjustInitiativeActiveBonus_AdjustInitiative)
		{
			InitiativeOptionController item = new InitiativeOptionController(initiativeOption, actor.Initiative());
			tuple = new Tuple<IOptionHolder, List<IOption>>(item, new List<IOption>
			{
				new InitiativeOption(-cAdjustInitiativeActiveBonus.Ability.Strength, Math.Max(1, actor.Initiative() - cAdjustInitiativeActiveBonus.Ability.Strength), initiativePickerIcon),
				new InitiativeOption(cAdjustInitiativeActiveBonus.Ability.Strength, actor.Initiative() + cAdjustInitiativeActiveBonus.Ability.Strength, initiativePickerIcon)
			});
			bonus2 = new AdjustInitativeActiveBonus(bonus, actor, item, tuple.Item2);
		}
		else if (bonus is CForgoActionsForCompanionActiveBonus cForgoActionsForCompanionActiveBonus)
		{
			ForgoActiveBonus forgoActiveBonus = new ForgoActiveBonus(cForgoActionsForCompanionActiveBonus, actor, optionsUI.FindAll((UIUseOption it) => it != initiativeOption));
			tuple = new Tuple<IOptionHolder, List<IOption>>(forgoActiveBonus, new List<IOption>
			{
				new AbilityOption(cForgoActionsForCompanionActiveBonus.ForgoActionsForCompanionAbility.ForgoTopActionAbility),
				new AbilityOption(cForgoActionsForCompanionActiveBonus.ForgoActionsForCompanionAbility.ForgoBottomActionAbility)
			});
			action = delegate(IActiveBonus _, bool hovered)
			{
				forgoActiveBonus.OnHovered(hovered);
			};
			bonus2 = forgoActiveBonus;
		}
		else if (bonus is CChooseAbilityActiveBonus cChooseAbilityActiveBonus)
		{
			ChooseAbilityOptionController item2 = new ChooseAbilityOptionController(optionsUI.First((UIUseOption it) => it != initiativeOption));
			tuple = new Tuple<IOptionHolder, List<IOption>>(item2, cChooseAbilityActiveBonus.ChooseAbility.ChooseAbilities.ConvertAll((Converter<CAbility, IOption>)((CAbility it) => new AbilityOption(it))));
			bonus2 = new ChooseAbilityActiveBonus(cChooseAbilityActiveBonus, actor, item2, tuple.Item2, this);
		}
		else
		{
			bonus2 = new ActiveBonus(bonus, actor);
		}
		Decorate(bonus);
		SetActiveBonus(actor, bonus2, delegate
		{
			onSelectBonus?.Invoke(bonus, actor);
		}, delegate
		{
			onUnselectBonus?.Invoke(bonus, actor);
		}, action, (IActiveBonus _) => isMandatoryChecker != null && isMandatoryChecker(bonus), tuple);
	}

	protected void SetActiveBonus(CActor actor, IActiveBonus bonus, Action<IActiveBonus> onSelectBonus = null, Action<IActiveBonus> onUnselectBonus = null, Action<IActiveBonus, bool> onHovered = null, Func<IActiveBonus, bool> isMandatoryChecker = null, Tuple<IOptionHolder, List<IOption>> option = null)
	{
		this.onHovered = onHovered;
		Init(actor, bonus, delegate(IActiveBonus activeBonus)
		{
			activeBonus.ToggleActiveBonus((consumes.Count > 0) ? consumes[0].SelectedElement : ((ElementInfusionBoardManager.EElement?)null), fromClick: true);
			onSelectBonus?.Invoke(activeBonus);
		}, delegate(IActiveBonus activeBonus)
		{
			onUnselectBonus?.Invoke(activeBonus);
		}, isMandatoryChecker, bonus.IsToggled, null, bonus.GetConsumes(), option);
		Decorate(bonus);
		OnPointerExit();
	}

	protected override void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		base.CreateConsumes(elements);
		if (element.GetSelectedConsume().HasValue)
		{
			consumes[0].SetSelectedElement(element.GetSelectedConsume());
		}
	}

	public override void OnPointerEnter()
	{
		if (!hovered)
		{
			onHovered?.Invoke(element, arg2: true);
		}
		base.OnPointerEnter();
	}

	public override void OnPointerExit()
	{
		if (hovered)
		{
			onHovered?.Invoke(element, arg2: false);
		}
		base.OnPointerExit();
	}

	public override void ClearSelection(bool fromClick = false)
	{
		if (!element.IsToggleLocked)
		{
			base.ClearSelection(fromClick: false);
			element.UntoggleActiveBonus(fromClick);
		}
	}

	private void Decorate(CActiveBonus bonus)
	{
		tooltip.Init(bonus);
		if (bonus.BaseCard is CItem description)
		{
			previewEffect.SetDescription(description);
		}
		else
		{
			previewEffect.SetDescription(bonus.Ability);
		}
	}

	private void Decorate(IActiveBonus bonus)
	{
		icon.sprite = bonus.GetIcon();
		selectAudioItem = bonus.GetSelectAudioItem();
	}

	protected override void ShowTooltip(bool show)
	{
		if (show)
		{
			tooltip.Show();
		}
		else
		{
			tooltip.Hide();
		}
	}

	public override void Hide()
	{
		base.Hide();
		tooltip.Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		OnPointerExit();
	}

	protected override void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnPointerDown);
		}
		base.OnDestroy();
		tooltip.Clear();
	}

	protected override void CancelAnimations()
	{
		base.CancelAnimations();
		mandatoryHighlight.Stop();
	}

	public void Highlight()
	{
		mandatoryHighlight.Play();
	}
}
