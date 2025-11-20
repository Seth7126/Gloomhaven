using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.Tabs;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopItemInventory : MonoBehaviour
{
	[Serializable]
	private struct SlotContent
	{
		public RectTransform container;

		public GameObject emptyIndicator;
	}

	private enum EShopMode
	{
		Buy,
		Sell
	}

	private class MerchantItemDecorator
	{
		private List<CItem> items;

		private UIShopItemSlot slot;

		private IShopItemService service;

		private CItem itemKey;

		private Action<UIShopItemSlot, bool, int> onHovered;

		private bool isNew;

		private int index;

		public int Amount => items?.Count ?? 0;

		public CItem Item => itemKey;

		public bool AnyNew { get; private set; }

		public List<CItem> Items => items;

		public MerchantItemDecorator(int index, CItem itemKey, List<CItem> items, IShopItemService service, UIShopItemSlot slot, int total, Action<MerchantItemDecorator> onSelected, Action<UIShopItemSlot, bool, int> onHovered, Action<UIShopItemSlot, MoveDirection> onMoved, int discount, CMapCharacter characterToBuyFor)
		{
			MerchantItemDecorator obj = this;
			this.items = items;
			this.itemKey = itemKey;
			this.service = service;
			this.slot = slot;
			this.onHovered = onHovered;
			this.index = index;
			AnyNew = items?.Exists((CItem it) => it.IsNew) ?? false;
			bool flag = items != null && items.Count != 0 && items.All((CItem it) => it.IsNew);
			slot.Initialize(itemKey, service.DiscountedCost(itemKey), delegate
			{
				onSelected(obj);
			}, OnHovered, onMoved, items?.Count ?? 0, total, service.IsAffordable(itemKey, characterToBuyFor), flag, !flag && AnyNew, discount, characterToBuyFor);
		}

		private void OnHovered(UIShopItemSlot itemUI, bool hovered)
		{
			if (hovered && items != null)
			{
				service.UnmarkNewItem(items);
				AnyNew = false;
			}
			onHovered(itemUI, hovered, index);
		}

		public void Buy(CMapCharacter targetCharacter, uint targetItemNetworkID = 0u, bool equip = false)
		{
			CItem item = ((targetItemNetworkID != 0) ? items.First((CItem x) => x.NetworkID == targetItemNetworkID) : items[items.Count - 1]);
			items.Remove(item);
			service.Buy(item, targetCharacter, equip);
			AnyNew = items?.Exists((CItem it) => it.IsNew) ?? false;
			slot.UpdateAvailability(items.Count);
			bool flag = items != null && items.Count != 0 && items.All((CItem it) => it.IsNew);
			slot.ShowNewItemNotification(flag);
			slot.ShowNewAmountNotification(!flag && AnyNew);
		}

		public void UpdateAffordability(CMapCharacter targetCharacter)
		{
			slot.UpdateAffordability(service.IsAffordable(itemKey, targetCharacter));
		}

		public void EnableDivider(bool enable)
		{
			slot.EnableDivider(enable);
		}

		public void ToggleSlot(bool isEnabled)
		{
			slot.gameObject.SetActive(isEnabled);
		}

		public static implicit operator GameObject(MerchantItemDecorator d)
		{
			return d.slot.gameObject;
		}

		public static implicit operator Selectable(MerchantItemDecorator d)
		{
			return d.slot.Selectable;
		}
	}

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private UIShopItemSlot slotPrefab;

	[SerializeField]
	private UITab sellTab;

	[SerializeField]
	private UITab buyTab;

	[SerializeField]
	private UINavigationTabComponent _shopFilterTabComponent;

	[SerializeField]
	private TabComponentInputListener _shopFilterInputListener;

	[SerializeField]
	private UIPartyItemInventoryTooltip itemTooltip;

	[SerializeField]
	private UIShopItemFilter allFilter;

	[SerializeField]
	private string audioItemSellConfirmation = "PlaySound_UIEquipmentTabSwap";

	[SerializeField]
	private string audioItemBuyConfirmation = "PlaySound_UIEquipmentTabSelect";

	[SerializeField]
	private CanvasGroup itemsCanvasGroup;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private CanvasGroup _shopItemInventoryCanvasGroup;

	[Header("Owned")]
	[SerializeField]
	private UIShopItemFilter _ownedFilter;

	[Header("Head")]
	[SerializeField]
	private UIShopItemFilter headFilter;

	[SerializeField]
	private SlotContent headSection;

	[Header("Body")]
	[SerializeField]
	private UIShopItemFilter bodyFilter;

	[SerializeField]
	private SlotContent bodySection;

	[Header("Hands")]
	[SerializeField]
	private UIShopItemFilter handsFilter;

	[SerializeField]
	private SlotContent handsSection;

	[Header("Legs")]
	[SerializeField]
	private UIShopItemFilter legsFilter;

	[SerializeField]
	private SlotContent legsSection;

	[Header("Small Items")]
	[SerializeField]
	private UIShopItemFilter smallItemsFilter;

	[SerializeField]
	private SlotContent smallItemsSection;

	[Header("HotkeysTooltips")]
	[SerializeField]
	private Hotkey[] _hotkeys;

	[Header("HotkeysTooltips")]
	[SerializeField]
	private Hotkey _selectHotkey;

	[SerializeField]
	private GameObject _backgroundBlackoutObject;

	[SerializeField]
	private Image _focusBackground;

	[SerializeField]
	private ControllerInputScroll _controllerInputScroll;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	private const string FORMAT_COST = "<color=#{2}>{0}: <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>";

	public BasicEventHandler OnNewItemBuyShown;

	private List<UIShopItemSlot> slotPool = new List<UIShopItemSlot>();

	private Dictionary<CItem.EItemSlot, List<UIShopItemSlot>> sellSlots = new Dictionary<CItem.EItemSlot, List<UIShopItemSlot>>();

	private Dictionary<CItem.EItemSlot, List<MerchantItemDecorator>> buySlots = new Dictionary<CItem.EItemSlot, List<MerchantItemDecorator>>();

	private IShopItemService service;

	private EShopMode mode;

	private CMapCharacter character;

	private UIShopItemSlot _currentHoveredItemSlot;

	private readonly CItemMerchantEqualityComparer _cItemMerchantEqualityComparer = new CItemMerchantEqualityComparer();

	private string _selectHotkeyBuyEvent = "Buy";

	private string _selectHotkeySellEvent = "Sell";

	public UIShopItemSlot CurrentHoveredItemSlot
	{
		get
		{
			return _currentHoveredItemSlot;
		}
		set
		{
			_currentHoveredItemSlot = value;
			this.HoveredItemSlotStateChanged?.Invoke(_currentHoveredItemSlot);
		}
	}

	public ControllerInputArea ControllerArea => controllerArea;

	public bool TooltipShown
	{
		get
		{
			if (itemTooltip != null)
			{
				return itemTooltip.TooltipShown;
			}
			return false;
		}
	}

	public event Action<IUiNavigationSelectable> OnItemsRefreshed;

	public event Action<UIShopItemSlot> HoveredItemSlotStateChanged;

	public event Action<UIPartyItemInventoryTooltip> NewItemTooltipShown;

	[UsedImplicitly]
	private void Awake()
	{
		if (InputManager.GamePadInUse)
		{
			HoveredItemSlotStateChanged += OnHoveredItemSlotStateChangedSelf;
		}
		foreach (ItemListingType value in Enum.GetValues(typeof(ItemListingType)))
		{
			ItemListingType filterType = value;
			if (filterType != ItemListingType.Owned || InputManager.GamePadInUse)
			{
				GetFilter(filterType).tab.onValueChanged.AddListener(OnFilterTabClick);
			}
			void OnFilterTabClick(bool active)
			{
				if (active)
				{
					FilterShownItems(filterType);
				}
			}
		}
		buyTab.onValueChanged.AddListener(OnBuyValueChanged);
		sellTab.onValueChanged.AddListener(OnSellValueChanged);
		ControllerArea.OnFocused.AddListener(EnableNavigation);
		ControllerArea.OnUnfocused.AddListener(DisableNavigation);
		ToggleFocusBackground(isActive: false);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		HoveredItemSlotStateChanged -= OnHoveredItemSlotStateChangedSelf;
		ControllerArea.OnFocused.RemoveListener(EnableNavigation);
		ControllerArea.OnUnfocused.RemoveListener(DisableNavigation);
		sellTab.onValueChanged.RemoveListener(OnSellValueChanged);
		buyTab.onValueChanged.RemoveListener(OnBuyValueChanged);
		legsFilter.tab.onValueChanged.RemoveAllListeners();
		bodyFilter.tab.onValueChanged.RemoveAllListeners();
		handsFilter.tab.onValueChanged.RemoveAllListeners();
		headFilter.tab.onValueChanged.RemoveAllListeners();
		smallItemsFilter.tab.onValueChanged.RemoveAllListeners();
		allFilter.tab.onValueChanged.RemoveAllListeners();
		Hotkey[] hotkeys = _hotkeys;
		for (int i = 0; i < hotkeys.Length; i++)
		{
			hotkeys[i].Deinitialize();
		}
		if (InputManager.GamePadInUse)
		{
			_selectHotkey.Deinitialize();
		}
	}

	private void OnBuyValueChanged(bool active)
	{
		if (active && mode != EShopMode.Buy)
		{
			ShowBuy();
		}
	}

	private void OnSellValueChanged(bool active)
	{
		if (active && mode != EShopMode.Sell)
		{
			ShowSell();
		}
	}

	private void OnHoveredItemSlotStateChangedSelf(UIShopItemSlot obj)
	{
		RaiseSelectHotkeyDisplayChanged();
		if (!(obj == null))
		{
			if (obj.Owner != null)
			{
				SetSellHotkey();
			}
			else
			{
				SetBuyHotkey();
			}
		}
	}

	public void ToggleFocusBackground(bool isActive)
	{
		if (_focusBackground != null)
		{
			_focusBackground.enabled = isActive;
		}
	}

	public void SetBuyHotkey()
	{
		_selectHotkey.ExpectedEvent = _selectHotkeyBuyEvent;
		_selectHotkey.UpdateHotkeyDisplay();
	}

	public void SetSellHotkey()
	{
		_selectHotkey.ExpectedEvent = _selectHotkeySellEvent;
		_selectHotkey.UpdateHotkeyDisplay();
	}

	public void SetHotkeysActive(bool value)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (value)
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj in hotkeys)
			{
				obj.Initialize(Singleton<UINavigation>.Instance.Input);
				obj.DisplayHotkey(active: true);
			}
			_selectHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			RaiseSelectHotkeyDisplayChanged();
		}
		else
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj2 in hotkeys)
			{
				obj2.Deinitialize();
				obj2.DisplayHotkey(active: false);
			}
			_selectHotkey.Deinitialize();
			_selectHotkey.DisplayHotkey(active: false);
		}
	}

	private void RaiseSelectHotkeyDisplayChanged()
	{
		if (InputManager.GamePadInUse)
		{
			_selectHotkey.DisplayHotkey(SelectHotkeyDisplayPredicate());
		}
	}

	private bool SelectHotkeyDisplayPredicate()
	{
		if (CurrentHoveredItemSlot != null)
		{
			return CurrentHoveredItemSlot.IsAvailable;
		}
		return false;
	}

	public void Init(IShopItemService service, CMapCharacter character)
	{
		this.service = service;
		this.character = character;
		if (InputManager.GamePadInUse)
		{
			_shopFilterTabComponent.First();
			ShowItemsGamepad();
		}
		else
		{
			ShowBuy();
		}
		allFilter.tab.isOn = true;
	}

	public void SetInteractable(bool isInteractable)
	{
		itemsCanvasGroup.interactable = isInteractable;
	}

	public void RefreshView()
	{
		if (InputManager.GamePadInUse)
		{
			ShowItemsGamepad();
			UIShopItemFilter selectedFilter = GetSelectedFilter();
			if (selectedFilter != null)
			{
				FilterShownItems(selectedFilter.filter);
			}
		}
		else if (mode == EShopMode.Sell)
		{
			ShowSell();
		}
		else if (mode == EShopMode.Buy)
		{
			ShowBuy();
		}
	}

	private void ShowItemsGamepad()
	{
		Singleton<UIItemConfirmationBox>.Instance.Hide();
		Clear();
		List<CItem> list = ((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null) ? new List<CItem>() : service.GetItemsToBuy(character));
		List<CItem> list2 = ((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null) ? list : list.Concat(service.GetItemsToSell()).ToList());
		if (character != null)
		{
			list2 = list2.FindAll((CItem it) => it.CanEquipItem(character.CharacterID));
		}
		List<CItem> sellItems = ((character == null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? new List<CItem>() : service.GetItemsToSell(character));
		Dictionary<CItem, Tuple<CMapCharacter, bool>> boundItems = ((character == null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? new Dictionary<CItem, Tuple<CMapCharacter, bool>>() : service.GetBoundsItems(character));
		List<CItem> list3 = (from it in sellItems
			orderby LocalizationManager.GetTranslation(it.YMLData.Name), boundItems.ContainsKey(it) ? ((!boundItems[it].Item2) ? 1 : 2) : 0
			select it).ToList();
		List<Tuple<CItem, int>> list4 = (from x in list2
			group x by x.ID into it
			select new Tuple<CItem, int>(it.FirstOrDefault(), it.Count())).ToList();
		ILookup<int, CItem> lookup = list.ToLookup((CItem it) => it.ID);
		for (int num = 0; num < list3.Count; num++)
		{
			CItem item = list3[num];
			CItem.EItemSlot eItemSlot = ((item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : item.YMLData.Slot);
			UIShopItemSlot slot = GetSlot(GetSection(eItemSlot).container, num);
			if (!sellSlots.ContainsKey(eItemSlot))
			{
				sellSlots[eItemSlot] = new List<UIShopItemSlot>();
			}
			sellSlots[eItemSlot].Add(slot);
			int num2 = 0;
			num2 = list4.Find((Tuple<CItem, int> x) => x.Item1.ID == item.ID)?.Item2 ?? 0;
			slot.Initialize(item, item.SellPrice, OnSelectedItemSell, OnHoveredItemSell, null, boundItems.ContainsKey(item) ? boundItems[item].Item1 : null, lookup[item.ID].ToList().Count, num2, boundItems.ContainsKey(item) && boundItems[item].Item2, character, boundItems.ContainsKey(item));
			slot.transform.SetSiblingIndex(num);
		}
		for (int num3 = sellItems.Count; num3 < slotPool.Count; num3++)
		{
			slotPool[num3].gameObject.SetActive(value: false);
		}
		allFilter.ShowAmount(sellItems.Count);
		_ownedFilter.ShowAmount(sellItems.Count);
		foreach (KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> sellSlot in sellSlots)
		{
			int count = sellSlot.Value.Count;
			UIShopItemFilter filter = GetFilter(sellSlot.Key);
			filter.ShowAmount(count);
			filter.ShowNewNotification(sellSlot.Value.Any((UIShopItemSlot it) => it.Item.IsNew));
			GetSection(sellSlot.Key).emptyIndicator.SetActive(count == 0);
			if (sellSlot.Key != CItem.EItemSlot.SmallItem && count > 0)
			{
				sellSlot.Value[count - 1].EnableDivider(enable: false);
			}
		}
		list2 = list2.Where((CItem x) => sellItems.Find((CItem y) => x.ID == y.ID) == null).ToList();
		list = list.Where((CItem x) => sellItems.Find((CItem y) => x.ID == y.ID) == null).ToList();
		int count2 = sellItems.Count;
		CreateBuySlotsGamepad(list, list2, count2);
	}

	private void CreateBuySlotsGamepad(List<CItem> buyItems, List<CItem> allItems, int lastIndex)
	{
		ILookup<int, CItem> lookup = buyItems.ToLookup((CItem it) => it.ID);
		IOrderedEnumerable<IGrouping<CItem.EItemSlot, CItem>> orderedEnumerable = from it in allItems
			group it by (it.YMLData.Slot != CItem.EItemSlot.TwoHand) ? it.YMLData.Slot : CItem.EItemSlot.OneHand into gr
			orderby gr.Key switch
			{
				CItem.EItemSlot.Head => 0, 
				CItem.EItemSlot.Body => 1, 
				CItem.EItemSlot.OneHand => 2, 
				CItem.EItemSlot.Legs => 3, 
				CItem.EItemSlot.SmallItem => 4, 
				_ => int.MaxValue, 
			}
			select gr;
		int num = 0;
		int num2 = lastIndex;
		foreach (IGrouping<CItem.EItemSlot, CItem> item2 in orderedEnumerable)
		{
			CItem.EItemSlot key = item2.Key;
			bool flag = false;
			int num3 = 0;
			SlotContent section = GetSection(key);
			bool isItem;
			List<Tuple<CItem, int>> list = (from it in item2
				group it by it.ID into it
				select new Tuple<CItem, int>(it.FirstOrDefault(), it.Count()) into it
				orderby LocalizationNameConverter.MultiLookupLocalization(it.Item1.Name, out isItem)
				select it).ToList();
			if (!buySlots.ContainsKey(key))
			{
				buySlots[key] = new List<MerchantItemDecorator>();
			}
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				CItem item = list[num4].Item1;
				UIShopItemSlot slot = GetSlot(section.container, num2);
				lookup[item.ID].ToList().Count();
				MerchantItemDecorator merchantItemDecorator = new MerchantItemDecorator(num, item, lookup.Contains(item.ID) ? lookup[item.ID].ToList() : null, service, slot, list[num4].Item2, OnSelectedItemBuy, OnHoveredItemBuy, null, service.GetBuyDiscount(), character);
				slot.transform.SetSiblingIndex(num);
				buySlots[key].Add(merchantItemDecorator);
				num++;
				num2++;
				num3 += merchantItemDecorator.Amount;
				if (!flag)
				{
					flag = merchantItemDecorator.AnyNew;
				}
			}
			if (key != CItem.EItemSlot.SmallItem)
			{
				buySlots[key][list.Count - 1].EnableDivider(enable: false);
			}
			UIShopItemFilter filter = GetFilter(key);
			filter.ShowNewNotification(flag);
			filter.ShowAmount(num3);
			section.emptyIndicator.SetActive(list.Count == 0);
		}
		allFilter.ShowAmount(buyItems.Count + lastIndex);
		for (int num5 = num2; num5 < slotPool.Count; num5++)
		{
			slotPool[num5].gameObject.SetActive(value: false);
		}
		RefreshNavigationSlots();
	}

	public void RefreshView(CMapCharacter character)
	{
		this.character = character;
		Singleton<UIItemConfirmationBox>.Instance.Hide();
		RefreshView();
	}

	public void ToggleForceHoveredCurrentItem(bool isForceHovered)
	{
		if (CurrentHoveredItemSlot != null)
		{
			CurrentHoveredItemSlot.ForceHovered(isForceHovered);
		}
	}

	private void Clear()
	{
		foreach (KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> buySlot in buySlots)
		{
			buySlot.Value.Clear();
		}
		buySlots.Clear();
		foreach (KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> sellSlot in sellSlots)
		{
			sellSlot.Value.Clear();
		}
		sellSlots.Clear();
		scroll.verticalNormalizedPosition = 1f;
		foreach (ItemListingType value in Enum.GetValues(typeof(ItemListingType)))
		{
			if (value != ItemListingType.Owned || InputManager.GamePadInUse)
			{
				UIShopItemFilter filter = GetFilter(value);
				filter.ShowAmount(0);
				filter.ShowNewNotification(show: false);
			}
		}
		foreach (CItem.EItemSlot value2 in Enum.GetValues(typeof(CItem.EItemSlot)))
		{
			GetSection(value2).emptyIndicator?.SetActive(value: true);
		}
	}

	private void ShowSell()
	{
		mode = EShopMode.Sell;
		Singleton<UIItemConfirmationBox>.Instance.Hide();
		Clear();
		List<CItem> list = ((character == null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? new List<CItem>() : service.GetItemsToSell(character));
		Dictionary<CItem, Tuple<CMapCharacter, bool>> boundItems = ((character == null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? new Dictionary<CItem, Tuple<CMapCharacter, bool>>() : service.GetBoundsItems(character));
		List<CItem> list2 = (from it in list
			orderby LocalizationManager.GetTranslation(it.YMLData.Name), boundItems.ContainsKey(it) ? ((!boundItems[it].Item2) ? 1 : 2) : 0
			select it).ToList();
		for (int num = 0; num < list2.Count; num++)
		{
			CItem cItem = list2[num];
			CItem.EItemSlot eItemSlot = ((cItem.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : cItem.YMLData.Slot);
			UIShopItemSlot slot = GetSlot(GetSection(eItemSlot).container, num);
			if (!sellSlots.ContainsKey(eItemSlot))
			{
				sellSlots[eItemSlot] = new List<UIShopItemSlot>();
			}
			sellSlots[eItemSlot].Add(slot);
			slot.Initialize(cItem, cItem.SellPrice, OnSelectedItemSell, OnHoveredItemSell, null, boundItems.ContainsKey(cItem) ? boundItems[cItem].Item1 : null, boundItems.ContainsKey(cItem) && boundItems[cItem].Item2, character);
			slot.transform.SetSiblingIndex(num);
		}
		for (int num2 = list.Count; num2 < slotPool.Count; num2++)
		{
			slotPool[num2].gameObject.SetActive(value: false);
		}
		allFilter.ShowAmount(list.Count);
		foreach (KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> sellSlot in sellSlots)
		{
			int count = sellSlot.Value.Count;
			UIShopItemFilter filter = GetFilter(sellSlot.Key);
			filter.ShowAmount(count);
			filter.ShowNewNotification(sellSlot.Value.Any((UIShopItemSlot it) => it.Item.IsNew));
			GetSection(sellSlot.Key).emptyIndicator.SetActive(count == 0);
			if (sellSlot.Key != CItem.EItemSlot.SmallItem && count > 0)
			{
				sellSlot.Value[count - 1].EnableDivider(enable: false);
			}
		}
		RefreshNavigationSlots();
	}

	private void ShowBuy()
	{
		mode = EShopMode.Buy;
		Clear();
		List<CItem> list = ((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null) ? new List<CItem>() : service.GetItemsToBuy(character));
		List<CItem> list2 = ((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null) ? list : list.Concat(service.GetItemsToSell()).ToList());
		if (character != null)
		{
			list2 = list2.FindAll((CItem it) => it.CanEquipItem(character.CharacterID));
		}
		CreateBuySlots(list, list2);
	}

	private void CreateBuySlots(List<CItem> buyItems, List<CItem> allItems)
	{
		ILookup<int, CItem> lookup = buyItems.ToLookup((CItem it) => it.ID);
		IOrderedEnumerable<IGrouping<CItem.EItemSlot, CItem>> orderedEnumerable = from it in allItems
			group it by (it.YMLData.Slot != CItem.EItemSlot.TwoHand) ? it.YMLData.Slot : CItem.EItemSlot.OneHand into gr
			orderby gr.Key switch
			{
				CItem.EItemSlot.Head => 0, 
				CItem.EItemSlot.Body => 1, 
				CItem.EItemSlot.OneHand => 2, 
				CItem.EItemSlot.Legs => 3, 
				CItem.EItemSlot.SmallItem => 4, 
				_ => int.MaxValue, 
			}
			select gr;
		int num = 0;
		foreach (IGrouping<CItem.EItemSlot, CItem> item2 in orderedEnumerable)
		{
			CItem.EItemSlot key = item2.Key;
			bool flag = false;
			int num2 = 0;
			SlotContent section = GetSection(key);
			bool isItem;
			List<Tuple<CItem, int>> list = (from it in item2
				group it by it.ID into it
				select new Tuple<CItem, int>(it.FirstOrDefault(), it.Count()) into it
				orderby LocalizationNameConverter.MultiLookupLocalization(it.Item1.Name, out isItem)
				select it).ToList();
			if (!buySlots.ContainsKey(key))
			{
				buySlots[key] = new List<MerchantItemDecorator>();
			}
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				CItem item = list[num3].Item1;
				UIShopItemSlot slot = GetSlot(section.container, num);
				MerchantItemDecorator merchantItemDecorator = new MerchantItemDecorator(num, item, lookup.Contains(item.ID) ? lookup[item.ID].ToList() : null, service, slot, list[num3].Item2, OnSelectedItemBuy, OnHoveredItemBuy, null, service.GetBuyDiscount(), character);
				slot.transform.SetSiblingIndex(num);
				buySlots[key].Add(merchantItemDecorator);
				num++;
				num2 += merchantItemDecorator.Amount;
				if (!flag)
				{
					flag = merchantItemDecorator.AnyNew;
				}
			}
			if (key != CItem.EItemSlot.SmallItem)
			{
				buySlots[key][list.Count - 1].EnableDivider(enable: false);
			}
			UIShopItemFilter filter = GetFilter(key);
			filter.ShowNewNotification(flag);
			filter.ShowAmount(num2);
			section.emptyIndicator.SetActive(list.Count == 0);
		}
		allFilter.ShowAmount(buyItems.Count);
		for (int num4 = num; num4 < slotPool.Count; num4++)
		{
			slotPool[num4].gameObject.SetActive(value: false);
		}
		RefreshNavigationSlots();
	}

	private List<UIShopItemSlot> GetSellSlots(CItem.EItemSlot slotType)
	{
		CItem.EItemSlot key = ((slotType == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : slotType);
		if (!sellSlots.ContainsKey(key))
		{
			return null;
		}
		return sellSlots[key];
	}

	private List<MerchantItemDecorator> GetBuySlots(CItem.EItemSlot slotType)
	{
		slotType = ((slotType == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : slotType);
		FFSNet.Console.LogInfo("slotType: " + slotType);
		FFSNet.Console.LogInfo("BuySlots[slot]: " + buySlots[slotType]);
		if (buySlots[slotType] != null)
		{
			FFSNet.Console.LogInfo(slotType.ToString() + " item count (different ones): " + buySlots[slotType].Count);
			foreach (MerchantItemDecorator item in buySlots[slotType])
			{
				FFSNet.Console.LogInfo(item.Item.YMLData.StringID + " (" + item.Amount + ") (Key Item NetworkID: " + item.Item.NetworkID + ").");
				if (item.Items == null)
				{
					continue;
				}
				foreach (CItem item2 in item.Items)
				{
					FFSNet.Console.LogInfo("Instance NetworkID: " + item2.NetworkID);
				}
			}
		}
		return buySlots[slotType];
	}

	private UIShopItemFilter GetFilter(CItem.EItemSlot type)
	{
		switch (type)
		{
		case CItem.EItemSlot.Legs:
			return legsFilter;
		case CItem.EItemSlot.Body:
			return bodyFilter;
		case CItem.EItemSlot.OneHand:
		case CItem.EItemSlot.TwoHand:
			return handsFilter;
		case CItem.EItemSlot.Head:
			return headFilter;
		case CItem.EItemSlot.SmallItem:
			return smallItemsFilter;
		default:
			return null;
		}
	}

	private UIShopItemFilter GetFilter(ItemListingType type)
	{
		return type switch
		{
			ItemListingType.Legs => legsFilter, 
			ItemListingType.Body => bodyFilter, 
			ItemListingType.Hands => handsFilter, 
			ItemListingType.Head => headFilter, 
			ItemListingType.SmallItems => smallItemsFilter, 
			ItemListingType.Owned => _ownedFilter, 
			_ => allFilter, 
		};
	}

	private CItem.EItemSlot GetSlotType(ItemListingType type)
	{
		return type switch
		{
			ItemListingType.Legs => CItem.EItemSlot.Legs, 
			ItemListingType.Body => CItem.EItemSlot.Body, 
			ItemListingType.Hands => CItem.EItemSlot.OneHand, 
			ItemListingType.Head => CItem.EItemSlot.Head, 
			ItemListingType.SmallItems => CItem.EItemSlot.SmallItem, 
			_ => CItem.EItemSlot.None, 
		};
	}

	private SlotContent GetSection(CItem.EItemSlot itemSlot)
	{
		switch (itemSlot)
		{
		case CItem.EItemSlot.Body:
			return bodySection;
		case CItem.EItemSlot.Head:
			return headSection;
		case CItem.EItemSlot.Legs:
			return legsSection;
		case CItem.EItemSlot.OneHand:
		case CItem.EItemSlot.TwoHand:
			return handsSection;
		default:
			return smallItemsSection;
		}
	}

	private string GetLocalizationSlot(ItemListingType itemSlot)
	{
		return itemSlot switch
		{
			ItemListingType.Body => "BODY", 
			ItemListingType.Head => "HEAD", 
			ItemListingType.Legs => "LEG", 
			ItemListingType.Hands => "HAND", 
			ItemListingType.SmallItems => "SMALL_ITEMS", 
			ItemListingType.Owned => "OWNED", 
			ItemListingType.AllGear => "ALL", 
			ItemListingType.None => "ALL", 
			_ => throw new ArgumentException("this argument has no localization"), 
		};
	}

	private UIShopItemSlot GetSlot(RectTransform container, int pos)
	{
		if (slotPool.Count - 1 < pos)
		{
			UIShopItemSlot uIShopItemSlot = UnityEngine.Object.Instantiate(slotPrefab, container);
			slotPool.Add(uIShopItemSlot);
			return uIShopItemSlot;
		}
		slotPool[pos].transform.SetParent(container);
		slotPool[pos].gameObject.SetActive(value: true);
		return slotPool[pos];
	}

	public void RefreshSellNewItemNotification(CItem item)
	{
		if (mode == EShopMode.Buy)
		{
			return;
		}
		List<UIShopItemSlot> list = GetSellSlots(item.YMLData.Slot);
		UIShopItemSlot uIShopItemSlot = list.FirstOrDefault((UIShopItemSlot it) => it.Item == item);
		if (!(uIShopItemSlot == null))
		{
			uIShopItemSlot.ShowNewItemNotification(item.IsNew);
			GetFilter(item.YMLData.Slot).ShowNewNotification(list.Exists((UIShopItemSlot it) => it.Item.IsNew));
		}
	}

	private void OnHoveredItemSell(UIShopItemSlot itemUI, bool hovered)
	{
		if (hovered)
		{
			CurrentHoveredItemSlot = itemUI;
			CItem item = itemUI.Item;
			if (item.IsNew)
			{
				service.UnmarkNewItem(item);
				GetFilter(item.YMLData.Slot).ShowNewNotification(GetSellSlots(item.YMLData.Slot).Exists((UIShopItemSlot it) => it.Item.IsNew));
			}
			itemTooltip.Show(item, itemUI.transform as RectTransform, itemUI.Owner, item.Tradeable ? null : string.Format("<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_ITEM_CANNOT_BE_SOLD") + "</color>"), item.Tradeable ? ((CItem.EItemSlotState?)null) : new CItem.EItemSlotState?(CItem.EItemSlotState.Spent));
			this.NewItemTooltipShown?.Invoke(itemTooltip);
			if (InputManager.GamePadInUse)
			{
				scroll.ScrollToFit(itemUI.transform as RectTransform);
			}
		}
		else
		{
			CurrentHoveredItemSlot = null;
			itemTooltip.Hide();
		}
	}

	private void OnHoveredItemBuy(UIShopItemSlot itemUI, bool hovered, int index)
	{
		if (hovered)
		{
			CurrentHoveredItemSlot = itemUI;
			if (!itemUI.IsAvailable)
			{
				itemTooltip.Show(itemUI.Item, itemUI.transform as RectTransform, null, "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_ITEM_SOLDOUT") + "</color>", CItem.EItemSlotState.Spent, service);
			}
			else
			{
				UIPartyItemInventoryTooltip uIPartyItemInventoryTooltip = itemTooltip;
				CItem item = itemUI.Item;
				RectTransform target = itemUI.transform as RectTransform;
				IShopItemService shopItemService = service;
				uIPartyItemInventoryTooltip.Show(item, target, null, null, null, shopItemService);
			}
			StartCoroutine(SkipAFrameAndNotifyNewItemTooltip());
			CItem.EItemSlot eItemSlot = ((itemUI.Item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : itemUI.Item.YMLData.Slot);
			GetFilter(eItemSlot).ShowNewNotification(buySlots[eItemSlot].Exists((MerchantItemDecorator it) => it.AnyNew));
			if (index == 0)
			{
				scroll.verticalNormalizedPosition = 1f;
			}
			else if (InputManager.GamePadInUse)
			{
				scroll.ScrollToFit(itemUI.transform as RectTransform);
			}
			OnNewItemBuyShown?.Invoke();
		}
		else
		{
			CurrentHoveredItemSlot = null;
			itemTooltip.Hide();
		}
	}

	private IEnumerator SkipAFrameAndNotifyNewItemTooltip()
	{
		yield return new WaitForEndOfFrame();
		this.NewItemTooltipShown?.Invoke(itemTooltip);
	}

	public void FilterShownItems(ItemListingType shownType)
	{
		bool flag = shownType == ItemListingType.AllGear;
		bool flag2 = flag || shownType == ItemListingType.Owned;
		headSection.container.gameObject.SetActive(flag2 || shownType == ItemListingType.Head);
		bodySection.container.gameObject.SetActive(flag2 || shownType == ItemListingType.Body);
		legsSection.container.gameObject.SetActive(flag2 || shownType == ItemListingType.Legs);
		handsSection.container.gameObject.SetActive(flag2 || shownType == ItemListingType.Hands);
		smallItemsSection.container.gameObject.SetActive(flag2 || shownType == ItemListingType.SmallItems);
		if (_titleText != null)
		{
			_titleText.text = LocalizationManager.GetTranslation("Consoles/GUI_INVENTORY_SLOT_HEADER_" + GetLocalizationSlot(shownType));
		}
		if (mode == EShopMode.Sell)
		{
			if (flag)
			{
				foreach (KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> sellSlot in sellSlots)
				{
					int count = sellSlot.Value.Count;
					if (sellSlot.Key != CItem.EItemSlot.SmallItem && count > 0)
					{
						sellSlot.Value[count - 1].EnableDivider(enable: false);
					}
				}
			}
			else
			{
				CItem.EItemSlot slotType = GetSlotType(shownType);
				if (slotType != CItem.EItemSlot.None && sellSlots.ContainsKey(slotType))
				{
					sellSlots[slotType].LastOrDefault()?.EnableDivider(enable: true);
				}
			}
		}
		else if (flag)
		{
			foreach (KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> buySlot in buySlots)
			{
				int count2 = buySlot.Value.Count;
				if (buySlot.Key != CItem.EItemSlot.SmallItem && count2 > 0)
				{
					buySlot.Value[count2 - 1].EnableDivider(enable: false);
				}
			}
		}
		else
		{
			CItem.EItemSlot slotType2 = GetSlotType(shownType);
			if (slotType2 != CItem.EItemSlot.None && buySlots.ContainsKey(slotType2))
			{
				buySlots[slotType2].LastOrDefault()?.EnableDivider(enable: true);
			}
		}
		HandleFilterShownItemsGamepad(shownType);
		RefreshNavigationSlots();
	}

	private void HandleFilterShownItemsGamepad(ItemListingType shownType)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		foreach (KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> buySlot in buySlots)
		{
			buySlot.Value.ForEach(delegate(MerchantItemDecorator x)
			{
				x.ToggleSlot(shownType != ItemListingType.Owned);
			});
		}
	}

	public void UpdateListingsAffordability()
	{
		if (mode != EShopMode.Buy)
		{
			return;
		}
		foreach (KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> buySlot in buySlots)
		{
			foreach (MerchantItemDecorator item in buySlot.Value)
			{
				item.UpdateAffordability(character);
			}
		}
	}

	private void OnSelectedItemSell(UIShopItemSlot slot)
	{
		if (!FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant))
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (slot.Owner != null)
			{
				CharacterYMLData characterYMLData = slot.Owner.CharacterYMLData;
				stringBuilder.AppendFormat(LocalizationManager.GetTranslation("GUI_ITEM_BOUND_TO"), UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig).ToHex(), slot.Owner.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterYMLData.LocKey) : slot.Owner.CharacterName, $"AA_{characterYMLData.Model}");
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("<color=#{2}>{0}: <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>", LocalizationManager.GetTranslation("Gain"), slot.Item.SellPrice, UIInfoTools.Instance.goldColor.ToHex());
			Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(BoxConfirmationType.Sell, LocalizationManager.GetTranslation("GUI_SELL_CONFIRMATION_TITLE"), stringBuilder.ToString(), slot.Item, delegate
			{
				SellItem(slot.Item, slot);
			}, "GUI_SELL_CONFIRMATION_TITLE", "GUI_CANCEL", audioItemSellConfirmation, delegate
			{
				if (ControllerArea.IsFocused)
				{
					EventSystem.current.SetSelectedGameObject(slot.gameObject);
				}
			});
		}
		else
		{
			FFSNet.Console.Log("Attempted to sell an item. Cannot access this feature as spectator.");
		}
	}

	private void OnSelectedItemBuy(MerchantItemDecorator slot)
	{
		if (!FFSNetwork.IsOnline || (PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant))
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (character != null)
			{
				CharacterYMLData characterYMLData = character.CharacterYMLData;
				stringBuilder.AppendFormat(LocalizationManager.GetTranslation("GUI_ITEM_BUY_FOR"), UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig).ToHex(), character.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterYMLData.LocKey) : character.CharacterName, $"AA_{characterYMLData.Model}");
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("<color=#{2}>{0}: <sprite name=\"Gold_Icon_White\" color=#{2}>{1}</color>", LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST"), service.DiscountedCost(slot.Item), UIInfoTools.Instance.goldColor.ToHex());
			Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(BoxConfirmationType.Buy, LocalizationManager.GetTranslation("GUI_BUY_CONFIRMATION_TITLE"), stringBuilder.ToString(), slot.Item, delegate
			{
				if (ControllerArea.IsFocused)
				{
					EventSystem.current.SetSelectedGameObject(slot);
				}
				BuyItem(slot, character);
			}, "GUI_BUY_CONFIRMATION_TITLE", "GUI_CANCEL", audioItemBuyConfirmation, delegate
			{
				if (ControllerArea.IsFocused)
				{
					EventSystem.current.SetSelectedGameObject(slot);
				}
			});
		}
		else
		{
			FFSNet.Console.Log("Attempted to buy an item. Cannot access this feature as spectator.");
		}
	}

	private void SellItem(CItem item, UIShopItemSlot itemUI, bool networkActionIfOnline = true)
	{
		if (itemUI.Item != item)
		{
			return;
		}
		if (FFSNetwork.IsOnline && networkActionIfOnline)
		{
			int actorID = ((itemUI.Owner == null) ? (-1) : (AdventureState.MapState.IsCampaign ? itemUI.Owner.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(itemUI.Owner.CharacterID)));
			int playerID = PlayerRegistry.MyPlayer.PlayerID;
			IProtocolToken supplementaryDataToken = new ItemToken(itemUI.Item.NetworkID, (int)itemUI.Item.YMLData.Slot);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.SellItem, ActionPhaseType.MapHQ, disableAutoReplication: false, actorID, 0, 0, playerID, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			Singleton<UIMultiplayerLockOverlay>.Instance.ShowLock(this, "GUI_MODDING_VALIDATING");
			return;
		}
		_ = itemUI.Owner;
		service.Sell(item, itemUI.Owner);
		CItem.EItemSlot slot = item.YMLData.Slot;
		List<UIShopItemSlot> list = GetSellSlots(slot);
		list.Remove(itemUI);
		itemUI.gameObject.SetActive(value: false);
		UpdateListingsAffordability();
		GetFilter(slot).ShowAmount(list.Count);
		GetSection(slot).emptyIndicator.SetActive(list.IsNullOrEmpty());
		allFilter.ShowAmount(sellSlots.Sum((KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> it) => it.Value.Count));
		if (slot != CItem.EItemSlot.SmallItem && list.Count > 0)
		{
			list[list.Count - 1].EnableDivider(enable: false);
		}
		SelectFirstElement();
		ReplicateItems();
		if (InputManager.GamePadInUse)
		{
			RefreshView();
		}
	}

	private void BuyItem(MerchantItemDecorator itemUI, CMapCharacter targetCharacter = null, uint targetItemNetworkID = 0u, bool equip = true)
	{
		if (FFSNetwork.IsOnline && targetItemNetworkID == 0)
		{
			int actorID = ((character != null) ? (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID)) : 0);
			int playerID = PlayerRegistry.MyPlayer.PlayerID;
			IProtocolToken supplementaryDataToken = new ItemToken(itemUI.Items[itemUI.Items.Count - 1].NetworkID, (int)itemUI.Item.YMLData.Slot);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.BuyItem, ActionPhaseType.MapHQ, disableAutoReplication: false, actorID, 0, 0, playerID, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			return;
		}
		CItem.EItemSlot slot = ((itemUI.Item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : itemUI.Item.YMLData.Slot);
		itemUI.Buy(targetCharacter, targetItemNetworkID, equip);
		UpdateListingsAffordability();
		List<CItem> list = ((AdventureState.MapState.GoldMode == EGoldMode.CharacterGold && character == null) ? new List<CItem>() : service.GetItemsToBuy(character));
		GetFilter(slot).ShowAmount(list.Count((CItem it) => it.CanBeAssignedToSlot(slot)));
		allFilter.ShowAmount(list.Count);
		ReplicateItems();
		if (InputManager.GamePadInUse)
		{
			RefreshView();
		}
	}

	public void UpdateItemBound(CItem item, CMapCharacter character)
	{
		if (mode == EShopMode.Buy)
		{
			return;
		}
		CItem.EItemSlot eItemSlot = ((item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : item.YMLData.Slot);
		if (sellSlots.ContainsKey(eItemSlot))
		{
			UIShopItemSlot uIShopItemSlot = sellSlots[eItemSlot].FirstOrDefault((UIShopItemSlot it) => it.Item.ItemGuid == item.ItemGuid);
			if (!(uIShopItemSlot == null))
			{
				uIShopItemSlot.UpdateOwner(character);
				RefreshOrder(eItemSlot);
			}
		}
	}

	public void UpdateItemEquipped(CItem item)
	{
		if (mode == EShopMode.Buy)
		{
			return;
		}
		CItem.EItemSlot eItemSlot = ((item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : item.YMLData.Slot);
		if (sellSlots.ContainsKey(eItemSlot))
		{
			UIShopItemSlot uIShopItemSlot = sellSlots[eItemSlot].FirstOrDefault((UIShopItemSlot it) => it.Item.ItemGuid == item.ItemGuid);
			if (!(uIShopItemSlot == null))
			{
				uIShopItemSlot.UpdateOwner(uIShopItemSlot.Owner, isEquipped: true);
				RefreshOrder(eItemSlot);
			}
		}
	}

	public void UpdateItemUnbound(CItem item)
	{
		if (mode == EShopMode.Buy)
		{
			return;
		}
		CItem.EItemSlot key = ((item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : item.YMLData.Slot);
		if (sellSlots.ContainsKey(key))
		{
			UIShopItemSlot uIShopItemSlot = sellSlots[key].FirstOrDefault((UIShopItemSlot it) => it.Item.ItemGuid == item.ItemGuid);
			if (!(uIShopItemSlot == null))
			{
				uIShopItemSlot.UpdateOwner(null);
				RefreshOrder((item.YMLData.Slot == CItem.EItemSlot.TwoHand) ? CItem.EItemSlot.OneHand : item.YMLData.Slot);
			}
		}
	}

	private void RefreshOrder(CItem.EItemSlot slotType)
	{
		int count = sellSlots[slotType].Count;
		if (slotType != CItem.EItemSlot.SmallItem && count > 0)
		{
			sellSlots[slotType][count - 1].EnableDivider(enable: true);
		}
		sellSlots[slotType] = (from it in sellSlots[slotType]
			orderby it.Name, (it.Owner != null) ? ((!it.IsEquipped) ? 1 : 2) : 0
			select it).ToList();
		for (int num = 0; num < count; num++)
		{
			sellSlots[slotType][num].transform.SetSiblingIndex(num + 1);
		}
		if (slotType != CItem.EItemSlot.SmallItem && count > 0)
		{
			sellSlots[slotType][count - 1].EnableDivider(enable: false);
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}

	private UIShopItemFilter GetSelectedFilter()
	{
		foreach (ItemListingType value in Enum.GetValues(typeof(ItemListingType)))
		{
			if ((value != ItemListingType.Owned || InputManager.GamePadInUse) && value != ItemListingType.None)
			{
				UIShopItemFilter filter = GetFilter(value);
				if (filter.tab.isOn)
				{
					return filter;
				}
			}
		}
		return null;
	}

	public void MPBuyItem(GameAction action, ref bool actionValid, bool shopWindowOpen)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		ItemToken itemToken = (ItemToken)action.SupplementaryDataToken;
		if (service == null)
		{
			service = new ShopService(AdventureState.MapState.MapParty, delegate(CItem x)
			{
				Singleton<UIShopItemWindow>.Instance.OnUpdateNewPartyItemNotification?.Invoke(x);
			});
		}
		if (FFSNetwork.IsHost)
		{
			SimpleLog.AddToSimpleLog("Host is validating player:" + ((player != null) ? player.Username : "NULL PLAYER") + " buying an item. NetworkID: " + itemToken.ItemNetworkID + ", ItemSlot: " + (CItem.EItemSlot)itemToken.ItemSlotID/*cast due to .constrained prefix*/);
			CItem cItem = AdventureState.MapState.HeadquartersState.CheckMerchantStock.SingleOrDefault((CItem it) => it.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				CMapCharacter mapCharacter = null;
				switch (AdventureState.MapState.GoldMode)
				{
				case EGoldMode.PartyGold:
					if (AdventureState.MapState.MapParty.PartyGold < service.DiscountedCost(cItem))
					{
						SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. Not enough gold to buy " + cItem.YMLData.StringID + ". Party gold: " + AdventureState.MapState.MapParty.PartyGold + ", item cost: " + service.DiscountedCost(cItem));
						actionValid = false;
						return;
					}
					break;
				case EGoldMode.CharacterGold:
				{
					string targetCharacterID = null;
					if (action.ActorID != 0)
					{
						targetCharacterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
						if (targetCharacterID != null)
						{
							mapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == targetCharacterID);
						}
					}
					if (mapCharacter != null)
					{
						if (PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.CreatingCharacter != null && it.CreatingCharacter.CharacterName == mapCharacter.CharacterName))
						{
							SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. MapCharacter (" + mapCharacter.CharacterID + " " + mapCharacter.CharacterName + ") given is still considered as being created by a player");
							actionValid = false;
							return;
						}
						if (mapCharacter.CharacterGold < service.DiscountedCost(cItem))
						{
							SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. Not enough gold to buy " + cItem.YMLData.StringID + ". Character gold: " + mapCharacter.CharacterGold + ", item cost: " + service.DiscountedCost(cItem));
							actionValid = false;
							return;
						}
						break;
					}
					SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. Unable to find map character");
					actionValid = false;
					return;
				}
				}
				if (shopWindowOpen && mode == EShopMode.Buy)
				{
					MerchantItemDecorator merchantItemDecorator = GetBuySlots((CItem.EItemSlot)itemToken.ItemSlotID)?.SingleOrDefault((MerchantItemDecorator x) => x.Items != null && x.Items.Exists((CItem y) => y.NetworkID == itemToken.ItemNetworkID));
					if (merchantItemDecorator != null)
					{
						if (Singleton<UIItemConfirmationBox>.Instance.IsConfirmingItem(merchantItemDecorator.Item))
						{
							Singleton<UIItemConfirmationBox>.Instance.Hide();
						}
						BuyItem(merchantItemDecorator, mapCharacter, itemToken.ItemNetworkID);
					}
					else
					{
						SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. Unable to find merchant slot of item");
						actionValid = false;
					}
				}
				else
				{
					AdventureState.MapState.HeadquartersState.RemoveItemFromMerchantStock(cItem);
					switch (AdventureState.MapState.GoldMode)
					{
					case EGoldMode.PartyGold:
						AdventureState.MapState.MapParty.ModifyPartyGold(-service.DiscountedCost(cItem), useGoldModifier: true);
						AdventureState.MapState.MapParty.AddItem(cItem);
						break;
					case EGoldMode.CharacterGold:
						mapCharacter?.ModifyGold(-service.DiscountedCost(cItem), useGoldModifier: true);
						AdventureState.MapState.MapParty.BindItem(mapCharacter.CharacterID, mapCharacter.CharacterName, cItem, equip: true);
						break;
					default:
						throw new Exception("Error buying item. Gold mode not determined.");
					}
					ReplicateItems();
					SaveData.Instance.SaveCurrentAdventureData();
					if (InputManager.GamePadInUse)
					{
						ShowItemsGamepad();
					}
					else if (shopWindowOpen && mode == EShopMode.Sell)
					{
						ShowSell();
					}
				}
				actionValid = true;
				SimpleLog.AddToSimpleLog("MP BUY ITEM - Host verified successful purchase of " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ")" + ((mapCharacter != null) ? (" for CharacterID: " + mapCharacter.CharacterID) : "") + " by Player: " + ((player != null) ? player.Username : PlayerRegistry.MyPlayer.Username));
			}
			else
			{
				actionValid = false;
				SimpleLog.AddToSimpleLog("BUY ITEM ACTION DENIED. Unable to buy item because it doesn't exist in Merchant Stock. Another player may have bought it just beforehand.");
			}
		}
		else
		{
			ShowInventory(shopWindowOpen);
			actionValid = true;
		}
	}

	public void MPSellItem(GameAction action, ref bool actionValid, bool shopWindowOpen)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		ItemToken itemToken = (ItemToken)action.SupplementaryDataToken;
		if (service == null)
		{
			service = new ShopService(AdventureState.MapState.MapParty, delegate(CItem x)
			{
				Singleton<UIShopItemWindow>.Instance.OnUpdateNewPartyItemNotification?.Invoke(x);
			});
		}
		if (FFSNetwork.IsHost)
		{
			SimpleLog.AddToSimpleLog("MP SELL ITEM - Host is validating player:" + ((player != null) ? player.Username : "NULL PLAYER") + " selling an item. NetworkID: " + itemToken.ItemNetworkID + ", ItemSlot: " + (CItem.EItemSlot)itemToken.ItemSlotID/*cast due to .constrained prefix*/);
			CItem cItem = AdventureState.MapState.MapParty.GetAllPartyItems(includeMultiplayerItemReserve: true).SingleOrDefault((CItem x) => x.NetworkID == itemToken.ItemNetworkID);
			if (cItem != null)
			{
				UIShopItemSlot uIShopItemSlot = GetSellSlots((CItem.EItemSlot)itemToken.ItemSlotID)?.SingleOrDefault((UIShopItemSlot x) => x.Item.NetworkID == itemToken.ItemNetworkID);
				if (shopWindowOpen && mode == EShopMode.Sell && uIShopItemSlot != null)
				{
					if (Singleton<UIItemConfirmationBox>.Instance.IsConfirmingItem(cItem))
					{
						Singleton<UIItemConfirmationBox>.Instance.Hide();
					}
					SellItem(cItem, uIShopItemSlot, networkActionIfOnline: false);
					actionValid = true;
					CMapCharacter owner = uIShopItemSlot.Owner;
					SimpleLog.AddToSimpleLog("MP SELL ITEM - Host verified successful sale of " + uIShopItemSlot.Item.YMLData.StringID + " (NetworkID: " + uIShopItemSlot.Item.NetworkID + ") by Player: " + ((player != null) ? player.Username : "NULL PLAYER") + ((owner != null) ? (" for CharacterID: " + owner.CharacterID) : ""));
				}
				else
				{
					CMapCharacter targetCharacter = null;
					if (AdventureState.MapState.IsCampaign)
					{
						targetCharacter = AdventureState.MapState.GetMapCharacterWithCharacterNameHash(action.ActorID);
					}
					else if (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold || action.ActorID != -1)
					{
						targetCharacter = AdventureState.MapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == CharacterClassManager.GetCharacterIDFromModelInstanceID(action.ActorID));
					}
					if (PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.CreatingCharacter != null && it.CreatingCharacter.CharacterName == targetCharacter.CharacterName))
					{
						SimpleLog.AddToSimpleLog("SELL ITEM ACTION DENIED. MapCharacter (" + targetCharacter.CharacterID + " " + targetCharacter.CharacterName + ") given is still considered as being created by a player");
						actionValid = false;
						return;
					}
					if (targetCharacter != null || AdventureState.MapState.GoldMode == EGoldMode.PartyGold)
					{
						if (AdventureState.MapState.GoldMode == EGoldMode.PartyGold || targetCharacter.AllCharacterItems.Contains(cItem))
						{
							service.Sell(cItem, targetCharacter);
							ReplicateItems();
							SaveData.Instance.SaveCurrentAdventureData();
							ShowInventory(shopWindowOpen);
							actionValid = true;
							SimpleLog.AddToSimpleLog("MP SELL ITEM - Host verified successful sale of " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ")(Contained in MultiplayerItemReserve) by Player: " + ((player != null) ? player.Username : "NULL PLAYER") + ((targetCharacter != null) ? (" for CharacterID: " + targetCharacter.CharacterID) : ""));
						}
						else
						{
							actionValid = false;
							SimpleLog.AddToSimpleLog("MP SELL ITEM ACTION DENIED. Target Character CharacterID: " + targetCharacter.CharacterID + " inventory does not contain the item: " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ")");
						}
					}
					else
					{
						actionValid = false;
						SimpleLog.AddToSimpleLog("MP SELL ITEM ACTION DENIED. Unable to find character selling the item " + cItem.YMLData.StringID + " (NetworkID: " + cItem.NetworkID + ") matching controllable ID: " + action.ActorID);
					}
				}
			}
			else
			{
				actionValid = false;
				SimpleLog.AddToSimpleLog("MP SELL ITEM ACTION DENIED. Unable to sell item (NetworkID: " + itemToken.ItemNetworkID + ") because it doesn't exist in party items.");
			}
		}
		else
		{
			ShowInventory(shopWindowOpen);
			actionValid = true;
		}
		Singleton<UIMultiplayerLockOverlay>.Instance.HideLock(this);
	}

	private void ShowInventory(bool shopWindowOpen)
	{
		if (InputManager.GamePadInUse)
		{
			ShowItemsGamepad();
		}
		else if (shopWindowOpen && mode == EShopMode.Sell)
		{
			ShowSell();
		}
		else if (shopWindowOpen && mode == EShopMode.Buy)
		{
			ShowBuy();
		}
	}

	public void ReplicateItems()
	{
		if (!FFSNetwork.IsOnline || !FFSNetwork.IsHost)
		{
			return;
		}
		foreach (CMapCharacter mapCharacter in AdventureState.MapState.MapParty.CheckCharacters)
		{
			if (!AdventureState.MapState.IsCampaign || (mapCharacter.PersonalQuest != null && !PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.CreatingCharacter != null && it.CreatingCharacter.CharacterName == mapCharacter.CharacterName)))
			{
				int controllableID = (AdventureState.MapState.IsCampaign ? mapCharacter.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(mapCharacter.CharacterID));
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ItemInventoryToken(mapCharacter, AdventureState.MapState.MapParty, AdventureState.MapState.GoldMode);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			else
			{
				SimpleLog.AddToSimpleLog("Unable to replicate character item inventory as the character (ID: " + mapCharacter.CharacterID + " Name: " + mapCharacter.CharacterName + ") is still being created.");
			}
		}
		AdventureState.MapState.MapParty.MultiplayerSoldItems.Clear();
	}

	private void Navigate(UIShopItemSlot slot, MoveDirection direction)
	{
		if (direction != MoveDirection.Left && direction != MoveDirection.Right)
		{
			return;
		}
		ItemListingType filter = GetSelectedFilter().filter;
		if (direction == MoveDirection.Left)
		{
			if (filter == ItemListingType.AllGear)
			{
				smallItemsFilter.tab.isOn = true;
			}
			else
			{
				GetFilter(filter - 1).tab.isOn = true;
			}
		}
		else if (filter == ItemListingType.SmallItems)
		{
			allFilter.tab.isOn = true;
		}
		else
		{
			GetFilter(filter + 1).tab.isOn = true;
		}
	}

	private void EnableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Merchant);
		RefreshNavigationSlots();
		_shopFilterInputListener.Register();
		SetHotkeysActive(value: true);
		if (_controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = true;
		}
	}

	private void DisableNavigation()
	{
		if (_shopFilterInputListener != null)
		{
			_shopFilterInputListener.UnRegister();
		}
		SetHotkeysActive(value: false);
		if (_controllerInputScroll != null)
		{
			_controllerInputScroll.enabled = false;
		}
	}

	public void EnablePartialHide()
	{
		if (_shopItemInventoryCanvasGroup != null)
		{
			_shopItemInventoryCanvasGroup.alpha = 0f;
		}
	}

	public void DisablePartialHide()
	{
		if (_shopItemInventoryCanvasGroup != null)
		{
			_shopItemInventoryCanvasGroup.alpha = 1f;
		}
	}

	private void RefreshNavigationSlots()
	{
		if (!ControllerArea.IsFocused)
		{
			return;
		}
		UIShopItemFilter selectedFilter = GetSelectedFilter();
		CItem.EItemSlot slotType = GetSlotType(selectedFilter.filter);
		Selectable selectable = null;
		if (mode == EShopMode.Sell)
		{
			List<UIShopItemSlot> list = ((slotType == CItem.EItemSlot.None) ? sellSlots.OrderBy((KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> it) => GetSection(it.Key).container.GetSiblingIndex()).SelectMany((KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> it) => it.Value).ToList() : (sellSlots.ContainsKey(slotType) ? sellSlots[slotType] : null));
			if (!list.IsNullOrEmpty())
			{
				selectable = list.FirstOrDefault()?.Selectable;
			}
		}
		else
		{
			List<MerchantItemDecorator> list2 = ((slotType == CItem.EItemSlot.None) ? buySlots.OrderBy((KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> it) => GetSection(it.Key).container.GetSiblingIndex()).SelectMany((KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> it) => it.Value).ToList() : (buySlots.ContainsKey(slotType) ? buySlots[slotType] : null));
			if (!list2.IsNullOrEmpty())
			{
				selectable = list2.FirstOrDefault();
			}
		}
		if (selectable != null && selectable.TryGetComponent<IUiNavigationSelectable>(out var component))
		{
			this.OnItemsRefreshed?.Invoke(component);
		}
		else if (_currentHoveredItemSlot != null)
		{
			_currentHoveredItemSlot.OnHovered(hovered: false);
		}
	}

	private void SelectFirstElement()
	{
		if (!ControllerArea.IsFocused)
		{
			return;
		}
		UIShopItemFilter selectedFilter = GetSelectedFilter();
		CItem.EItemSlot slotType = GetSlotType(selectedFilter.filter);
		GameObject gameObject = null;
		if (mode == EShopMode.Sell)
		{
			UIShopItemSlot uIShopItemSlot = ((slotType == CItem.EItemSlot.None) ? sellSlots.OrderBy((KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> it) => GetSection(it.Key).container.GetSiblingIndex()).SelectMany((KeyValuePair<CItem.EItemSlot, List<UIShopItemSlot>> it) => it.Value).FirstOrDefault() : sellSlots[slotType].FirstOrDefault());
			if (uIShopItemSlot != null)
			{
				gameObject = uIShopItemSlot.gameObject;
			}
		}
		else
		{
			MerchantItemDecorator merchantItemDecorator = ((slotType == CItem.EItemSlot.None) ? buySlots.OrderBy((KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> it) => GetSection(it.Key).container.GetSiblingIndex()).SelectMany((KeyValuePair<CItem.EItemSlot, List<MerchantItemDecorator>> it) => it.Value).FirstOrDefault() : buySlots[slotType].FirstOrDefault());
			if (merchantItemDecorator != null)
			{
				gameObject = merchantItemDecorator;
			}
		}
		if (gameObject != null && gameObject.TryGetComponent<IUiNavigationSelectable>(out var component))
		{
			this.OnItemsRefreshed?.Invoke(component);
		}
	}

	private void HighlightItemTooltip(UINavigationDirection direction)
	{
		if (direction == UINavigationDirection.Left && !itemTooltip.IsHighlighted)
		{
			itemTooltip.Highlight(state: true);
		}
	}

	public void HighlightItemTooltip(bool state)
	{
		itemTooltip.Highlight(state);
	}

	public void ToggleItemTextTooltip()
	{
		Singleton<UIShopItemWindow>.Instance.ItemInventory.itemTooltip.ToggleTextTooltip();
	}

	public void ToggleBackground(bool isEnabled)
	{
		_backgroundBlackoutObject.SetActive(isEnabled);
	}
}
