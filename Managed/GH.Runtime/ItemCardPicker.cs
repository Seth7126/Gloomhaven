using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ItemCardPicker : MonoBehaviour
{
	[SerializeField]
	private RectTransform cardContainer;

	[SerializeField]
	private ItemCardPickerSlot slotPrefab;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ButtonOptionView _keyboardOption;

	[SerializeField]
	private LongHotkeyOptionView _gamepadOption;

	private UIWindow window;

	private List<ItemCardPickerSlot> cardPool = new List<ItemCardPickerSlot>();

	private Dictionary<CItem, ItemCardPickerSlot> cardSlots = new Dictionary<CItem, ItemCardPickerSlot>();

	private UnifiedOptionPresenter<ItemPickerOptionPresenter> _confirmOption;

	private int itemsToSelect;

	private List<CItem> itemsSelected = new List<CItem>();

	private Action onConfirmPressed;

	private Action<List<CItem>> onItemsSelected;

	private Action onItemsDeselected;

	private PopupStateTag stateToNavigate;

	private string hintTitle;

	private string hintMessage;

	public ItemCardPickerSlot _currentHoveredSlot;

	public ItemCardPickerSlot CurrentHoveredSlot => _currentHoveredSlot;

	public bool AreAllItemsSelected => itemsSelected.Count == itemsToSelect;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		window.onShown.AddListener(OnWindowShown);
		_confirmOption = new UnifiedOptionPresenter<ItemPickerOptionPresenter>(new ItemPickerOptionPresenter(new ButtonOptionPresenter(_keyboardOption, OnConfirmPressed), ConfirmAvailableChangedKeyboard), new ItemPickerOptionPresenter(new LongHotkeyOptionPresenter(_gamepadOption, OnConfirmPressed, PerformShortRest)), delegate(ItemPickerOptionPresenter option)
		{
			option.Enter();
		}, delegate(ItemPickerOptionPresenter option)
		{
			option.Exit();
		});
		void ConfirmAvailableChangedKeyboard(bool available)
		{
			_keyboardOption.SetTextKey(available ? "GUI_END_SELECTION" : "GUI_SELECT_ITEM");
		}
	}

	private void OnWindowShown()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(stateToNavigate);
	}

	private void OnHidden()
	{
		ClearContent();
		controllerArea.Destroy();
	}

	private void ClearContent()
	{
		foreach (KeyValuePair<CItem, ItemCardPickerSlot> cardSlot in cardSlots)
		{
			cardSlot.Value.Clear();
		}
		cardSlots.Clear();
		itemsSelected.Clear();
	}

	public void Show(PopupStateTag stateToNavigate, List<CItem> items, Action onConfirmPressed, bool canSelect, int itemsToSelect = 1, string hintTitle = null, string hintMessage = null, Action<List<CItem>> onAllItemsSelected = null, Action onItemsDeselected = null)
	{
		this.onConfirmPressed = onConfirmPressed;
		onItemsSelected = onAllItemsSelected;
		this.onItemsDeselected = onItemsDeselected;
		this.itemsToSelect = Math.Min(items.Count, itemsToSelect);
		this.stateToNavigate = stateToNavigate;
		this.hintTitle = hintTitle;
		this.hintMessage = hintMessage;
		ClearContent();
		HelperTools.NormalizePool(ref cardPool, slotPrefab.gameObject, cardContainer, items.Count);
		for (int i = 0; i < items.Count; i++)
		{
			cardPool[i].Init(items[i], OnSelectSlot, OnDeselectSlot);
			cardPool.ForEach(delegate(ItemCardPickerSlot card)
			{
				card.OnHover -= OnHoverSlot;
				card.OnHover += OnHoverSlot;
				card.IsAllItemsSelected = IsAllItemsSelected;
				card.Selectable.interactable = canSelect;
			});
			cardSlots[items[i]] = cardPool[i];
		}
		_confirmOption.Enter();
		if (!canSelect)
		{
			_confirmOption.Current.Hide();
		}
		window.Show();
		controllerArea.Enable();
		StartCoroutine(CoroutineHelper.SkipFramesCoroutine(UpdateConfirmAvailable));
	}

	private void UpdateConfirmAvailable()
	{
		bool flag = itemsSelected.Count >= itemsToSelect;
		_confirmOption.Current.SetConfirmAvailable(flag);
		SetShownHint(!flag);
	}

	private void SetShownHint(bool show)
	{
		if (!hintMessage.IsNullOrEmpty())
		{
			if (show)
			{
				Singleton<HelpBox>.Instance.ShowTranslated(hintMessage, hintTitle);
				Singleton<HelpBox>.Instance.BringToFront();
			}
			else
			{
				Singleton<HelpBox>.Instance.Hide();
			}
		}
	}

	private void PerformShortRest()
	{
		_currentHoveredSlot?.ToggleSelectSlotFromShortRest();
	}

	private void OnConfirmPressed()
	{
		onConfirmPressed?.Invoke();
	}

	private void OnHoverSlot(ItemCardPickerSlot slot)
	{
		_currentHoveredSlot = slot;
	}

	private void OnSelectSlot(CItem item)
	{
		if (!itemsSelected.Contains(item))
		{
			itemsSelected.Add(item);
			if (itemsSelected.Count == itemsToSelect)
			{
				onItemsSelected?.Invoke(itemsSelected);
			}
			else if (itemsSelected.Count > itemsToSelect)
			{
				cardSlots[itemsSelected[0]].Deselect();
			}
			UpdateConfirmAvailable();
		}
	}

	public bool IsAllItemsSelected()
	{
		return AreAllItemsSelected;
	}

	private void OnDeselectSlot(CItem item)
	{
		if (itemsSelected.Contains(item))
		{
			itemsSelected.Remove(item);
			if (itemsSelected.Count == itemsToSelect)
			{
				onItemsSelected?.Invoke(itemsSelected);
			}
			else
			{
				onItemsDeselected?.Invoke();
			}
			UpdateConfirmAvailable();
		}
	}

	public List<CItem> GetCurrentSelectedItems()
	{
		return itemsSelected;
	}

	public void Hide()
	{
		_confirmOption.Exit();
		window.Hide();
	}
}
