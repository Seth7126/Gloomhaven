using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseAbility : UIUseConsumeInfuseOptionsSlot<IAbility>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIAbilityTooltip tooltip;

	private ControllerInputAreaCustom _controllerArea;

	protected override void Awake()
	{
		base.Awake();
		tooltip.gameObject.SetActive(value: false);
		_controllerArea = new ControllerInputAreaCustom("Use Ability Item", null, null, stackArea: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnPointerDown, base.ShowHotkey, base.HideHotkey, isPersistent: false, KeyActionHandler.RegisterType.Click).AddBlocker(new ControllerAreaCustomFocusKeyActionHandlerBlocker(_controllerArea)));
	}

	public void Init(CPlayerActor player, IAbility ability, Action<IAbility> onSelect = null, Action<IAbility> onUnselect = null)
	{
		Init(player, ability, onSelect, onUnselect, null, isSelected: false, ability.Infusions, null, ability.GenerateOption(optionsUI[0]), OnPickedAll);
		_controllerArea.Focus();
		Decorate(ability, player);
		Refresh();
	}

	private void OnPickedAll(CItem item)
	{
		ConsumeOrInfuseIfPossible();
		onSelect?.Invoke(element);
	}

	private void Decorate(IAbility ability, CPlayerActor player)
	{
		selectAudioItem = ability.GetSelectAudioItem();
		previewEffect.SetDescription(ability.Abilities);
		icon.sprite = ability.Icon;
		tooltip.Initialize(ability, player);
	}

	protected override void ShowTooltip(bool show)
	{
		if (show)
		{
			if (!tooltip.gameObject.activeSelf)
			{
				UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
				tooltip.gameObject.SetActive(value: true);
			}
		}
		else if (tooltip.gameObject.activeSelf)
		{
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
			tooltip.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		OnPointerExit();
		_controllerArea.Unfocus();
		base.OnDisable();
	}
}
