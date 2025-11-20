#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class ItemsUI : MonoBehaviour
{
	[SerializeField]
	protected GameObject itemPrefab;

	[SerializeField]
	protected List<GameObject> unfocusedMasks;

	protected CActor m_Actor;

	private UIItemScenario m_Head;

	private UIItemScenario m_Body;

	private UIItemScenario m_Legs;

	private UIItemScenario m_Hand1;

	private UIItemScenario m_Hand2;

	private UIItemScenario[] m_SmallItems;

	private UIItemScenario[] m_QuestItems;

	private List<InteractabilityIsolatedUIControl> m_IsolatableControls = new List<InteractabilityIsolatedUIControl>();

	[UsedImplicitly]
	private void OnDestroy()
	{
		Unsubscribe();
	}

	public void Toggle(bool active)
	{
		List<UIItemScenario> list = new List<UIItemScenario>(5 + m_SmallItems.Length + m_QuestItems.Length);
		list.Add(m_Head);
		list.Add(m_Body);
		list.Add(m_Legs);
		list.Add(m_Hand1);
		list.Add(m_Hand2);
		list.AddRange(m_SmallItems);
		list.AddRange(m_QuestItems);
		foreach (UIItemScenario item in list)
		{
			item.gameObject.SetActive(active);
		}
		RefreshItemsState();
	}

	private void SetItemUIState(CItem item)
	{
		try
		{
			switch (item.YMLData.Slot)
			{
			case CItem.EItemSlot.Head:
				m_Head.RefreshItemState();
				break;
			case CItem.EItemSlot.Body:
				m_Body.RefreshItemState();
				break;
			case CItem.EItemSlot.Legs:
				m_Legs.RefreshItemState();
				break;
			case CItem.EItemSlot.TwoHand:
				m_Hand1.RefreshItemState();
				m_Hand2.RefreshItemState();
				break;
			case CItem.EItemSlot.OneHand:
				if (m_Hand1.Item != null && m_Hand1.Item.ID == item.ID)
				{
					m_Hand1.RefreshItemState();
				}
				else if (m_Hand2.Item != null && m_Hand2.Item.ID == item.ID)
				{
					m_Hand2.RefreshItemState();
				}
				break;
			case CItem.EItemSlot.SmallItem:
				m_SmallItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == item.ItemGuid)?.RefreshItemState();
				break;
			case CItem.EItemSlot.QuestItem:
				m_QuestItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == item.ItemGuid)?.RefreshItemState();
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void OnItemAdded(object o, EventArgs e)
	{
		Debug.Log("OnItemAdded: " + (o as CItem).Name);
		CItem cItem = o as CItem;
		RefreshItems(cItem.YMLData.Slot);
		RefreshNavigation();
	}

	private void OnItemRefreshed(object o, EventArgs e)
	{
		Debug.Log("OnItemRefreshed: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RefreshItem(o as CItem);
	}

	private void OnItemUseable(object o, EventArgs e)
	{
		Debug.Log("OnItemUseable: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.AddItem(o as CItem);
	}

	private void OnItemNoLongerUseable(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Debug.Log("OnItemNoLongerUseable: " + cItem.Name + " " + cItem.SlotState);
		SetItemUIState(cItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemSelected(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Debug.Log("OnItemSelected: " + cItem.Name);
		SetItemUIState(cItem);
		Singleton<UIUseItemsBar>.Instance.RefreshItem(cItem);
		ElementInfusionBoardManager.SaveState();
		InfusionBoardUI.Instance.SaveState();
		WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
		WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
	}

	private void OnItemDeselected(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Debug.Log("OnItemDeselected: " + cItem.Name);
		SetItemUIState(cItem);
		ResetItemElements(cItem);
		if (!cItem.ChosenElement.IsNullOrEmpty())
		{
			List<ElementInfusionBoardManager.EElement> elements = cItem.ChosenElement.FindAll((ElementInfusionBoardManager.EElement it) => it != ElementInfusionBoardManager.EElement.Any);
			InfusionBoardUI.Instance.ReserveElements(elements, active: false);
			cItem.ChosenElement.Clear();
		}
		else
		{
			InfusionBoardUI.Instance.ReserveElements(cItem.YMLData.Consumes, active: false);
		}
		InfusionBoardUI.Instance.UpdateBoard();
		Singleton<UIUseItemsBar>.Instance.RefreshItem(cItem);
		WorldspaceStarHexDisplay.Instance.ClearCachedAbilityTiles();
		WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
	}

	private void OnItemSpent(object o, EventArgs e)
	{
		Debug.Log("OnItemSpent: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemConsumed(object o, EventArgs e)
	{
		Debug.Log("OnItemConsumed: " + (o as CItem).Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemUnrestrictedUsed(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Debug.Log("OnItemUnrestrictedUsed: " + cItem.Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void OnItemActive(object o, EventArgs e)
	{
		CItem cItem = o as CItem;
		Debug.Log("OnItemActive: " + cItem.Name);
		SetItemUIState(o as CItem);
		Singleton<UIUseItemsBar>.Instance.RemoveItem(o as CItem);
	}

	private void RefreshItems(CItem.EItemSlot slot = CItem.EItemSlot.None)
	{
		bool flag = false;
		if (m_Hand2 != null)
		{
			m_Hand2.enabled = true;
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.Head)
		{
			if (m_Head == null)
			{
				m_Head = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
				m_Head.SetItem(m_Actor, CItem.EItemSlot.Head);
				flag = true;
			}
			else
			{
				m_Head.Refresh();
			}
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.Body)
		{
			if (m_Body == null)
			{
				m_Body = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
				m_Body.SetItem(m_Actor, CItem.EItemSlot.Body);
				flag = true;
			}
			else
			{
				m_Body.Refresh();
			}
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.Legs)
		{
			if (m_Legs == null)
			{
				m_Legs = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
				m_Legs.SetItem(m_Actor, CItem.EItemSlot.Legs);
				flag = true;
			}
			else
			{
				m_Legs.Refresh();
			}
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.TwoHand || slot == CItem.EItemSlot.OneHand)
		{
			if (m_Actor.Inventory.TwoHandSlot != null)
			{
				if (m_Hand1 == null)
				{
					m_Hand1 = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					flag = true;
				}
				if (m_Hand1.Slot != CItem.EItemSlot.TwoHand)
				{
					m_Hand1.SetItem(m_Actor, CItem.EItemSlot.TwoHand);
				}
				else
				{
					m_Hand1.Refresh();
				}
				if (m_Hand2 == null)
				{
					m_Hand2 = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					flag = true;
				}
				if (m_Hand2.Slot != CItem.EItemSlot.TwoHand)
				{
					m_Hand2.SetItem(m_Actor, CItem.EItemSlot.TwoHand);
				}
				else
				{
					m_Hand2.Refresh();
				}
			}
			else
			{
				if (m_Hand1 == null)
				{
					m_Hand1 = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					flag = true;
				}
				if (m_Hand1.Slot != CItem.EItemSlot.OneHand)
				{
					m_Hand1.SetItem(m_Actor, CItem.EItemSlot.OneHand);
				}
				else
				{
					m_Hand1.Refresh();
				}
				if (m_Hand2 == null)
				{
					m_Hand2 = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					flag = true;
				}
				if (m_Hand2.Slot != CItem.EItemSlot.OneHand)
				{
					m_Hand2.SetItem(m_Actor, CItem.EItemSlot.OneHand, 1);
				}
				else
				{
					m_Hand2.Refresh();
				}
			}
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.SmallItem)
		{
			if (m_SmallItems == null)
			{
				m_SmallItems = new UIItemScenario[m_Actor.Inventory.SmallItemMax];
				int num = m_Actor.Inventory.SmallItemMax - m_Actor.Inventory.SmallItemOverride;
				for (int i = 0; i < m_Actor.Inventory.SmallItemOverride; i++)
				{
					m_SmallItems[i] = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					m_SmallItems[i].SetItem(m_Actor, CItem.EItemSlot.SmallItem, i + num);
					flag = true;
				}
				for (int j = 0; j < num; j++)
				{
					m_SmallItems[j + m_Actor.Inventory.SmallItemOverride] = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					m_SmallItems[j + m_Actor.Inventory.SmallItemOverride].SetItem(m_Actor, CItem.EItemSlot.SmallItem, j);
					flag = true;
				}
			}
			else
			{
				UIItemScenario[] smallItems = m_SmallItems;
				for (int k = 0; k < smallItems.Length; k++)
				{
					smallItems[k].Refresh();
				}
			}
		}
		if (slot == CItem.EItemSlot.None || slot == CItem.EItemSlot.QuestItem)
		{
			if (m_QuestItems == null)
			{
				m_QuestItems = new UIItemScenario[m_Actor.Inventory.QuestItemMax];
				for (int l = 0; l < m_Actor.Inventory.QuestItemMax; l++)
				{
					m_QuestItems[l] = UnityEngine.Object.Instantiate(itemPrefab, base.transform).GetComponent<UIItemScenario>();
					m_QuestItems[l].SetItem(m_Actor, CItem.EItemSlot.QuestItem, l);
					m_QuestItems[l].gameObject.SetActive(m_QuestItems[l].Item != null);
					flag = true;
				}
			}
			else
			{
				UIItemScenario[] smallItems = m_QuestItems;
				foreach (UIItemScenario uIItemScenario in smallItems)
				{
					uIItemScenario.Refresh();
					uIItemScenario.gameObject.SetActive(uIItemScenario.Item != null);
				}
			}
		}
		if (flag)
		{
			ClearOutInteractabilityControls();
			SetUpInteractabilityControls();
		}
		SetUnfocused(FFSNetwork.IsOnline && !m_Actor.IsUnderMyControl);
		RefreshNavigation();
	}

	public void SetActor(CActor actor, bool subscribeToEvents = true)
	{
		if (actor != m_Actor)
		{
			if (m_Actor != null)
			{
				Unsubscribe();
			}
			m_Actor = actor;
			if (subscribeToEvents)
			{
				m_Actor.Inventory.ItemAdded += OnItemAdded;
				m_Actor.Inventory.ItemRefreshed += OnItemRefreshed;
				m_Actor.Inventory.ItemUsable += OnItemUseable;
				m_Actor.Inventory.ItemNoLongerUsable += OnItemNoLongerUseable;
				m_Actor.Inventory.ItemSelected += OnItemSelected;
				m_Actor.Inventory.ItemDeSelected += OnItemDeselected;
				m_Actor.Inventory.ItemSpent += OnItemSpent;
				m_Actor.Inventory.ItemConsumed += OnItemConsumed;
				m_Actor.Inventory.ItemUnrestrictedUsed += OnItemUnrestrictedUsed;
				m_Actor.Inventory.ItemActive += OnItemActive;
			}
			RefreshItems();
		}
	}

	private void Unsubscribe()
	{
		m_Actor.Inventory.ItemAdded -= OnItemAdded;
		m_Actor.Inventory.ItemRefreshed -= OnItemRefreshed;
		m_Actor.Inventory.ItemUsable -= OnItemUseable;
		m_Actor.Inventory.ItemNoLongerUsable -= OnItemNoLongerUseable;
		m_Actor.Inventory.ItemSelected -= OnItemSelected;
		m_Actor.Inventory.ItemDeSelected -= OnItemDeselected;
		m_Actor.Inventory.ItemSpent -= OnItemSpent;
		m_Actor.Inventory.ItemConsumed -= OnItemConsumed;
		m_Actor.Inventory.ItemUnrestrictedUsed -= OnItemUnrestrictedUsed;
		m_Actor.Inventory.ItemActive -= OnItemActive;
	}

	public void RefreshItemsState()
	{
		m_Body.RefreshItemState();
		m_Hand1.RefreshItemState();
		m_Hand2.RefreshItemState();
		m_Head.RefreshItemState();
		m_Legs.RefreshItemState();
		UIItemScenario[] smallItems = m_SmallItems;
		for (int i = 0; i < smallItems.Length; i++)
		{
			smallItems[i].RefreshItemState();
		}
		smallItems = m_QuestItems;
		foreach (UIItemScenario uIItemScenario in smallItems)
		{
			uIItemScenario.gameObject.SetActive(uIItemScenario.Item != null);
			uIItemScenario.RefreshItemState();
		}
		if (!m_QuestItems.IsNullOrEmpty())
		{
			RefreshNavigation();
		}
	}

	public void SetUpInteractabilityControls()
	{
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = m_Head.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableControls.Add(interactabilityIsolatedUIControl);
		interactabilityIsolatedUIControl.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
		interactabilityIsolatedUIControl.ControlIdentifier = CItem.EItemSlot.Head.ToString();
		interactabilityIsolatedUIControl.ControlSecondIdentifier = m_Actor.ActorGuid;
		interactabilityIsolatedUIControl.ControlIndex = -1;
		interactabilityIsolatedUIControl.ExtendedButtonsToAllow.Add(m_Head.GetComponent<ExtendedButton>());
		interactabilityIsolatedUIControl.Initialise();
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl2 = m_Body.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableControls.Add(interactabilityIsolatedUIControl2);
		interactabilityIsolatedUIControl2.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
		interactabilityIsolatedUIControl2.ControlIdentifier = CItem.EItemSlot.Body.ToString();
		interactabilityIsolatedUIControl2.ControlSecondIdentifier = m_Actor.ActorGuid;
		interactabilityIsolatedUIControl2.ControlIndex = -1;
		interactabilityIsolatedUIControl2.ExtendedButtonsToAllow.Add(m_Body.GetComponent<ExtendedButton>());
		interactabilityIsolatedUIControl2.Initialise();
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl3 = m_Legs.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableControls.Add(interactabilityIsolatedUIControl3);
		interactabilityIsolatedUIControl3.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
		interactabilityIsolatedUIControl3.ControlIdentifier = CItem.EItemSlot.Legs.ToString();
		interactabilityIsolatedUIControl3.ControlSecondIdentifier = m_Actor.ActorGuid;
		interactabilityIsolatedUIControl3.ControlIndex = -1;
		interactabilityIsolatedUIControl3.ExtendedButtonsToAllow.Add(m_Legs.GetComponent<ExtendedButton>());
		interactabilityIsolatedUIControl3.Initialise();
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl4 = m_Hand1.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableControls.Add(interactabilityIsolatedUIControl4);
		interactabilityIsolatedUIControl4.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
		interactabilityIsolatedUIControl4.ControlIdentifier = CItem.EItemSlot.OneHand.ToString();
		interactabilityIsolatedUIControl4.ControlSecondIdentifier = m_Actor.ActorGuid;
		interactabilityIsolatedUIControl4.ControlIndex = 0;
		interactabilityIsolatedUIControl4.ExtendedButtonsToAllow.Add(m_Hand1.GetComponent<ExtendedButton>());
		interactabilityIsolatedUIControl4.Initialise();
		InteractabilityIsolatedUIControl interactabilityIsolatedUIControl5 = m_Hand2.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
		m_IsolatableControls.Add(interactabilityIsolatedUIControl5);
		interactabilityIsolatedUIControl5.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
		interactabilityIsolatedUIControl5.ControlIdentifier = CItem.EItemSlot.OneHand.ToString();
		interactabilityIsolatedUIControl5.ControlSecondIdentifier = m_Actor.ActorGuid;
		interactabilityIsolatedUIControl5.ControlIndex = 1;
		interactabilityIsolatedUIControl5.ExtendedButtonsToAllow.Add(m_Hand2.GetComponent<ExtendedButton>());
		interactabilityIsolatedUIControl5.Initialise();
		for (int i = 0; i < m_SmallItems.Length; i++)
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl6 = m_SmallItems[i].gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableControls.Add(interactabilityIsolatedUIControl6);
			interactabilityIsolatedUIControl6.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
			interactabilityIsolatedUIControl6.ControlIdentifier = CItem.EItemSlot.SmallItem.ToString();
			interactabilityIsolatedUIControl6.ControlSecondIdentifier = m_Actor.ActorGuid;
			interactabilityIsolatedUIControl6.ControlIndex = i;
			interactabilityIsolatedUIControl6.ExtendedButtonsToAllow.Add(m_SmallItems[i].GetComponent<ExtendedButton>());
			interactabilityIsolatedUIControl6.Initialise();
		}
		for (int j = 0; j < m_QuestItems.Length; j++)
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl7 = m_QuestItems[j].gameObject.AddComponent<InteractabilityIsolatedUIControl>();
			m_IsolatableControls.Add(interactabilityIsolatedUIControl7);
			interactabilityIsolatedUIControl7.ControlType = CLevelUIInteractionProfile.EIsolatedControlType.ItemSlotButton;
			interactabilityIsolatedUIControl7.ControlIdentifier = CItem.EItemSlot.QuestItem.ToString();
			interactabilityIsolatedUIControl7.ControlSecondIdentifier = m_Actor.ActorGuid;
			interactabilityIsolatedUIControl7.ControlIndex = j;
			interactabilityIsolatedUIControl7.ExtendedButtonsToAllow.Add(m_QuestItems[j].GetComponent<ExtendedButton>());
			interactabilityIsolatedUIControl7.Initialise();
		}
	}

	public void ClearOutInteractabilityControls()
	{
		foreach (InteractabilityIsolatedUIControl isolatableControl in m_IsolatableControls)
		{
			UnityEngine.Object.Destroy(isolatableControl);
		}
		m_IsolatableControls.Clear();
	}

	public List<InfuseElement> PickUnselectedInfusionsForItem(CItem item)
	{
		UIItemScenario uIItemScenario = null;
		switch (item.YMLData.Slot)
		{
		case CItem.EItemSlot.Body:
			uIItemScenario = m_Body;
			break;
		case CItem.EItemSlot.Head:
			uIItemScenario = m_Head;
			break;
		case CItem.EItemSlot.Legs:
			uIItemScenario = m_Legs;
			break;
		case CItem.EItemSlot.OneHand:
			if (m_Hand1.Item != null && m_Hand1.Item.ID == item.ID)
			{
				uIItemScenario = m_Hand1;
			}
			else if (m_Hand2.Item != null && m_Hand2.Item.ID == item.ID)
			{
				uIItemScenario = m_Hand2;
			}
			break;
		case CItem.EItemSlot.TwoHand:
			uIItemScenario = m_Hand1;
			break;
		case CItem.EItemSlot.SmallItem:
		{
			for (int j = 0; j < m_SmallItems.Length; j++)
			{
				UIItemScenario uIItemScenario3 = m_SmallItems[j];
				if (uIItemScenario3.Item != null && uIItemScenario3.Item.ID == item.ID)
				{
					uIItemScenario = m_SmallItems[j];
				}
			}
			break;
		}
		case CItem.EItemSlot.QuestItem:
		{
			for (int i = 0; i < m_QuestItems.Length; i++)
			{
				UIItemScenario uIItemScenario2 = m_QuestItems[i];
				if (uIItemScenario2.Item != null && uIItemScenario2.Item.ID == item.ID)
				{
					uIItemScenario = m_QuestItems[i];
				}
			}
			break;
		}
		}
		if (uIItemScenario != null)
		{
			return uIItemScenario.UnselectedInfusions();
		}
		return null;
	}

	public void ResetItemElements(CItem item)
	{
		UIItemScenario uIItemScenario = null;
		switch (item.YMLData.Slot)
		{
		case CItem.EItemSlot.Body:
			uIItemScenario = m_Body;
			break;
		case CItem.EItemSlot.Head:
			uIItemScenario = m_Head;
			break;
		case CItem.EItemSlot.Legs:
			uIItemScenario = m_Legs;
			break;
		case CItem.EItemSlot.OneHand:
			if (m_Hand1.Item != null && m_Hand1.Item.ID == item.ID)
			{
				uIItemScenario = m_Hand1;
			}
			else if (m_Hand2.Item != null && m_Hand2.Item.ID == item.ID)
			{
				uIItemScenario = m_Hand2;
			}
			break;
		case CItem.EItemSlot.TwoHand:
			uIItemScenario = m_Hand1;
			break;
		case CItem.EItemSlot.SmallItem:
		{
			for (int j = 0; j < m_SmallItems.Length; j++)
			{
				UIItemScenario uIItemScenario3 = m_SmallItems[j];
				if (uIItemScenario3.Item != null && uIItemScenario3.Item.ID == item.ID)
				{
					uIItemScenario = m_SmallItems[j];
				}
			}
			break;
		}
		case CItem.EItemSlot.QuestItem:
		{
			for (int i = 0; i < m_QuestItems.Length; i++)
			{
				UIItemScenario uIItemScenario2 = m_QuestItems[i];
				if (uIItemScenario2.Item != null && uIItemScenario2.Item.ID == item.ID)
				{
					uIItemScenario = m_QuestItems[i];
				}
			}
			break;
		}
		}
		if (uIItemScenario != null)
		{
			uIItemScenario.ResetItemElements();
		}
	}

	public UIItemScenario GetUIItemScenario(CItem.EItemSlot slot, string itemGuid)
	{
		UIItemScenario result = null;
		switch (slot)
		{
		case CItem.EItemSlot.None:
			result = null;
			break;
		case CItem.EItemSlot.Head:
			result = m_Head;
			break;
		case CItem.EItemSlot.Body:
			result = m_Body;
			break;
		case CItem.EItemSlot.Legs:
			result = m_Legs;
			break;
		case CItem.EItemSlot.OneHand:
			if (m_Hand1.Item != null && m_Hand1.Item.ItemGuid == itemGuid)
			{
				result = m_Hand1;
			}
			else if (m_Hand2.Item != null && m_Hand2.Item.ItemGuid == itemGuid)
			{
				result = m_Hand2;
			}
			break;
		case CItem.EItemSlot.TwoHand:
			result = m_Hand1;
			break;
		case CItem.EItemSlot.SmallItem:
			result = m_SmallItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == itemGuid);
			break;
		case CItem.EItemSlot.QuestItem:
			result = m_QuestItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == itemGuid);
			break;
		}
		return result;
	}

	public UIItemScenario FindUIItemScenario(string itemGuid)
	{
		if (m_Head != null && m_Head.Item != null && m_Head.Item.ItemGuid == itemGuid)
		{
			return m_Head;
		}
		if (m_Body != null && m_Body.Item != null && m_Body.Item.ItemGuid == itemGuid)
		{
			return m_Body;
		}
		if (m_Legs != null && m_Legs.Item != null && m_Legs.Item.ItemGuid == itemGuid)
		{
			return m_Legs;
		}
		if (m_Hand1 != null && m_Hand1.Item != null && m_Hand1.Item.ItemGuid == itemGuid)
		{
			return m_Hand1;
		}
		if (m_Hand2 != null && m_Hand2.Item != null && m_Hand2.Item.ItemGuid == itemGuid)
		{
			return m_Hand1;
		}
		UIItemScenario uIItemScenario = m_SmallItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == itemGuid);
		if (uIItemScenario != null)
		{
			return uIItemScenario;
		}
		uIItemScenario = m_QuestItems.SingleOrDefault((UIItemScenario x) => x.Item?.ItemGuid == itemGuid);
		if (uIItemScenario != null)
		{
			return uIItemScenario;
		}
		return null;
	}

	public void SetUnfocused(bool unfocused)
	{
		foreach (GameObject unfocusedMask in unfocusedMasks)
		{
			if (unfocused)
			{
				unfocusedMask.transform.SetAsLastSibling();
			}
			unfocusedMask.SetActive(unfocused);
		}
	}

	public void EnableNavigation(Selectable up, Selectable right)
	{
		m_Head.Selectable.SetNavigation(right, null, up);
		m_Body.Selectable.SetNavigation(right);
		m_Legs.Selectable.SetNavigation(right);
		m_Hand1.Selectable.SetNavigation(right);
		m_Hand2.Selectable.SetNavigation(right);
		for (int i = 0; i < m_SmallItems.Length; i++)
		{
			m_SmallItems[i].Selectable.SetNavigation(right);
		}
		for (int j = 0; j < m_QuestItems.Length; j++)
		{
			m_QuestItems[j].Selectable.SetNavigation(right);
		}
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		if (m_Head.Selectable.navigation.mode == Navigation.Mode.None)
		{
			return;
		}
		Selectable selectable = m_Head.Selectable.navigation.selectOnUp;
		if (m_Head.Item != null)
		{
			Selectable nextEquippedItem = GetNextEquippedItem(m_Head.Slot, 0);
			m_Head.Selectable.SetNavigation(null, null, null, (nextEquippedItem == null) ? selectable : nextEquippedItem);
			selectable = m_Head.Selectable;
		}
		if (m_Body.Item != null)
		{
			Selectable nextEquippedItem2 = GetNextEquippedItem(m_Body.Slot, 0);
			Selectable selectable2 = m_Body.Selectable;
			Selectable down = ((nextEquippedItem2 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem2);
			selectable2.SetNavigation(null, null, selectable, down);
			selectable = m_Body.Selectable;
		}
		if (m_Legs.Item != null)
		{
			Selectable nextEquippedItem3 = GetNextEquippedItem(m_Legs.Slot, 0);
			Selectable selectable3 = m_Legs.Selectable;
			Selectable down = ((nextEquippedItem3 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem3);
			selectable3.SetNavigation(null, null, selectable, down);
			selectable = m_Legs.Selectable;
		}
		if (m_Hand1.Item != null)
		{
			Selectable nextEquippedItem4 = GetNextEquippedItem(m_Hand1.Slot, 0);
			Selectable selectable4 = m_Hand1.Selectable;
			Selectable down = ((nextEquippedItem4 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem4);
			selectable4.SetNavigation(null, null, selectable, down);
			selectable = m_Hand1.Selectable;
		}
		if (m_Hand2.Item != null)
		{
			Selectable nextEquippedItem5 = GetNextEquippedItem(m_Hand2.Slot, 0);
			Selectable selectable5 = m_Hand2.Selectable;
			Selectable down = ((nextEquippedItem5 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem5);
			selectable5.SetNavigation(null, null, selectable, down);
			selectable = m_Hand2.Selectable;
		}
		for (int i = 0; i < m_SmallItems.Length; i++)
		{
			if (m_SmallItems[i].Item != null)
			{
				Selectable nextEquippedItem6 = GetNextEquippedItem(m_SmallItems[i].Slot, i);
				Selectable selectable6 = m_SmallItems[i].Selectable;
				Selectable down = ((nextEquippedItem6 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem6);
				selectable6.SetNavigation(null, null, selectable, down);
				selectable = m_SmallItems[i].Selectable;
			}
		}
		for (int j = 0; j < m_QuestItems.Length; j++)
		{
			if (m_QuestItems[j].Item != null)
			{
				Selectable nextEquippedItem7 = GetNextEquippedItem(m_QuestItems[j].Slot, j);
				Selectable selectable7 = m_QuestItems[j].Selectable;
				Selectable down = ((nextEquippedItem7 == null) ? m_Head.Selectable.navigation.selectOnUp : nextEquippedItem7);
				selectable7.SetNavigation(null, null, selectable, down);
				selectable = m_QuestItems[j].Selectable;
			}
		}
	}

	public void DisableNavigation()
	{
		m_Head.Selectable.DisableNavigation();
		m_Body.Selectable.DisableNavigation();
		m_Legs.Selectable.DisableNavigation();
		m_Hand1.Selectable.DisableNavigation();
		m_Hand2.Selectable.DisableNavigation();
		for (int i = 0; i < m_SmallItems.Length; i++)
		{
			m_SmallItems[i].Selectable.DisableNavigation();
		}
		for (int j = 0; j < m_QuestItems.Length; j++)
		{
			m_QuestItems[j].Selectable.DisableNavigation();
		}
	}

	public Selectable GetFirstEquippedItem()
	{
		return GetNextEquippedItem(CItem.EItemSlot.None, 0);
	}

	private Selectable GetNextEquippedItem(CItem.EItemSlot slot, int index)
	{
		if (m_Head.Item != null && slot == CItem.EItemSlot.None)
		{
			return m_Head.Selectable;
		}
		if (m_Body.Item != null && slot <= CItem.EItemSlot.Head)
		{
			return m_Body.Selectable;
		}
		if (m_Legs.Item != null && slot <= CItem.EItemSlot.Body)
		{
			return m_Legs.Selectable;
		}
		if (m_Hand1.Item != null && slot <= CItem.EItemSlot.Legs)
		{
			return m_Hand1.Selectable;
		}
		if (m_Hand2.Item != null && slot <= CItem.EItemSlot.OneHand)
		{
			return m_Hand2.Selectable;
		}
		for (int i = 0; i < m_SmallItems.Length; i++)
		{
			if (m_SmallItems[i].Item != null && (slot <= CItem.EItemSlot.TwoHand || (slot == CItem.EItemSlot.SmallItem && i > index)))
			{
				return m_SmallItems[i].Selectable;
			}
		}
		for (int j = 0; j < m_QuestItems.Length; j++)
		{
			if (m_QuestItems[j].Item != null && (slot <= CItem.EItemSlot.SmallItem || (slot == CItem.EItemSlot.QuestItem && j > index)))
			{
				return m_QuestItems[j].Selectable;
			}
		}
		return null;
	}
}
