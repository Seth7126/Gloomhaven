using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using Code.State;
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
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.HotkeysBehaviour;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.SpecialStates;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPartyItemInventoryDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private Image iconTypeInventory;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private ScrollRect itemsPanel;

	[SerializeField]
	private UIPartyItemSlot slotPrefab;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private UIPartyItemInventoryTooltip itemTooltip;

	[SerializeField]
	private string audioItemDisplay = "PlaySound_UISubTabOpen";

	[SerializeField]
	private string audioItemUnbind = "PlaySound_UIGenericBuy";

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private LocalHotkeys _hotkeyContainer;

	private LTDescr animationDisplay;

	[SerializeField]
	private RectTransform _tooltipContainer;

	[SerializeField]
	private Hotkey _scrollHotKeyGamepad;

	private IPartyItemEquipmentService service;

	private UIWindow window;

	private List<UIPartyItemSlot> itemSlotPool = new List<UIPartyItemSlot>();

	private Dictionary<CItem, UIPartyItemSlot> assignedSlots = new Dictionary<CItem, UIPartyItemSlot>();

	private CMapCharacter character;

	private Action<CItem> onItemEquipped;

	private Action<CItem, int> onItemRemoved;

	private Action<CItem, bool> onItemHovered;

	private int slotNum;

	private UIPartyItemSlot selectedSlot;

	protected UIPartyItemSlot hoveredSlot;

	private CItem.EItemSlot slotType;

	private UIPartyItemSlot selectedItemToCofirmSwitch;

	private bool canInteractWithItems = true;

	private IHotkeySession _hotkeySession;

	private SessionHotkey _selectHotkey;

	private SessionHotkey _unselectHotkey;

	public bool IsVisible => window.IsVisible;

	public UnityEvent OnHidden => window.onHidden;

	[UsedImplicitly]
	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHide);
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		_scrollHotKeyGamepad.TryEnableHotkey();
		showAnimation.OnAnimationFinished.AddListener(delegate
		{
			if (controllerArea.IsFocused)
			{
				EnterInventoryState();
			}
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		ClearAll();
	}

	private void EnterInventoryState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Inventory, (selectedSlot == null) ? null : new SelectionStateData(selectedSlot.NavigationSelectable));
	}

	public void Display(CMapCharacter character, IPartyItemEquipmentService service, CItem.EItemSlot slotType, int slotNum, RectTransform pointTo, CItem selectedItem = null, Action<CItem> onItemEquipped = null, Action<CItem, int> onItemRemoved = null, Action<CItem, bool> onItemHovered = null, bool resetOpen = true, bool editAllowed = true)
	{
		ClearAll();
		verticalPointer.PointAt(pointTo);
		this.onItemEquipped = onItemEquipped;
		this.character = character;
		this.service = service;
		this.slotNum = slotNum;
		this.slotType = slotType;
		this.onItemRemoved = onItemRemoved;
		this.onItemHovered = onItemHovered;
		selectedItemToCofirmSwitch = null;
		infoText.text = LocalizationManager.GetTranslation("GUI_ITEM_SLOT_" + slotType.ToString().ToUpper());
		iconTypeInventory.sprite = UIInfoTools.Instance.GetItemSlotIcon(slotType.ToString());
		InitializeHotkeys();
		CreateItems(slotType, slotNum, selectedItem, editAllowed);
		service.RegisterOnAddedItemParty(OnNewItemAdded);
		service.RegisterOnRemovedItemParty(OnItemRemoved);
		service.RegisterOnRemovedNewItemFlag(RefreshNewItemNotification);
		service.RegisterOnBindedItemToCharacter(OnItemBinded);
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
		if (resetOpen || !IsVisible)
		{
			AudioControllerUtils.PlaySound(audioItemDisplay);
			window.Show();
			showAnimation.Play();
			Singleton<UINavigation>.Instance.StateMachine.Enter(SpecialStateTag.Lock);
		}
		controllerArea.Enable();
	}

	public void OnSwitchToTooltip()
	{
		if (hoveredSlot != null)
		{
			hoveredSlot.SetMarked();
		}
	}

	private void ClearEvents()
	{
		if (service != null)
		{
			service.UnregisterOnAddedItemParty(OnNewItemAdded);
			service.UnregisterOnRemovedItemParty(OnItemRemoved);
			service.UnregisterOnRemovedNewItemFlag(RefreshNewItemNotification);
			service.UnregisterOnBindedItemToCharacter(OnItemBinded);
		}
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnershipChanged));
	}

	private void OnControllableOwnershipChanged(NetworkControllable controllable, NetworkPlayer oldcontroller, NetworkPlayer newcontroller)
	{
		if (selectedItemToCofirmSwitch != null)
		{
			Singleton<UIItemConfirmationBox>.Instance.Hide();
		}
		foreach (UIPartyItemSlot value in assignedSlots.Values)
		{
			value.RefreshOwnerInteraction();
		}
	}

	private void OnNewItemAdded(CItem item)
	{
		if (item.CanBeAssignedToSlot(slotType) && service.CanEquip(item, character))
		{
			int count = assignedSlots.Count;
			if (itemSlotPool.Count <= count)
			{
				itemSlotPool.Add(UnityEngine.Object.Instantiate(slotPrefab, itemsPanel.content));
			}
			UIPartyItemSlot uIPartyItemSlot = itemSlotPool[count];
			uIPartyItemSlot.SetItem(character, item, null, OnItemSelected, OnItemDeselected, OnItemHovered);
			assignedSlots[item] = uIPartyItemSlot;
			List<UIPartyItemSlot> list = (from it in assignedSlots.Values
				orderby LocalizationManager.GetTranslation(it.Item.YMLData.Name), (it.Owner != null) ? ((!(it.Owner?.CharacterID == character.CharacterID)) ? 1 : 2) : 0
				select it).ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].transform.SetSiblingIndex(num);
			}
			itemSlotPool.RemoveRange(0, assignedSlots.Count);
			itemSlotPool.InsertRange(0, list);
			uIPartyItemSlot.gameObject.SetActive(value: true);
		}
	}

	private void OnItemBinded(CItem item, CMapCharacter newOwner)
	{
		if (assignedSlots.ContainsKey(item))
		{
			assignedSlots[item].SetOwner(newOwner);
		}
	}

	private void OnItemRemoved(CItem item, int slotIndex)
	{
		if (item.CanBeAssignedToSlot(slotType) && assignedSlots.ContainsKey(item))
		{
			UIPartyItemSlot uIPartyItemSlot = assignedSlots[item];
			assignedSlots.Remove(item);
			uIPartyItemSlot.gameObject.SetActive(value: false);
			itemSlotPool.Remove(uIPartyItemSlot);
			itemSlotPool.Insert(itemSlotPool.Count, uIPartyItemSlot);
			uIPartyItemSlot.transform.SetAsLastSibling();
			if (uIPartyItemSlot == selectedSlot)
			{
				selectedSlot = null;
			}
			onItemRemoved?.Invoke(item, slotIndex);
		}
	}

	private void RefreshNewItemNotification(CItem item)
	{
		if (assignedSlots.ContainsKey(item))
		{
			assignedSlots[item].RefreshNewNotification();
		}
	}

	private void CreateItems(CItem.EItemSlot slotType, int slotNum, CItem selectedItem = null, bool interactable = true)
	{
		assignedSlots.Clear();
		List<CItem> unboundItemsForSlot = service.GetUnboundItemsForSlot(slotType, character);
		Dictionary<CItem, Tuple<CMapCharacter, bool>> boundItems = service.GetBoundAndEquippedItemsForSlot(slotType, (AdventureState.MapState.GoldMode == EGoldMode.CharacterGold) ? character : null);
		foreach (KeyValuePair<CItem, Tuple<CMapCharacter, bool>> item in boundItems)
		{
			unboundItemsForSlot.Add(item.Key);
		}
		unboundItemsForSlot = (from it in unboundItemsForSlot
			orderby LocalizationManager.GetTranslation(it.YMLData.Name), boundItems.ContainsKey(it) ? ((!boundItems[it].Item2) ? 1 : 2) : 0
			select it).ToList();
		List<CItem> list = character.CheckEquippedItems.FindAll((CItem it) => it.CanBeAssignedToSlot(slotType));
		selectedSlot = null;
		hoveredSlot = null;
		HelperTools.NormalizePool(ref itemSlotPool, slotPrefab.gameObject, itemsPanel.content, unboundItemsForSlot.Count);
		for (int num = 0; num < unboundItemsForSlot.Count; num++)
		{
			CItem cItem = unboundItemsForSlot[num];
			CMapCharacter cMapCharacter = null;
			bool isInOtherSlot = false;
			if (boundItems.ContainsKey(cItem))
			{
				cMapCharacter = boundItems[cItem].Item1;
				if (cMapCharacter.CharacterID == character.CharacterID && list.Contains(cItem))
				{
					isInOtherSlot = cItem.SlotIndex != slotNum;
				}
			}
			itemSlotPool[num].SetItem(character, cItem, cMapCharacter, OnItemSelected, OnItemDeselected, OnItemHovered, isInOtherSlot, interactable);
			assignedSlots[cItem] = itemSlotPool[num];
			if (cItem == selectedItem)
			{
				selectedSlot = itemSlotPool[num];
			}
		}
		UIPartyItemSlot slot = selectedSlot;
		if (selectedSlot == null)
		{
			itemsPanel.verticalNormalizedPosition = 1f;
			if (unboundItemsForSlot.Count > 0)
			{
				slot = assignedSlots[unboundItemsForSlot.First()];
			}
		}
		else
		{
			selectedSlot.SetSelected(selected: true);
			itemsPanel.ScrollToFit(selectedSlot.transform as RectTransform);
		}
		UpdateSlotHotkeys(slot);
	}

	public bool CanShowTooltip()
	{
		return itemTooltip.CanShowTooltip();
	}

	public void ToggleItemTextTooltip()
	{
		itemTooltip.ToggleTextTooltip();
	}

	public void HideItemTextTooltip()
	{
		itemTooltip.HideItemTextTooltip();
	}

	public void SetInteractionAvailable(bool isActive)
	{
		canInteractWithItems = isActive;
		UpdateSlotHotkeys();
	}

	private void UpdateSlotHotkeys()
	{
		UpdateSlotHotkeys(hoveredSlot);
	}

	private void UpdateSlotHotkeys(UIPartyItemSlot slot)
	{
		if (_hotkeySession != null)
		{
			if (slot != null && canInteractWithItems)
			{
				bool isSelected = slot.IsSelected;
				_selectHotkey.SetShown(!isSelected);
				_unselectHotkey.SetShown(isSelected);
			}
			else
			{
				_selectHotkey.Hide();
				_unselectHotkey.Hide();
			}
		}
	}

	private void InitializeHotkeys()
	{
		_hotkeySession = _hotkeyContainer.GetSessionOrEmpty().AddOrReplaceHotkeys("Back").GetHotkey(out _selectHotkey, "Select")
			.GetHotkey(out _unselectHotkey, "Unselect");
	}

	private void DisposeHotkeys()
	{
		if (_hotkeySession != null)
		{
			_selectHotkey.Dispose();
			_unselectHotkey.Dispose();
			_hotkeySession.Dispose();
			_hotkeySession = null;
		}
	}

	private void OnItemHovered(UIPartyItemSlot itemSlot, bool hovered)
	{
		if (hovered)
		{
			hoveredSlot = itemSlot;
			if (InputManager.GamePadInUse)
			{
				itemsPanel.ScrollToFit(itemSlot.transform as RectTransform);
			}
			string information = null;
			if (character.CharacterID == itemSlot.Owner?.CharacterID && itemSlot.IsEquippedInOtherSlot)
			{
				information = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_EQUIPPED_DIFFERENT_SLOT"), string.Format(LocalizationManager.GetTranslation("GUI_INVENTORY_SLOT_" + itemSlot.Item.YMLData.Slot), itemSlot.Item.SlotIndex + 1));
			}
			RectTransform target = ((_tooltipContainer != null) ? _tooltipContainer : (itemSlot.transform as RectTransform));
			itemTooltip.Show(itemSlot.Item, target, itemSlot.Owner, information);
		}
		else
		{
			itemTooltip.Hide();
		}
		UpdateSlotHotkeys();
		onItemHovered?.Invoke(itemSlot.Item, hovered);
	}

	private void OnItemSelected(UIPartyItemSlot itemSlot)
	{
		selectedItemToCofirmSwitch = null;
		bool flag = itemSlot.Owner?.CharacterID == character.CharacterID;
		if (character.ContainsEquipedItemWithId(itemSlot.Item) && (!flag || !character.CheckEquippedItems.Contains(itemSlot.Item)))
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_BIND_ITEM_ERROR_REPEATED"), "GUI_CANCEL", delegate
			{
				if (controllerArea.IsFocused)
				{
					Singleton<UINavigation>.Instance.NavigationManager.TrySelect(itemSlot.GetComponentInChildren<UINavigationSelectable>());
				}
			});
		}
		else if (itemSlot.Owner == null)
		{
			selectedItemToCofirmSwitch = itemSlot;
			string information = (itemSlot.Item.Tradeable ? string.Format(LocalizationManager.GetTranslation("GUI_BIND_EQUIP_ITEM_CONFIRMATION"), itemSlot.Item.SellPrice) : null);
			Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(BoxConfirmationType.General, LocalizationManager.GetTranslation("GUI_BIND_EQUIP_ITEM_CONFIRMATION_TITLE"), information, itemSlot.Item, delegate
			{
				if (controllerArea.IsFocused)
				{
					Singleton<UINavigation>.Instance.NavigationManager.TrySelect(selectedItemToCofirmSwitch.GetComponentInChildren<UINavigationSelectable>());
				}
				selectedItemToCofirmSwitch = null;
				BindAndEquipItem(itemSlot);
			}, "GUI_BIND_ITEM", "GUI_CANCEL", "", delegate
			{
				if (controllerArea.IsFocused)
				{
					Singleton<UINavigation>.Instance.NavigationManager.TrySelect(selectedItemToCofirmSwitch.GetComponentInChildren<UINavigationSelectable>());
				}
				selectedItemToCofirmSwitch = null;
			});
		}
		else if (flag)
		{
			EquipItem(itemSlot);
			OnItemHovered(itemSlot, hovered: true);
			if (FFSNetwork.IsOnline && character.IsUnderMyControl)
			{
				int num = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
				if (FFSNetwork.IsClient)
				{
					ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
					IProtocolToken supplementaryDataToken = new ItemToken(itemSlot.Item.NetworkID, -1, slotNum);
					Synchronizer.SendGameAction(GameActionType.EquipItem, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, num, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
				else
				{
					ActionPhaseType currentPhase2 = ActionProcessor.CurrentPhase;
					IProtocolToken supplementaryDataToken = new ItemInventoryToken(character, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode);
					Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase2, num, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
			}
		}
		else
		{
			if (FFSNetwork.IsOnline)
			{
				return;
			}
			if (!service.CanUnbound(itemSlot.Owner, itemSlot.Item))
			{
				itemSlot.ShowWarningCost(show: true);
				NewPartyDisplayUI.PartyDisplay.ShowWarningGold(show: true, (AdventureState.MapState.GoldMode != EGoldMode.CharacterGold) ? null : itemSlot.Owner?.CharacterID);
				return;
			}
			selectedItemToCofirmSwitch = itemSlot;
			CharacterYMLData characterYMLData = itemSlot.Owner.CharacterYMLData;
			string translation = LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey);
			string title = string.Format(LocalizationManager.GetTranslation("GUI_BIND_MOVE_ITEM_CONFIRMATION_TITLE"), translation);
			string information2 = string.Format(LocalizationManager.GetTranslation("GUI_BIND_MOVE_ITEM_CONFIRMATION"), UIInfoTools.Instance.GetCharacterColor(characterYMLData.Model, characterYMLData.CustomCharacterConfig).ToHex(), LocalizationManager.GetTranslation(characterYMLData.LocKey), $"AA_{characterYMLData.Model}", translation, itemSlot.Item.SellPrice);
			Singleton<UIItemConfirmationBox>.Instance.ShowConfirmation(BoxConfirmationType.General, title, information2, itemSlot.Item, delegate
			{
				if (controllerArea.IsFocused)
				{
					Singleton<UINavigation>.Instance.NavigationManager.TrySelect(selectedItemToCofirmSwitch.GetComponentInChildren<UINavigationSelectable>());
				}
				selectedItemToCofirmSwitch = null;
				BindAndEquipItem(itemSlot);
			}, "GUI_BIND_ITEM", "GUI_CANCEL", audioItemUnbind, delegate
			{
				if (controllerArea.IsFocused)
				{
					Singleton<UINavigation>.Instance.NavigationManager.TrySelect(selectedItemToCofirmSwitch.GetComponentInChildren<UINavigationSelectable>());
				}
				selectedItemToCofirmSwitch = null;
			});
		}
	}

	private void OnItemDeselected(UIPartyItemSlot itemSlot, bool networkActionIfOnline = true)
	{
		if (!service.CanUnequip(itemSlot.Item, itemSlot.Owner))
		{
			return;
		}
		if (itemSlot == selectedSlot)
		{
			selectedSlot = null;
		}
		int slotIndex = itemSlot.Item.SlotIndex;
		character.UnequipItems(new List<CItem> { itemSlot.Item });
		itemSlot.RemoveEquipped();
		onItemRemoved?.Invoke(itemSlot.Item, slotIndex);
		SaveData.Instance.SaveCurrentAdventureData();
		UpdateSlotHotkeys();
		if (FFSNetwork.IsOnline && character.IsUnderMyControl && networkActionIfOnline)
		{
			int num = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			if (FFSNetwork.IsClient)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ItemToken(itemSlot.Item.NetworkID, -1, slotIndex);
				Synchronizer.SendGameAction(GameActionType.UnequipItem, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: true, num, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			else
			{
				ActionPhaseType currentPhase2 = ActionProcessor.CurrentPhase;
				IProtocolToken supplementaryDataToken = new ItemInventoryToken(character, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.MapParty, SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.GoldMode);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyItemInventory, currentPhase2, num, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
		}
		FFSNet.Console.Log(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(PlayerRegistry.MyPlayer) + " unequipped " + itemSlot.Item.YMLData.StringID + " (NetworkID: " + itemSlot.Item.NetworkID + ").", customFlag: true);
	}

	public void EquipItem(UIPartyItemSlot itemSlot)
	{
		if (itemSlot.Owner != null && itemSlot.Owner?.CharacterID == character.CharacterID)
		{
			CheckItemLimitSlot(itemSlot.Item);
		}
		if (selectedSlot != null)
		{
			OnItemDeselected(selectedSlot, networkActionIfOnline: false);
		}
		AudioControllerUtils.PlaySound(UIInfoTools.Instance.GetItemConfig(itemSlot.Item.YMLData.Art).toggleAudioItem);
		selectedSlot = itemSlot;
		character.EquipItem(itemSlot.Item, slotNum);
		itemSlot.AssignOwner(character);
		onItemEquipped?.Invoke(itemSlot.Item);
		SaveData.Instance.SaveCurrentAdventureData();
		FFSNet.Console.Log(character.CharacterID + " " + PlayerRegistry.GetPlayerIdentifierString(PlayerRegistry.MyPlayer) + " equipped " + itemSlot.Item.YMLData.StringID + " (NetworkID: " + itemSlot.Item.NetworkID + ").", customFlag: true);
	}

	private void BindAndEquipItem(UIPartyItemSlot itemSlot)
	{
		if (itemSlot.Owner != null && itemSlot.Owner?.CharacterID != character.CharacterID)
		{
			service.UnbindItem(itemSlot.Owner, itemSlot.Item);
		}
		if (FFSNetwork.IsOnline)
		{
			int actorID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			IProtocolToken supplementaryDataToken = new ItemToken(itemSlot.Item.NetworkID, -1, slotNum);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.BindAndEquipItem, currentPhase, disableAutoReplication: true, actorID, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
		else
		{
			service.BindItem(character, itemSlot.Item);
			OnItemBinded(itemSlot.Item, character);
			EquipItem(itemSlot);
		}
	}

	public UIPartyItemSlot GetAssignedSlot(CItem item)
	{
		return assignedSlots.SingleOrDefault((KeyValuePair<CItem, UIPartyItemSlot> x) => x.Key.ItemGuid == item.ItemGuid).Value;
	}

	private void CheckItemLimitSlot(CItem item)
	{
		List<CItem> checkEquippedItems = character.CheckEquippedItems;
		if (item.YMLData.Slot == CItem.EItemSlot.SmallItem)
		{
			if (checkEquippedItems.Contains(item))
			{
				UIPartyItemSlot itemSlot = itemSlotPool.First((UIPartyItemSlot it) => it.Item == item && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot, networkActionIfOnline: false);
			}
		}
		else if (item.YMLData.Slot == CItem.EItemSlot.OneHand)
		{
			if (checkEquippedItems.Contains(item))
			{
				UIPartyItemSlot itemSlot2 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == item && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot2, networkActionIfOnline: false);
			}
			CItem twoHands = checkEquippedItems.FirstOrDefault((CItem it) => it.YMLData.Slot == CItem.EItemSlot.TwoHand);
			if (twoHands != null)
			{
				UIPartyItemSlot itemSlot3 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == twoHands && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot3, networkActionIfOnline: false);
			}
		}
		else
		{
			if (item.YMLData.Slot != CItem.EItemSlot.TwoHand)
			{
				return;
			}
			foreach (CItem oneHand in checkEquippedItems.FindAll((CItem it) => it.YMLData.Slot == CItem.EItemSlot.OneHand))
			{
				UIPartyItemSlot itemSlot4 = itemSlotPool.First((UIPartyItemSlot it) => it.Item == oneHand && it.gameObject.activeSelf);
				OnItemDeselected(itemSlot4, networkActionIfOnline: false);
			}
		}
	}

	private void CancelAnimations()
	{
		if (animationDisplay != null)
		{
			LeanTween.cancel(animationDisplay.id);
		}
		animationDisplay = null;
	}

	public void Hide()
	{
		ClearAll();
		window.Hide();
	}

	private void OnHide()
	{
		if (selectedItemToCofirmSwitch != null)
		{
			Singleton<UIItemConfirmationBox>.Instance.Hide();
		}
		selectedItemToCofirmSwitch = null;
		ClearAll();
		controllerArea.Destroy();
	}

	private void OnDisable()
	{
		controllerArea.Destroy();
		ClearAll();
	}

	private void ClearAll()
	{
		onItemEquipped = null;
		onItemRemoved = null;
		onItemHovered = null;
		ClearEvents();
		CancelAnimations();
		DisposeHotkeys();
	}

	private void EnableNavigation()
	{
		if (!showAnimation.IsPlaying)
		{
			EnterInventoryState();
		}
	}

	private void DisableNavigation()
	{
	}

	public void FocusController()
	{
		if (window.IsOpen)
		{
			controllerArea.Focus();
		}
	}
}
