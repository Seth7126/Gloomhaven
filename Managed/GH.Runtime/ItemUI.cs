using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
	[SerializeField]
	private Image ItemImage;

	[SerializeField]
	private Image selectedItemImage;

	private bool isHighlighted;

	[SerializeField]
	protected Sprite[] emptyItemSprites;

	[SerializeField]
	protected Sprite[] equippedItemSprites;

	[SerializeField]
	protected Sprite[] useableItemSprites;

	[SerializeField]
	protected Sprite[] selectedItemSprites;

	[SerializeField]
	protected Sprite[] spentItemSprites;

	[SerializeField]
	protected Sprite[] consumedItemSprites;

	private CItem.EItemSlot slot;

	private CItem.EItemSlotState itemState;

	private Action<bool, CItem.EItemSlot> onHoverAction;

	private Action<bool, CItem.EItemSlot> onToggleAction;

	protected Sprite EmptySprite => emptyItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	protected Sprite EquippedSprite => equippedItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	protected Sprite UseableSprite => useableItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	protected Sprite SelectedSprite => selectedItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	protected Sprite SpentSprite => spentItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	protected Sprite ConsumedSprite => consumedItemSprites[(int)((isHighlighted ? 6 : 0) + (Slot - 1))];

	public CItem.EItemSlot Slot
	{
		get
		{
			return slot;
		}
		set
		{
			slot = value;
			SetState(itemState);
		}
	}

	public void Init(Action<bool, CItem.EItemSlot> onHoverAction, Action<bool, CItem.EItemSlot> onToggleAction, ToggleGroup toggleGroup = null)
	{
		this.onToggleAction = onToggleAction;
		this.onHoverAction = onHoverAction;
		if (toggleGroup != null)
		{
			GetComponent<Toggle>().group = toggleGroup;
		}
	}

	public void ToggleDisplayError(bool isActive)
	{
		ItemImage.color = (isActive ? Color.red : Color.white);
		selectedItemImage.color = (isActive ? Color.red : Color.white);
	}

	public void SetState(CItem.EItemSlotState itemState)
	{
		this.itemState = itemState;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if (selectedItemImage != null)
		{
			selectedItemImage.sprite = ((itemState == CItem.EItemSlotState.Equipped) ? UseableSprite : SpentSprite);
		}
		switch (itemState)
		{
		default:
			ItemImage.sprite = EmptySprite;
			break;
		case CItem.EItemSlotState.Equipped:
			ItemImage.sprite = EquippedSprite;
			break;
		case CItem.EItemSlotState.Useable:
			ItemImage.sprite = UseableSprite;
			break;
		case CItem.EItemSlotState.Consumed:
			ItemImage.sprite = ConsumedSprite;
			break;
		case CItem.EItemSlotState.Spent:
			ItemImage.sprite = SpentSprite;
			break;
		case CItem.EItemSlotState.Selected:
			ItemImage.sprite = SelectedSprite;
			break;
		}
	}

	public void OnPointerEnter()
	{
		isHighlighted = true;
		if (onHoverAction != null)
		{
			onHoverAction(arg1: true, Slot);
		}
		UpdateDisplay();
	}

	public void OnPointerExit()
	{
		isHighlighted = false;
		if (onHoverAction != null)
		{
			onHoverAction(arg1: false, Slot);
		}
		UpdateDisplay();
	}

	public void OnToggleSwitch(bool value)
	{
		if (onToggleAction != null)
		{
			onToggleAction(value, Slot);
		}
		UpdateDisplay();
	}
}
