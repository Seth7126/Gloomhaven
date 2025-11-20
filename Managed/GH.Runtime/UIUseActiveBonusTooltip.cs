using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseActiveBonusTooltip : MonoBehaviour
{
	private enum ETooltipType
	{
		Item,
		Bonus
	}

	[SerializeField]
	private UIActiveAbility activeAbilityTooltip;

	[SerializeField]
	private UIItemTooltip itemTooltip;

	private ETooltipType type;

	public void Init(CActiveBonus bonus)
	{
		Reset();
		if (bonus.Layout == null && bonus.BaseCard is CItem item)
		{
			type = ETooltipType.Item;
			itemTooltip.Init(item);
		}
		else
		{
			type = ETooltipType.Bonus;
			activeAbilityTooltip.Initialize(bonus, 1, forceGeneration: true);
		}
	}

	private void Reset()
	{
		itemTooltip.Clear();
		activeAbilityTooltip.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		if (type == ETooltipType.Item)
		{
			UIManager.Instance.HighlightElement(itemTooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			itemTooltip.Show();
		}
		else if (type == ETooltipType.Bonus)
		{
			UIManager.Instance.HighlightElement(activeAbilityTooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			activeAbilityTooltip.gameObject.SetActive(value: true);
			SummonContainer componentInChildren = activeAbilityTooltip.GetComponentInChildren<SummonContainer>();
			if (componentInChildren != null)
			{
				componentInChildren.SummonLT.SetActive(value: false);
				componentInChildren.SummonLB.SetActive(value: false);
				componentInChildren.SummonMT.SetActive(value: false);
				componentInChildren.SummonMB.SetActive(value: false);
				componentInChildren.GetComponentInChildren<LayoutElement>().flexibleWidth = 0f;
			}
		}
	}

	public void Hide()
	{
		itemTooltip.Hide();
		UIManager.Instance.UnhighlightElement(itemTooltip.gameObject, unlockUI: false);
		UIManager.Instance.UnhighlightElement(activeAbilityTooltip.gameObject, unlockUI: false);
		activeAbilityTooltip.gameObject.SetActive(value: false);
		SummonContainer componentInChildren = activeAbilityTooltip.GetComponentInChildren<SummonContainer>();
		if (componentInChildren != null)
		{
			componentInChildren.SummonLT.SetActive(value: true);
			componentInChildren.SummonLB.SetActive(value: true);
			componentInChildren.SummonMT.SetActive(value: true);
			componentInChildren.SummonMB.SetActive(value: true);
			componentInChildren.GetComponentInChildren<LayoutElement>().flexibleWidth = 1f;
		}
	}

	public void Clear()
	{
		itemTooltip.Clear();
	}
}
