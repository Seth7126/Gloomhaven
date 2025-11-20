using System;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardPickerSlot : MonoBehaviour
{
	[SerializeField]
	private RectTransform cardContainer;

	[SerializeField]
	private Button buttonSelect;

	[SerializeField]
	private GUIAnimator selectionAnimator;

	[SerializeField]
	private GameObject _hoveredImage;

	[SerializeField]
	private GameObject _selectedImage;

	private ItemCardUI itemCardUI;

	private Action<CItem> onSelected;

	private Action<CItem> onDeselected;

	private CItem item;

	private bool selected;

	public Func<bool> IsAllItemsSelected;

	private Vector2 _normalizedPosition = new Vector2(0.5f, 0.5f);

	private static ItemCardPickerSlot _currentItem;

	public static ItemCardPickerSlot CurrentItem => _currentItem;

	public Selectable Selectable => buttonSelect;

	public bool IsSelectable => Selectable.interactable;

	public bool Selected => selected;

	public static event Action<ItemCardPickerSlot, bool> ItemCardHoveringStateChanged;

	public static event Action<ItemCardPickerSlot, bool> ItemCardSelectionStateChanged;

	public event Action<ItemCardPickerSlot> OnHover;

	public void Init(CItem item, Action<CItem> onSelected, Action<CItem> onDeselected)
	{
		this.item = item;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		selected = false;
		if (itemCardUI == null || itemCardUI.item != item)
		{
			Clear();
			itemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, cardContainer, resetLocalScale: true).GetComponent<ItemCardUI>();
			RectTransform component = itemCardUI.GetComponent<RectTransform>();
			component.anchorMin = _normalizedPosition;
			component.anchorMax = _normalizedPosition;
			component.pivot = _normalizedPosition;
			component.anchoredPosition = Vector2.zero;
			itemCardUI.item = item;
			if (itemCardUI.NavigationSelectable != null)
			{
				itemCardUI.NavigationSelectable.ControlledSelectable.interactable = false;
				itemCardUI.NavigationSelectable.enabled = false;
			}
		}
		itemCardUI.Show(highlightElement: false);
	}

	public void ToggleSelectSlot()
	{
		if (!IsAllItemsSelected())
		{
			Toggle();
		}
	}

	public void ToggleSelectSlotFromShortRest()
	{
		Toggle();
	}

	private void Toggle()
	{
		if (IsSelectable)
		{
			if (!selected)
			{
				Select();
			}
			else
			{
				Deselect();
			}
		}
	}

	private void Select()
	{
		itemCardUI.UpdateState(CItem.EItemSlotState.Equipped, force: true);
		SetSelected(selected: true);
	}

	public void Deselect()
	{
		SetSelected(selected: false);
		itemCardUI.UpdateState(force: true);
	}

	private void SetSelected(bool selected)
	{
		this.selected = selected;
		ItemCardPickerSlot.ItemCardSelectionStateChanged?.Invoke(this, selected);
		if (selected)
		{
			onSelected?.Invoke(item);
		}
		else
		{
			onDeselected?.Invoke(item);
		}
		if (selectionAnimator != null)
		{
			selectionAnimator.SetPlayed(selected);
		}
		if (_selectedImage != null)
		{
			_selectedImage.gameObject.SetActive(selected);
		}
	}

	public void OnPointerEnter()
	{
		if (!selected)
		{
			itemCardUI.cardEffects.PreviewRefresh();
		}
		SetHovered(hovered: true);
	}

	public void OnPointerExit()
	{
		SetHovered(hovered: false);
		if (!selected && itemCardUI != null)
		{
			itemCardUI.UpdateState(force: true);
		}
	}

	private void SetHovered(bool hovered)
	{
		_currentItem = (hovered ? this : null);
		this.OnHover?.Invoke(this);
		ItemCardPickerSlot.ItemCardHoveringStateChanged?.Invoke(this, hovered);
		if (_hoveredImage != null)
		{
			_hoveredImage.gameObject.SetActive(hovered);
		}
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			Clear();
		}
	}

	public void Clear()
	{
		if (itemCardUI != null)
		{
			if (itemCardUI.NavigationSelectable != null)
			{
				itemCardUI.NavigationSelectable.ControlledSelectable.interactable = true;
				itemCardUI.NavigationSelectable.enabled = true;
			}
			ObjectPool.RecycleCard(itemCardUI.CardID, ObjectPool.ECardType.Item, itemCardUI.gameObject);
			itemCardUI = null;
		}
		if (selectionAnimator != null)
		{
			selectionAnimator.Stop();
		}
	}
}
