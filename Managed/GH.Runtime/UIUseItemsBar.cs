#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIUseItemsBar : Singleton<UIUseItemsBar>
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseItemScenario itemSlotPrefab;

	[SerializeField]
	private ControllerInputItemsArea controllerInputItemsArea;

	private Dictionary<CItem, UIUseItemScenario> itemSlots = new Dictionary<CItem, UIUseItemScenario>();

	private Stack<UIUseItemScenario> pool = new Stack<UIUseItemScenario>();

	private Func<CItem, bool> condition;

	private Action<CItem, bool> onClickItem;

	private bool isShown;

	private string tooltip;

	private string tooltipTitle;

	private Transform lastCreatedSlot;

	private CActor actor;

	private CItem useItem;

	private readonly string ItemNameMinorManaPotion = "ITEM_NAME_MinorManaPotion";

	private readonly string ItemNameMajorManaPotion = "ITEM_NAME_MajorManaPotion";

	[SerializeField]
	private UnityEvent OnChangedItemsAvailable;

	private List<CItem> unavailableItems = new List<CItem>();

	public bool IsShown => isShown;

	public ControllerInputItemsArea ControllerInputItemsArea => controllerInputItemsArea;

	public Dictionary<CItem, UIUseItemScenario> ItemSlots => itemSlots;

	public void AddItem(CItem item)
	{
		if (!isShown || !actor.Inventory.AllItems.Contains(item) || (condition != null && !condition(item)))
		{
			return;
		}
		if (!CanConsume(item))
		{
			if (!unavailableItems.Contains(item))
			{
				unavailableItems.Add(item);
			}
			return;
		}
		if (!itemSlots.ContainsKey(item))
		{
			UIUseItemScenario slotFromPool = GetSlotFromPool();
			itemSlots[item] = slotFromPool;
			slotFromPool.Show();
			unavailableItems.Remove(item);
			itemSlots[item].SetItem(item, delegate(CItem it)
			{
				onClickItem(it, arg2: true);
			}, delegate(CItem it)
			{
				onClickItem(it, arg2: true);
			}, actor, null, CardsHandManager.Instance.GetHand(actor as CPlayerActor).PickUnselectedInfusionsForItem(item), SetUseItem, ClearUseItem);
			itemSlots[item].SetInteractable(interactable: true, updateLook: true);
		}
		else
		{
			itemSlots[item].gameObject.SetActive(value: true);
		}
		RefreshTooltip();
		OnChangedItemsAvailable.Invoke();
	}

	private void OnItemBackClick()
	{
		bool flag = false;
		if (useItem.Name == ItemNameMinorManaPotion || useItem.Name == ItemNameMajorManaPotion)
		{
			flag = true;
		}
		itemSlots[useItem].ClearSelectionNew();
		ClearUseItem();
		if (InputManager.GamePadInUse)
		{
			controllerInputItemsArea.Focus();
		}
		else if (ItemSlots.Count != 0)
		{
			SetActiveItemButtons(IsCurrentItemReadyToUse());
		}
		if (flag)
		{
			PhaseManager.SetNextPhase(CPhase.PhaseType.ActionSelection);
		}
	}

	public void SetActiveItemButtons(bool value)
	{
		if (InputManager.GamePadInUse)
		{
			controllerInputItemsArea.SetActiveTabInput(!value);
		}
		Choreographer.s_Choreographer.readyButton.ClearAlternativeAction();
		if (value)
		{
			Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMITEM, LocalizationManager.GetTranslation("GUI_CONFIRM"));
			Choreographer.s_Choreographer.readyButton.QueueAlternativeAction(UseItem);
		}
		else
		{
			Choreographer.s_Choreographer.readyButton.Toggle(active: false, ReadyButton.EButtonState.EREADYBUTTONNA, null, hideOnClick: true, glowingEffect: false, interactable: true, disregardTurnControlForInteractability: false, haltActionProcessorIfDeactivated: false);
		}
		Choreographer.s_Choreographer.m_UndoButton.ClearOnClickOverriders();
		if (value)
		{
			Choreographer.s_Choreographer.m_UndoButton.SkipNextNetworkAction = true;
			Choreographer.s_Choreographer.m_UndoButton.SetOnClickOverrider(OnItemBackClick, UndoButton.EButtonState.EUNDOBUTTONUNDO, previousEnabled: false, previousInteractable: false, LocalizationManager.GetTranslation("GUI_UNDO"));
		}
		else
		{
			Choreographer.s_Choreographer.m_UndoButton.SkipNextNetworkAction = false;
			Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
		}
		Choreographer.s_Choreographer.m_SkipButton.ClearSkipAction();
		if (value)
		{
			SkipButton skipButton = Choreographer.s_Choreographer.m_SkipButton;
			string translation = LocalizationManager.GetTranslation("GUI_SKIP");
			Action onSkipAction = UseItemWithoutEffects;
			skipButton.Toggle(active: true, translation, hideOnClick: true, null, onSkipAction);
		}
		else
		{
			Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
		}
	}

	public bool IsCurrentItemReadyToUse()
	{
		if (useItem != null)
		{
			UIUseItemScenario uIUseItemScenario = itemSlots[useItem];
			if (uIUseItemScenario.ConsumesNum == 0 && uIUseItemScenario.InfusionsNum == 0)
			{
				return false;
			}
			if (uIUseItemScenario.InfusionsNum != 0)
			{
				return uIUseItemScenario.IsAllInfusionsPicked();
			}
			if (uIUseItemScenario.ConsumesNum != 0)
			{
				return uIUseItemScenario.IsAllConsumesPicked();
			}
		}
		return false;
	}

	public void SetUseItem(CItem item)
	{
		useItem = item;
	}

	public void ClearUseItem()
	{
		useItem = null;
	}

	private void UseItem()
	{
		if (useItem != null)
		{
			if (FFSNetwork.IsOnline && actor.IsUnderMyControl && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ItemToken(useItem.NetworkID, -1, int.MaxValue, useItem.ChosenElement, itemSlots[useItem].GetSelectedInfusions());
				Synchronizer.SendGameAction(GameActionType.ClickItemBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			itemSlots[useItem].ConsumeOrInfuseIfPossible();
			new UseItemService(actor).UseItem(useItem, networkActionIfOnline: true, itemSlots[useItem].GetSelectedInfusions(), saveState: false);
			ClearUseItem();
		}
	}

	private void UseItemWithoutEffects()
	{
		if (useItem != null)
		{
			if (FFSNetwork.IsOnline && actor.IsUnderMyControl && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation)
			{
				ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
				int iD = actor.ID;
				IProtocolToken supplementaryDataToken = new ItemToken(useItem.NetworkID);
				Synchronizer.SendGameAction(GameActionType.ClickItemBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			}
			new UseItemService(actor).UseItem(useItem, networkActionIfOnline: true, null, saveState: false);
			ClearUseItem();
		}
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (CItem item in unavailableItems.ToList())
		{
			if (CanConsume(item))
			{
				unavailableItems.Remove(item);
				AddItem(item);
			}
		}
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> item in itemSlots.ToList())
		{
			if (!item.Value.IsSelected() && !CanConsume(item.Key))
			{
				RemoveItem(item.Key);
				if (unavailableItems.Contains(item.Key))
				{
					unavailableItems.Add(item.Key);
				}
			}
		}
	}

	private bool CanConsume(CItem item)
	{
		List<ElementInfusionBoardManager.EElement> list = item.YMLData.Consumes.ToList();
		if (item.YMLData.Data.Abilities != null)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in item.YMLData.Data.Abilities.Where((CAbility it) => it is CAbilityConsumeElement).SelectMany((CAbility it) => (it as CAbilityConsumeElement).ElementsToConsume))
			{
				int num = list.LastIndexOf(item2);
				if (num != -1)
				{
					list.RemoveAt(num);
				}
			}
		}
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		if (availableElements.Count < list.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement item3 in list)
		{
			if (item3 != ElementInfusionBoardManager.EElement.Any && !availableElements.Contains(item3))
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItem(CItem item, bool clear = false)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].gameObject.SetActive(value: false);
			unavailableItems.Remove(item);
			if (clear)
			{
				RemoveItemSlot(item, clearSelection: true);
			}
			RefreshTooltip();
			OnChangedItemsAvailable.Invoke();
		}
	}

	private void RemoveItemSlot(CItem item, bool clearSelection)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].Hide();
			if (clearSelection)
			{
				itemSlots[item].ClearSelection();
			}
			pool.Push(itemSlots[item]);
			itemSlots.Remove(item);
		}
	}

	public void RefreshItem(CItem item)
	{
		RefreshOrRemovedItem(item, clearIfRemoved: false);
	}

	private void RefreshOrRemovedItem(CItem item, bool clearIfRemoved)
	{
		if (itemSlots.ContainsKey(item))
		{
			if (condition != null && !condition(item) && !itemSlots[item].IsSelected())
			{
				RemoveItem(item, clearIfRemoved);
			}
			else
			{
				itemSlots[item].Refresh();
			}
		}
	}

	public void SetItemInteractable(CItem item, bool interactable)
	{
		if (itemSlots.ContainsKey(item))
		{
			itemSlots[item].SetInteractable(interactable);
		}
	}

	public void SetItemsInteractable(bool enable, List<CItem> excludedItems = null)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
		{
			if (excludedItems == null || !excludedItems.Contains(itemSlot.Key))
			{
				itemSlot.Value.SetInteractable(enable, updateLook: true);
			}
		}
	}

	private UIUseItemScenario GetSlotFromPool()
	{
		UIUseItemScenario uIUseItemScenario;
		if (pool.Count > 0)
		{
			uIUseItemScenario = pool.Pop();
			uIUseItemScenario.gameObject.SetActive(value: true);
			return uIUseItemScenario;
		}
		uIUseItemScenario = UnityEngine.Object.Instantiate(itemSlotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseItemScenario.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseItemScenario.transform;
		return uIUseItemScenario;
	}

	public void ShowItems(CActor actor, List<CItem> items, Func<CItem, bool> condition = null, Action<CItem, bool> onClickItem = null, bool resetShow = false, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		this.onClickItem = onClickItem;
		this.condition = condition;
		this.tooltip = tooltip;
		this.tooltipTitle = tooltipTitle;
		this.actor = actor;
		isShown = true;
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance.OnReservedElement.AddListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.AddListener(OnUnreservedElement);
		if (resetShow)
		{
			Clear();
		}
		else
		{
			foreach (CItem item in itemSlots.Keys.ToList())
			{
				RefreshItem(item);
			}
		}
		foreach (CItem item2 in items)
		{
			AddItem(item2);
		}
	}

	public void ShowItems(CActor inventoryOwner, Func<CItem, bool> condition = null, Action<CItem, bool> onClickItem = null, bool clear = false, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE")
	{
		ShowItems(inventoryOwner, inventoryOwner.Inventory.AllItems, condition, action, clear, tooltip, tooltipTitle);
		void action(CItem item, bool networkActionIfOnline)
		{
			if (onClickItem != null)
			{
				if (FFSNetwork.IsOnline && inventoryOwner.IsUnderMyControl && ActionProcessor.CurrentPhase != ActionPhaseType.TakeDamageConfirmation)
				{
					ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
					int iD = inventoryOwner.ID;
					IProtocolToken supplementaryDataToken = new ItemToken(item.NetworkID, -1, int.MaxValue, item.ChosenElement, itemSlots[item].GetSelectedInfusions());
					Synchronizer.SendGameAction(GameActionType.ClickItemBonusSlot, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, iD, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
				}
				onClickItem(item, networkActionIfOnline);
			}
		}
	}

	public void ShowUsableItems(CActor inventoryOwner, Func<CItem, bool> condition = null, string tooltip = null, string tooltipTitle = "GUI_USE_ITEM_TITLE", bool hideIfEmpty = false)
	{
		if (!FFSNetwork.IsOnline || inventoryOwner.Inventory.AllItems.Any(IsItemInteractable))
		{
			ShowItems(inventoryOwner, inventoryOwner.Inventory.AllItems, delegate(CItem item)
			{
				if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Useable && item.SlotState != CItem.EItemSlotState.Locked)
				{
					return false;
				}
				return condition == null || condition(item);
			}, delegate(CItem item, bool networkActionIfOnline)
			{
				new UseItemService(inventoryOwner).UseItem(item, networkActionIfOnline, itemSlots[item].GetSelectedInfusions());
			}, resetShow: false, tooltip, tooltipTitle);
			return;
		}
		actor = inventoryOwner;
		if (hideIfEmpty)
		{
			Hide(resetSlots: false);
			if (useItem != null)
			{
				itemSlots[useItem].ClearSelectionNew();
				ClearUseItem();
			}
		}
	}

	private bool IsItemInteractable(CItem element)
	{
		bool result = false;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			if (FFSNetwork.IsOnline)
			{
				if (element.YMLData.Slot == CItem.EItemSlot.QuestItem)
				{
					result = actor.IsUnderMyControl;
				}
				else
				{
					CMapCharacter cMapCharacter = SaveData.Instance.Global.CampaignData.AdventureMapState.MapParty.IsItemEquippedByParty(element);
					result = cMapCharacter != null && PlayerRegistry.MyPlayer.HasControlOver(cMapCharacter.CharacterName.GetHashCode());
				}
			}
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			if (SaveData.Instance.Global?.AdventureData?.AdventureMapState?.MapParty == null)
			{
				Debug.LogError("Adventure data is null");
			}
			if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer == null)
			{
				Debug.LogError("My player is null");
			}
			CMapCharacter cMapCharacter2 = SaveData.Instance.Global.AdventureData.AdventureMapState.MapParty.IsItemEquippedByParty(element);
			Debug.LogFormat("Item owner is {0}", cMapCharacter2?.CharacterID);
			result = !FFSNetwork.IsOnline || (cMapCharacter2 != null && PlayerRegistry.MyPlayer.HasControlOver(CharacterClassManager.GetModelInstanceIDFromCharacterID(cMapCharacter2.CharacterID)));
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.SingleScenario || SaveData.Instance.Global.GameMode == EGameMode.FrontEndTutorial || SaveData.Instance.Global.GameMode == EGameMode.Autotest)
		{
			PlayerState playerWithItem = SaveData.Instance.Global.CurrentCustomLevelData.ScenarioState.Players.FirstOrDefault((PlayerState p) => p.Items.Contains(element));
			if (playerWithItem != null && CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.CharacterID == playerWithItem.ClassID) != null)
			{
				CPlayerActor cPlayerActor = (CPlayerActor)playerWithItem.Actor;
				int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? cPlayerActor.CharacterName.GetHashCode() : cPlayerActor.CharacterClass.ModelInstanceID);
				result = !FFSNetwork.IsOnline || PlayerRegistry.MyPlayer.HasControlOver(controllableID);
			}
		}
		return result;
	}

	public void Clear()
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
		{
			if (itemSlot.Key.SlotState == CItem.EItemSlotState.Selected)
			{
				itemSlot.Value.ClearSelection();
			}
			itemSlot.Value.gameObject.SetActive(value: false);
			pool.Push(itemSlot.Value);
		}
		if (itemSlots.Count > 0)
		{
			OnChangedItemsAvailable.Invoke();
		}
		itemSlots.Clear();
		unavailableItems.Clear();
	}

	public void Hide(bool resetSlots = true)
	{
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		if (resetSlots)
		{
			Clear();
		}
		else
		{
			foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
			{
				itemSlot.Value.gameObject.SetActive(value: false);
			}
			OnChangedItemsAvailable.Invoke();
		}
		isShown = false;
		tooltip = null;
	}

	public void RefreshItems(bool clear = false)
	{
		foreach (CItem item in itemSlots.Keys.ToList())
		{
			RefreshOrRemovedItem(item, clear);
		}
		RefreshTooltip();
	}

	private bool AreItemsUsable()
	{
		return itemSlots.Any((KeyValuePair<CItem, UIUseItemScenario> it) => it.Value.gameObject.activeSelf);
	}

	public void RefreshTooltip()
	{
		if (isShown && tooltip.IsNOTNullOrEmpty() && AreItemsUsable())
		{
			Singleton<HelpBox>.Instance.ShowControllerOrKeyboardTip(LocalizationManager.GetTranslation(tooltip), Singleton<HelpBox>.Instance.ControllerText, LocalizationManager.GetTranslation(tooltipTitle ?? "GUI_USE_ITEM_TITLE"), Singleton<HelpBox>.Instance.ControllerTitle, "USE_ITEM_BAR");
		}
		else
		{
			Singleton<HelpBox>.Instance.HideKeyboard("USE_ITEM_BAR", onlyIfIdMatches: true);
		}
	}

	public void SetTooltip(string tooltip, string tooltipTitle)
	{
		this.tooltip = tooltip;
		this.tooltipTitle = tooltipTitle;
		RefreshTooltip();
	}

	public void ProxyUseItemBonus(GameAction action)
	{
		ProxyUseItemBonus((action.SupplementaryDataToken as ItemToken).ItemNetworkID, action.SupplementaryDataToken as ItemToken);
	}

	public void ProxyUseItemBonus(uint itemNetworkID, ItemToken itemToken)
	{
		foreach (KeyValuePair<CItem, UIUseItemScenario> itemSlot in itemSlots)
		{
			FFSNet.Console.Log("Slotted Item " + itemSlot.Key.Name + " (NetworkID: " + itemSlot.Key.NetworkID + ", ItemGuid: " + itemSlot.Key.ItemGuid + ").");
		}
		if (!isShown)
		{
			CItem cItem = actor.Inventory.AllItems.Find((CItem cItem2) => cItem2.NetworkID == itemToken.ItemNetworkID);
			List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>(itemToken.ChosenElement.Length);
			int[] chosenElement = itemToken.ChosenElement;
			foreach (int item in chosenElement)
			{
				list.Add((ElementInfusionBoardManager.EElement)item);
			}
			cItem.ChosenElement = list;
			List<ElementInfusionBoardManager.EElement> list2 = new List<ElementInfusionBoardManager.EElement>(itemToken.InfusionElements.Length);
			chosenElement = itemToken.InfusionElements;
			foreach (int item2 in chosenElement)
			{
				list2.Add((ElementInfusionBoardManager.EElement)item2);
			}
			if (list2.Count > 0)
			{
				ElementInfusionBoardManager.Infuse(list2, actor);
				InfusionBoardUI.Instance.UpdateBoard(list2);
			}
			if (list.Count > 0)
			{
				InfusionBoardUI.Instance.ReserveElements(list, active: true);
			}
			new UseItemService(actor).UseItem(cItem, networkActionIfOnline: false, null, saveState: false);
			return;
		}
		KeyValuePair<CItem, UIUseItemScenario> tObj = itemSlots.FirstOrDefault((KeyValuePair<CItem, UIUseItemScenario> x) => x.Key.NetworkID == itemNetworkID);
		if (!tObj.IsTNull())
		{
			if (!tObj.Value.IsSelected())
			{
				List<ElementInfusionBoardManager.EElement> list3 = new List<ElementInfusionBoardManager.EElement>(itemToken.ChosenElement.Length);
				for (int num2 = 0; num2 < itemToken.ChosenElement.Length; num2++)
				{
					list3.Add((ElementInfusionBoardManager.EElement)itemToken.ChosenElement[num2]);
				}
				tObj.Key.ChosenElement = list3;
				List<ElementInfusionBoardManager.EElement> list4 = new List<ElementInfusionBoardManager.EElement>(itemToken.InfusionElements.Length);
				for (int num3 = 0; num3 < itemToken.InfusionElements.Length; num3++)
				{
					list4.Add((ElementInfusionBoardManager.EElement)itemToken.InfusionElements[num3]);
				}
				tObj.Value.SetInfusions(list4);
				tObj.Value.SetAnyConsume(list3);
			}
			tObj.Value.OnPointerDown();
			return;
		}
		throw new Exception("Error using activatable item bonus for proxy playerActor. No such bonus found with item NetworkID: " + itemNetworkID);
	}
}
