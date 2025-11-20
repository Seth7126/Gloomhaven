using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINewEnhancementShopInventory : Singleton<UINewEnhancementShopInventory>
{
	[Serializable]
	public class ShopEnhancementEvent : UnityEvent<EnhancementSlot>
	{
	}

	private const int _buyTabElementIndex = 0;

	private const int _sellTabElementIndex = 1;

	[SerializeField]
	private int initialPoolSlots = 20;

	[SerializeField]
	private ScrollRect scrollContainer;

	[SerializeField]
	private UINewEnhancementShopSlot slotPrefab;

	[SerializeField]
	private CanvasGroup tooltip;

	[SerializeField]
	private TextMeshProUGUI tooltipText;

	[SerializeField]
	private GameObject emptyInventory;

	[SerializeField]
	private CanvasGroup enhancementsCanvasGroup;

	[SerializeField]
	private UINavigationTabComponent _tabComponent;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private ControllerInputScroll _controllerInputScroll;

	[SerializeField]
	private HotkeyContainer _selectEnhancementHotkeyContainer;

	private EnhancementSlot _currentSlot;

	public ShopEnhancementEvent OnSelectedSlot = new ShopEnhancementEvent();

	public ShopEnhancementEvent OnHoveredSlot = new ShopEnhancementEvent();

	public ShopEnhancementEvent OnUnhoveredSlot = new ShopEnhancementEvent();

	private List<UINewEnhancementShopSlot> slotsPool = new List<UINewEnhancementShopSlot>();

	public EnhancementSlot CurrentSlot => _currentSlot;

	public ControllerInputArea InputArea => controllerArea;

	public HotkeyContainer SelectEnhancementHotkeyContainer => _selectEnhancementHotkeyContainer;

	public Dictionary<EnhancementSlot, UINewEnhancementShopSlot> AssignedSlots { get; set; } = new Dictionary<EnhancementSlot, UINewEnhancementShopSlot>();

	public bool HasAvailableEnhancements { get; private set; }

	public event Action<bool> EventEnhancementsAvailableChange;

	[UsedImplicitly]
	protected override void Awake()
	{
		base.Awake();
		controllerArea.OnFocused.AddListener(OnAreaFocused);
		controllerArea.OnUnfocused.AddListener(OnAreaUnfocused);
	}

	public void Preload()
	{
		HelperTools.NormalizePool(ref slotsPool, slotPrefab.gameObject, scrollContainer.content, initialPoolSlots);
		for (int i = 0; i < initialPoolSlots; i++)
		{
			slotsPool[i].gameObject.SetActive(value: false);
		}
		SetActiveControllerInputScroll(active: false);
	}

	private void OnAreaUnfocused()
	{
		SetActiveControllerInputScroll(active: false);
	}

	private void OnAreaFocused()
	{
		SetActiveControllerInputScroll(active: true);
	}

	public void Display(ICharacter character, List<EnhancementSlot> enhancements)
	{
		scrollContainer.verticalNormalizedPosition = 1f;
		CreateSlots(character, enhancements);
	}

	private void CreateSlots(ICharacter character, List<EnhancementSlot> enhancements)
	{
		IEnumerable<IGrouping<EEnhancement, EnhancementSlot>> source = from it in enhancements
			group it by it.enhancement;
		int num = 0;
		HelperTools.NormalizePool(ref slotsPool, slotPrefab.gameObject, scrollContainer.content, enhancements.Count);
		AssignedSlots.Clear();
		foreach (var item in from x in source.SelectMany((IGrouping<EEnhancement, EnhancementSlot> slots) => slots.Select((EnhancementSlot slot, int i) => (slot: slot, slots.Count(), i: i)))
			orderby x.slot.AvailableToSell descending, (!x.slot.AvailableToSell) ? x.slot.enhancement : EEnhancement.NoEnhancement, x.Item2 descending, x.slot.button.EnhancementSlot
			select x)
		{
			EnhancementSlot enhancement = item.slot;
			UINewEnhancementShopSlot uINewEnhancementShopSlot = slotsPool[num];
			IEnhancementPriceCalculator priceCalculator = enhancement.priceCalculator;
			bool flag = item.Item2 > 1;
			int num2 = item.i + 1;
			uINewEnhancementShopSlot.Initialize(enhancement, character, priceCalculator.CalculateTotalPrice(enhancement), flag ? num2.ToString() : null, OnHovered, OnSelected, delegate(bool show, RectTransform holder)
			{
				ShowPriceTooltip(show, holder, show ? priceCalculator.BuildPriceDesglose(enhancement) : null);
			}, priceCalculator.CanAffordPrice(enhancement), priceCalculator.CalculatePoints(enhancement), priceCalculator.CanAffordPoints(enhancement));
			AssignedSlots[enhancement] = uINewEnhancementShopSlot;
			if (controllerArea.IsFocused)
			{
				uINewEnhancementShopSlot.EnableNavigation();
			}
			num++;
		}
		if (AssignedSlots.Count > 0)
		{
			AssignedSlots.FirstOrDefault().Value.OnHover();
		}
		bool flag2 = num > 0;
		emptyInventory.SetActive(!flag2);
		HasAvailableEnhancements = flag2;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		controllerArea.OnFocused.RemoveListener(OnAreaFocused);
		controllerArea.OnUnfocused.RemoveListener(OnAreaUnfocused);
	}

	public void SetActiveControllerInputScroll(bool active)
	{
		if (InputManager.GamePadInUse && _controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = active;
		}
	}

	private void OnHovered(bool hovered, EnhancementSlot enhancement)
	{
		if (hovered)
		{
			if (InputManager.GamePadInUse)
			{
				scrollContainer.ScrollToFit(AssignedSlots[enhancement].transform as RectTransform);
			}
			_currentSlot = enhancement;
			OnHoveredSlot.Invoke(enhancement);
		}
		else
		{
			OnUnhoveredSlot.Invoke(enhancement);
		}
	}

	private void ShowPriceTooltip(bool show, RectTransform position, string text)
	{
		if (show && text.IsNOTNullOrEmpty())
		{
			tooltip.alpha = 1f;
			tooltipText.text = text;
			tooltip.transform.position = position.position;
		}
		else
		{
			tooltip.alpha = 0f;
		}
	}

	private void OnSelected(EnhancementSlot enhancementSlot)
	{
		OnSelectedSlot.Invoke(enhancementSlot);
	}

	public void RefreshAffordable()
	{
		foreach (KeyValuePair<EnhancementSlot, UINewEnhancementShopSlot> assignedSlot in AssignedSlots)
		{
			IEnhancementPriceCalculator priceCalculator = assignedSlot.Key.priceCalculator;
			assignedSlot.Value.UpdateAffordable(priceCalculator.CanAffordPrice(assignedSlot.Key));
			assignedSlot.Value.UpdatePoints(priceCalculator.CalculatePoints(assignedSlot.Key), priceCalculator.CanAffordPoints(assignedSlot.Key));
		}
	}

	public void SetInteractable(bool isInteractable)
	{
		enhancementsCanvasGroup.interactable = isInteractable;
	}

	public void DetermineSlotInteractability()
	{
		foreach (KeyValuePair<EnhancementSlot, UINewEnhancementShopSlot> assignedSlot in AssignedSlots)
		{
			assignedSlot.Value.DetermineInteractability();
		}
	}

	public void Clear()
	{
		AssignedSlots.Clear();
		emptyInventory.SetActive(value: true);
		HelperTools.NormalizePool(ref slotsPool, slotPrefab.gameObject, scrollContainer.content, 0);
	}

	public void SelectTabElementByShopMode(bool buyOrSell)
	{
		if (_tabComponent != null)
		{
			if (buyOrSell)
			{
				_tabComponent.Select(0);
			}
			else
			{
				_tabComponent.Select(1);
			}
		}
	}
}
